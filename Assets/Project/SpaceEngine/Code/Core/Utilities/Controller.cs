using SpaceEngine.Core.Numerics.Vectors;
using SpaceEngine.Debugging;

using System;

using UnityEngine;

namespace SpaceEngine.Core.Utilities
{
    /// <summary>
    /// Controller used to collect user input and move the view (<see cref="TerrainView"/> or <see cref="PlanetView"/>)
    /// Provides smooth interpolation from the views current to new position.
    /// </summary>
    public class Controller : NodeSlave<Controller>
    {
        [SerializeField]
        double MoveSpeed = 1e-3;

        [SerializeField]
        double TurnSpeed = 5e-3;

        [SerializeField]
        double ZoomSpeed = 1.0;

        [SerializeField]
        double ScrollWheelSpeed = 2.0f;

        [SerializeField]
        double RotateSpeed = 0.1;

        [SerializeField]
        double DragSpeed = 0.01;

        /// <summary>
        /// Use exponential damping to go to target positions?
        /// </summary>
        [SerializeField]
        bool Smooth = true;

        bool ScrollIn;
        bool ScrollOut;
        bool NearPressed;
        bool FarPressed;
        bool ForwardPressed;
        bool BackwardPressed;
        bool LeftPressed;
        bool RightPressed;
        bool LeftMousePressed;
        bool RightMousePressed;

        bool Initialized;

        /// <summary>
        /// The target position manipulated by the user via the mouse and keyboard.
        /// </summary>
        [HideInInspector]
        public TerrainView.Position TargetPosition;

        /// <summary>
        /// Start position for an animation between two positions.
        /// </summary>
        TerrainView.Position StartPosition;

        /// <summary>
        /// End position for an animation between two positions.
        /// </summary>
        TerrainView.Position EndPosition;

        Vector3d PreviousMousePos;

        private double AnimationValue = -1.0;

        public TerrainView View { get; private set; }

        private DebugGUISwitcher DebugGUISwitcherInstance { get { return DebugGUISwitcher.Instance as DebugGUISwitcher; } }

        #region NodeSlave<Controller>

        public override void InitNode()
        {
            View = GetComponent<TerrainView>();

            TargetPosition = new TerrainView.Position();
            StartPosition = new TerrainView.Position();
            EndPosition = new TerrainView.Position();
            PreviousMousePos = new Vector3d(Input.mousePosition);
        }

        public override void UpdateNode()
        {
            if (!Initialized)
            {
                GetPosition(TargetPosition);

                Initialized = true;
            }

            KeyDown();
            MouseWheel();
            MouseButtons();
            MouseMotion();

            double dt = Time.deltaTime * 1000.0;

            // If animation requried interpolate from start to end position
            // NOTE : has not been tested and not currently used
            if (AnimationValue >= 0.0)
            {
                AnimationValue = View.Interpolate(StartPosition, EndPosition, AnimationValue);

                if (BrainFuckMath.NearlyEqual(AnimationValue, 1.0))
                {
                    GetPosition(TargetPosition);

                    AnimationValue = -1.0;
                }
            }
            else
            {
                UpdateController(dt);
            }
        }

        #endregion

        private void UpdateController(double dt)
        {
            var dzFactor = Math.Pow(1.02, Math.Min(dt, 1.0));

            if (NearPressed || ScrollOut)
            {
                TargetPosition.Distance = TargetPosition.Distance / (dzFactor * ZoomSpeed) / (ScrollOut ? ScrollWheelSpeed : 1.0f);
            }
            else if (FarPressed || ScrollIn)
            {
                TargetPosition.Distance = TargetPosition.Distance * dzFactor * ZoomSpeed * (ScrollIn ? ScrollWheelSpeed : 1.0f);
            }

            var currentPosition = new TerrainView.Position();

            GetPosition(currentPosition);
            SetPosition(TargetPosition);

            if (ForwardPressed || BackwardPressed)
            {
                var speed = Math.Max(View.GetHeight(), 1.0);

                if (ForwardPressed)
                {
                    View.MoveForward(speed * dt * MoveSpeed);
                }
                else if (BackwardPressed)
                {
                    View.MoveForward(-speed * dt * MoveSpeed);
                }
            }

            if (LeftPressed)
            {
                View.Turn(dt * TurnSpeed);
            }
            else if (RightPressed)
            {
                View.Turn(-dt * TurnSpeed);
            }

            GetPosition(TargetPosition);

            if (Smooth)
            {
                var lerp = 1.0 - Math.Exp(-dt * 2.301e-3);
                var interpolatedPosition = new TerrainView.Position();

                View.InterpolatePos(currentPosition, TargetPosition, lerp, ref interpolatedPosition);

                currentPosition.X = interpolatedPosition.X;
                currentPosition.Y = interpolatedPosition.Y;
                currentPosition.Theta = Mix(currentPosition.Theta, TargetPosition.Theta, lerp);
                currentPosition.Phi = Mix(currentPosition.Phi, TargetPosition.Phi, lerp);
                currentPosition.Distance = Mix(currentPosition.Distance, TargetPosition.Distance, lerp);

                SetPosition(currentPosition);
            }
            else
            {
                SetPosition(TargetPosition);
            }

        }

