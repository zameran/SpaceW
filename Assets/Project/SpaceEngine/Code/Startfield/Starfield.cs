using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using UnityEngine;

namespace SpaceEngine.Startfield
{
    public class Starfield : MonoBehaviour
    {
        public float StarIntensity = 1.0f;
        public float StarsScale = 1.0f;
        public float StarsDistance = 1000.0f;

        public Shader StarfieldShader;
        public Material StarfieldMaterial;

        public Mesh BillboardMesh;
        public Mesh StarfieldMesh;

        public EngineRenderQueue RenderQueue = EngineRenderQueue.Background;
        public int RenderQueueOffset = 0;

        private void Start()
        {
            InitMaterials();
            InitMesh();
        }

        public void Render(int drawLayer = 8)
        {
            Render(CameraHelper.Main(), drawLayer);
        }

        public void Render(Camera camera, int drawLayer = 8)
        {
            if (StarfieldMesh != null && StarfieldMaterial != null)
            {
                SetUniforms(StarfieldMaterial);

                StarfieldMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

                //GetComponent<MeshFilter>().sharedMesh = StarfieldMesh;
                //GetComponent<MeshRenderer>().sharedMaterial = StarfieldMaterial;

                Graphics.DrawMesh(StarfieldMesh, transform.localToWorldMatrix, StarfieldMaterial, drawLayer, CameraHelper.Main(), 0, null, false, false);
            }
        }

        [ContextMenu("InitMesh")]
        public void InitMesh()
        {
            float starSize = StarsDistance / 100 * StarsScale;

            BillboardMesh = MeshFactory.MakeBillboardQuad(starSize);
            StarfieldMesh = CreateStarfieldMesh(StarsDistance);
        }

        public void InitMaterials()
        {
            if (StarfieldMaterial == null)
            {
                StarfieldMaterial = MaterialHelper.CreateTemp(StarfieldShader, "Starfield");
            }
        }

        public void SetUniforms(Material mat)
        {
            if (mat == null) return;

            mat.SetFloat("_StarIntensity", StarIntensity);
            mat.SetMatrix("_RotationMatrix", Matrix4x4.identity);
        }

        private Mesh CreateStarfieldMesh(float starDistance)
        {
            const int numberOfStars = 9110;       

            var dataFile = Resources.Load("Json/Stars", typeof(TextAsset)) as TextAsset;
            var stars = new List<StarfieldStar>(numberOfStars);
            var starsData = JsonConvert.DeserializeObject<StarfieldStarJson[]>(dataFile.text).ToList();
            var starsCIs = new List<CombineInstance>();

            starsData.ForEach((sd) => stars.Add(new StarfieldStar(sd)));
            stars.ForEach((star) =>
            {
                float magnitude = Vector3.Dot(new Vector3(star.Color.r, star.Color.g, star.Color.b), new Vector3(0.22f, 0.707f, 0.071f));

                star.Color.a = magnitude;
                star.Position = Vector3.Scale(star.Position, new Vector3(-1.0f, 1.0f, -1.0f));

                CombineInstance ci = new CombineInstance();

                ci.mesh = BillboardMesh;
                ci.mesh.colors = new[] {star.Color, star.Color, star.Color, star.Color};
                ci.transform = MatrixHelper.BillboardMatrix(star.Position * starDistance);

                starsCIs.Add(ci);
            });

            var mesh = new Mesh();
            mesh.name = string.Format("StarfieldMesh_({0})", Random.Range(float.MinValue, float.MaxValue));
            mesh.CombineMeshes(starsCIs.ToArray());
            mesh.Optimize();
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));

            return mesh;
        }
    }
}