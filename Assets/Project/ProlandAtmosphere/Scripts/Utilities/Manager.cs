using UnityEngine;

namespace Proland
{
    public class Manager : MonoBehaviour
    {
        public Camera ruleCamera;

        [SerializeField]
        ComputeShader writeDataShader;

        [SerializeField]
        ComputeShader readDataShader;

        [SerializeField]
        float HDRExposure = 0.2f;

        [SerializeField]
        float radius = 6360000.0f;

        SkyNode skyNode;
        SunNode sunNode;

        public Vector3 origin;

        public float GetRadius()
        {
            return radius;
        }

        public ComputeShader GetWriteData()
        {
            return writeDataShader;
        }

        public ComputeShader GetReadData()
        {
            return readDataShader;
        }

        public SkyNode GetSkyNode()
        {
            return skyNode;
        }

        public SunNode GetSunNode()
        {
            return sunNode;
        }

        void Awake()
        {
            origin = Vector3.zero;

            //Get the nodes that are children of the manager
            skyNode = GetComponentInChildren<SkyNode>();
            sunNode = GetComponentInChildren<SunNode>();
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

            mat.SetVector("_Globals_Origin", -origin);
            mat.SetFloat("_Exposure", HDRExposure);
        }

        void Update()
        {
            sunNode.UpdateNode();
            skyNode.UpdateNode();
        }
    }
}