        private double Mix(double x, double y, double t)
        {
            return BrainFuckMath.NearlyEqual(x, y) ? y : x * (1.0 - t) + y * t;
        }

        private void GetPosition(TerrainView.Position p)
        {
            p.X = View.position.X;
            p.Y = View.position.Y;
            p.Theta = View.position.Theta;
            p.Phi = View.position.Phi;
            p.Distance = View.position.Distance;
        }

        private void SetPosition(TerrainView.Position p)
        {
            View.position.X = p.X;
            View.position.Y = p.Y;
            View.position.Theta = p.Theta;
            View.position.Phi = p.Phi;
            View.position.Distance = p.Distance;

            AnimationValue = -1.0;
        }

        public void GoToPosition(TerrainView.Position p)
        {
            GetPosition(StartPosition);

            EndPosition = p;
            AnimationValue = 0.0;
        }

        public void JumpToPosition(TerrainView.Position p)
        {
            SetPosition(p);

            TargetPosition = p;
        }

        private void KeyDown()
        {
            FarPressed = Input.GetKey(KeyCode.PageDown);
            NearPressed = Input.GetKey(KeyCode.PageUp);

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) return;

            ForwardPressed = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
            BackwardPressed = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
            LeftPressed = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
            RightPressed = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
        }

        private void MouseWheel()
        {
            if (DebugGUISwitcherInstance.MouseOverGUI) return;

            ScrollIn = false;
            ScrollOut = false;

            if (Input.GetAxis("Mouse ScrollWheel") < 0.0f)
            {
                ScrollIn = true;
            }

            if (Input.GetAxis("Mouse ScrollWheel") > 0.0f)
            {
                ScrollOut = true;
            }
        }

        private void MouseButtons()
        {
            if (DebugGUISwitcherInstance.MouseOverGUI) return;

            LeftMousePressed = Input.GetMouseButton(0);
            RightMousePressed = Input.GetMouseButton(1);
        }

        private void MouseMotion()
        {
            if (DebugGUISwitcherInstance.MouseOverGUI) return;

            if (LeftMousePressed && Input.GetKey(KeyCode.LeftControl))
            {
                TargetPosition.Phi -= Input.GetAxis("Mouse X") * RotateSpeed;
                TargetPosition.Theta += Input.GetAxis("Mouse Y") * RotateSpeed;
            }
            else if (LeftMousePressed)
            {
                var oldPosition = View.CameraToWorldMatrix * Input.mousePosition.xy0();
                var position = View.CameraToWorldMatrix * PreviousMousePos.xy0;

                if (!(double.IsNaN(oldPosition.x) || double.IsNaN(oldPosition.y) || double.IsNaN(oldPosition.z) || double.IsNaN(position.x) || double.IsNaN(position.y) || double.IsNaN(position.z)))
                {
                    var currentPosition = new TerrainView.Position();

                    GetPosition(currentPosition);
                    SetPosition(TargetPosition);

                    View.Move(new Vector3d(oldPosition), new Vector3d(position), DragSpeed);

                    GetPosition(TargetPosition);
                    SetPosition(currentPosition);
                }
            }
            else if (RightMousePressed)
            {

            }

            PreviousMousePos = new Vector3d(Input.mousePosition);
        }
    }
}