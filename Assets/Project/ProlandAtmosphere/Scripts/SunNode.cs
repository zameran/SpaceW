using UnityEngine;

namespace Proland
{
    public class SunNode : MonoBehaviour
    {
        //Dont change this
        static readonly Vector3 Z_AXIS = new Vector3(0, 0, 1);

        [SerializeField]
        Vector3 startSunDirection = Z_AXIS;

        [SerializeField]
        float sunIntensity = 100.0f;

        Matrix4x4 worldToLocalRotation;

        bool hasMoved = false;

        public Vector3 GetDirection()
        {
            return transform.forward;
        }

        public bool GetHasMoved()
        {
            return hasMoved;
        }

        //The rotation needed to move the sun direction back to the z axis.
        //The sky shader requires that the sun direction is always at the z axis
        public Matrix4x4 GetWorldToLocalRotation()
        {
            return worldToLocalRotation;
        }

        //The rotation needed to move the sun direction from the z axis 
        public Matrix4x4 GetLocalToWorldRotation()
        {
            return worldToLocalRotation.inverse;
        }

        void Start()
        {
            //if the sun direction entered is (0,0,0) which is not valid, change to default
            if (startSunDirection.magnitude < Mathf.Epsilon)
                startSunDirection = Z_AXIS;

            transform.forward = startSunDirection.normalized;
        }

        public void SetUniforms(Material mat)
        {
            if (mat == null) return;

            mat.SetFloat("_Sun_Intensity", sunIntensity);
            mat.SetVector("_Sun_WorldSunDir", GetDirection());
        }

        public void UpdateNode()
        {
            hasMoved = false;

            if (Input.GetMouseButton(2))
            {
                float y = Input.GetAxis("Mouse Y");
                float x = -Input.GetAxis("Mouse X");
                transform.Rotate(new Vector3(x, y, 0), Space.Self);
                hasMoved = true;
            }

            Quaternion q = Quaternion.FromToRotation(GetDirection(), Z_AXIS);
            worldToLocalRotation = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
        }
    }
}