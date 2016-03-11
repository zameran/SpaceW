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
        public Camera ruleCamera;

        [SerializeField]
        ComputeShader m_writeData;

        [SerializeField]
        ComputeShader m_readData;

        [SerializeField]
        float m_HDRExposure = 0.2f;

        [SerializeField]
        float m_radius = 6360000.0f;

        SkyNode m_skyNode;
        SunNode m_sunNode;

        Vector3 m_origin;

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

        void Awake()
        {
            m_origin = Vector3.zero;

            //Get the nodes that are children of the manager
            m_skyNode = GetComponentInChildren<SkyNode>();
            m_sunNode = GetComponentInChildren<SunNode>();
        }

        public void SetUniforms(Material mat)
        {
            //Sets uniforms that this or other gameobjects may need
            if (mat == null) return;

            mat.SetMatrix("_Globals_WorldToCamera", ruleCamera.GetWorldToCamera());
            mat.SetMatrix("_Globals_CameraToWorld", ruleCamera.GetCameraToWorld());
            mat.SetMatrix("_Globals_CameraToScreen", ruleCamera.GetCameraToScreen());
            mat.SetMatrix("_Globals_ScreenToCamera", ruleCamera.GetScreenToCamera());
            mat.SetVector("_Globals_WorldCameraPos", ruleCamera.transform.position);

            mat.SetVector("_Globals_Origin", m_origin);
            mat.SetFloat("_Exposure", m_HDRExposure);
        }

        void Update()
        {
            m_origin = this.transform.position;

            m_sunNode.UpdateNode();
            m_skyNode.UpdateNode();
        }
    }
}