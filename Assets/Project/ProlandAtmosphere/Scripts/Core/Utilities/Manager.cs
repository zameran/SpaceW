using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Proland
{
    /*
     * A manger to organise what order update functions are called, the running of tasks and the drawing of the terrain.
     * Provides a location for common settings and allows the nodes to access each other.
     * Also sets uniforms that are considered global.
     * Must have a scheduler script attached to the same gameobject
     * 
     */
    public class Manager : MonoBehaviour
    {
        public enum DEFORM { PLANE, SPHERE };

        [SerializeField]
        ComputeShader m_writeData;

        [SerializeField]
        ComputeShader m_readData;

        [SerializeField]
        int m_gridResolution = 25;

        [SerializeField]
        float m_HDRExposure = 0.2f;

        //If the world is a flat plane or a sphere
        [SerializeField]
        DEFORM m_deformType = DEFORM.PLANE;

        [SerializeField]
        float m_radius = 6360000.0f;

        SkyNode m_skyNode;
        SunNode m_sunNode;
        Controller m_controller;
        Vector3 m_origin;

        public int GetGridResolution()
        {
            return m_gridResolution;
        }

        public bool IsDeformed()
        {
            return (m_deformType == DEFORM.SPHERE);
        }

        public float GetRadius()
        {
            return m_radius;
        }

        public ComputeShader GetWriteData()
        {
            return m_writeData;
        }

        public ComputeShader GetReadData()
        {
            return m_readData;
        }

        public SkyNode GetSkyNode()
        {
            return m_skyNode;
        }

        public SunNode GetSunNode()
        {
            return m_sunNode;
        }

        public Controller GetController()
        {
            return m_controller;
        }

        // Use this for initialization
        void Awake()
        {
            if (IsDeformed())
                m_origin = Vector3.zero;
            else
                m_origin = new Vector3(0.0f, 0.0f, m_radius);

            m_controller = GetComponentInChildren<Controller>();

            //if planet view is being use set the radius
            if (m_controller.GetView() is PlanetView)
                ((PlanetView)m_controller.GetView()).SetRadius(m_radius);

            //Get the nodes that are children of the manager
            m_skyNode = GetComponentInChildren<SkyNode>();
            m_sunNode = GetComponentInChildren<SunNode>();
        }

        public void SetUniforms(Material mat)
        {
            //Sets uniforms that this or other gameobjects may need
            if (mat == null) return;

            mat.SetMatrix("_Globals_WorldToCamera", m_controller.GetView().GetWorldToCamera().ToMatrix4x4());
            mat.SetMatrix("_Globals_CameraToWorld", m_controller.GetView().GetCameraToWorld().ToMatrix4x4());
            mat.SetMatrix("_Globals_CameraToScreen", m_controller.GetView().GetCameraToScreen().ToMatrix4x4());
            mat.SetMatrix("_Globals_ScreenToCamera", m_controller.GetView().GetScreenToCamera().ToMatrix4x4());
            mat.SetVector("_Globals_WorldCameraPos", m_controller.GetView().GetWorldCameraPos().ToVector3());
            mat.SetVector("_Globals_Origin", m_origin);
            mat.SetFloat("_Exposure", m_HDRExposure);

        }

        // Update is called once per frame
        void Update()
        {
            //Update the sky, sun and controller. These node are presumed to always be present
            m_controller.UpdateController();
            m_sunNode.UpdateNode();
            m_skyNode.UpdateNode();
        }
    }
}