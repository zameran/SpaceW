using SpaceEngine.Core.Numerics;
using SpaceEngine.Core.Numerics.Matrices;
using SpaceEngine.Core.Numerics.Shapes;
using SpaceEngine.Core.Numerics.Vectors;
using SpaceEngine.Core.Utilities;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;

#pragma warning disable 0219, 0414

namespace SpaceEngine.Core.Preprocess.Forest
{
    // TODO : Make all this stuff in Houdini...
    public class PreProcessTree : MonoBehaviour
    {
        public int GRIDRES_VIEWS = 256;
        public int N_VIEWS = 9;

        public Mesh[] TreeMeshes;

        public Shader ViewShader;
        public Material ViewMaterial;

        public Texture2D TreeSampler;

        public string DestinationFolder = "/Resources/Preprocess/Forest/";

        public double Z = 1.0;
        public double S = 1.0;

        private Mesh PreProcessMesh;
        private RenderTexture PreProcessAORT;

        private string ApplicationDataPath = "";

        private void Start()
        {
            var startTime = Time.realtimeSinceStartup;

            ApplicationDataPath = Application.dataPath;

            ViewMaterial = MaterialHelper.CreateTemp(ViewShader, "TreeView");

            CalculateMesh();
            //CalculateAO();
            CalculateViews();

            Debug.Log(string.Format("PreProcessTree.Start: Computation time: {0} s", (Time.realtimeSinceStartup - startTime)));
        }

        private void Update()
        {

        }

        private void OnDestroy()
        {
            Helper.Destroy(ViewMaterial);
            Helper.Destroy(PreProcessMesh);

            if (PreProcessAORT != null) PreProcessAORT.ReleaseAndDestroy();
        }

        private void Swap(ref int a, ref int b)
        {
            var c = a;

            a = b;
            b = c;
        }

        private void Swap(ref float a, ref float b)
        {
            var c = a;

            a = b;
            b = c;
        }

        private void Swap(ref double a, ref double b)
        {
            var c = a;

            a = b;
            b = c;
        }

        private void CalculateMesh()
        {
            var CIs = new List<CombineInstance>();

            foreach (var treeMesh in TreeMeshes)
            {
                var ci = new CombineInstance()
                {
                    mesh = treeMesh,
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one)
                };

                CIs.Add(ci);
            }

            PreProcessMesh = new Mesh();
            PreProcessMesh.CombineMeshes(CIs.ToArray());
            PreProcessMesh.RecalculateBounds();
            PreProcessMesh.RecalculateNormals();
            PreProcessMesh.RecalculateTangents();
            PreProcessMesh.hideFlags = HideFlags.DontSave;

            CIs.Clear();
        }

        [Obsolete("Use vertex baked AO instead!")]
        private void CalculateAO()
        {
            Debug.Log("Precomputing AO Started...");

            int GRIDRES_AO = 128;
            int N_AO = 2;

            var options = new ParallelOptions { MaxDegreeOfParallelism = 4 };

            float[] buf = new float[GRIDRES_AO * GRIDRES_AO * GRIDRES_AO * 4];

            for (int i = 0; i < GRIDRES_AO; ++i)
            {
                for (int j = 0; j < GRIDRES_AO; ++j)
                {
                    for (int k = 0; k < GRIDRES_AO; ++k)
                    {
                        int off = i + j * GRIDRES_AO + k * GRIDRES_AO * GRIDRES_AO;

                        buf[4 * off] = 0;
                        buf[4 * off + 1] = 0;
                        buf[4 * off + 2] = 0;
                        buf[4 * off + 3] = 0;
                    }
                }
            }

            var indices = PreProcessMesh.GetIndices(0);
            var vertices = PreProcessMesh.vertices;

            for (int ni = 0; ni < indices.Length; ni += 3)
            {
                int a = indices[ni];
                int b = indices[ni + 1];
                int c = indices[ni + 2];

                float x1 = vertices[a].x, y1 = vertices[a].y, z1 = vertices[a].z;
                float x2 = vertices[b].x, y2 = vertices[b].y, z2 = vertices[b].z;
                float x3 = vertices[c].x, y3 = vertices[c].y, z3 = vertices[c].z;

                x1 = (x1 + 1.0f) / 2.0f;
                x2 = (x2 + 1.0f) / 2.0f;
                x3 = (x3 + 1.0f) / 2.0f;
                y1 = (y1 + 1.0f) / 2.0f;
                y2 = (y2 + 1.0f) / 2.0f;
                y3 = (y3 + 1.0f) / 2.0f;
                z1 = (z1 + 1.0f) / 2.0f;
                z2 = (z2 + 1.0f) / 2.0f;
                z3 = (z3 + 1.0f) / 2.0f;

                double l12 = Mathf.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1) + (z2 - z1) * (z2 - z1));
                double l23 = Mathf.Sqrt((x3 - x2) * (x3 - x2) + (y3 - y2) * (y3 - y2) + (z3 - z2) * (z3 - z2));
                double l31 = Mathf.Sqrt((x1 - x3) * (x1 - x3) + (y1 - y3) * (y1 - y3) + (z1 - z3) * (z1 - z3));

                if (l12 > l23 && l12 > l31)
                {
                    Swap(ref a, ref c);
                    Swap(ref x1, ref x3); Swap(ref y1, ref y3); Swap(ref z1, ref z3);
                    Swap(ref l12, ref l23);
                }
                else if (l31 > l12 && l31 > l23)
                {
                    Swap(ref a, ref b);
                    Swap(ref x1, ref x2); Swap(ref y1, ref y2); Swap(ref z1, ref z2);
                    Swap(ref l31, ref l23);
                }

                int n12 = (int)(Math.Ceiling(l12 * GRIDRES_AO) * 2.0);
                int n13 = (int)(Math.Ceiling(l31 * GRIDRES_AO) * 2.0);

                Parallel.For(0, n12 - 1, i =>
                {
                    var u = (double)i / n12;

                    Parallel.For(0, n13 - 1, j =>
                    {
                        var v = (double)j / n13;

                        if (u + v < 1.0)
                        {
                            var x = x1 + u * (x2 - x1) + v * (x3 - x1);
                            var y = y1 + u * (y2 - y1) + v * (y3 - y1);
                            var z = z1 + u * (z2 - z1) + v * (z3 - z1);

                            int ix = (int)(x * GRIDRES_AO);
                            int iy = (int)(y * GRIDRES_AO);
                            int iz = (int)(z * GRIDRES_AO);

                            if (ix >= 0 && ix < GRIDRES_AO && iy >= 0 && iy < GRIDRES_AO && iz >= 0 && iz < GRIDRES_AO)
                            {
                                int off = 4 * (ix + iy * GRIDRES_AO + iz * GRIDRES_AO * GRIDRES_AO);

                                buf[off] = 255;
                                buf[off + 1] = 255;
                                buf[off + 2] = 255;
                                buf[off + 3] = 255;
                            }
                        }
                    });
                });
            }

            Debug.Log("Precomputing AO Mesh Passed...");

            double[] vocc = new double[GRIDRES_AO * GRIDRES_AO * GRIDRES_AO];

            for (int i = 0; i < GRIDRES_AO * GRIDRES_AO * GRIDRES_AO; ++i)
            {
                vocc[i] = 1.0;
            }

            double zmax = Math.Abs(Z);
            double zmin = -Math.Abs(Z);

            Parallel.For(0, N_AO - 1, options, i =>
            {
                var theta = (i + 0.5) / N_AO * Math.PI / 2.0;
                var dtheta = 1.0 / N_AO * Math.PI / 2.0;

                Parallel.For(0, (4 * N_AO) - 1, options, j =>
                {
                    var phi = (j + 0.5) / (4 * N_AO) * 2.0 * Math.PI;
                    var dphi = 1.0 / (4 * N_AO) * 2.0 * Math.PI;
                    var docc = Math.Cos(theta) * Math.Sin(theta) * dtheta * dphi / Math.PI;

                    if ((i * 4 * N_AO + j) % 4 == 0) Debug.Log(string.Format("Precomputing AO Step {0} of {1}", i * 4 * N_AO + j, 4 * N_AO * N_AO));

                    Vector3d uz = new Vector3d(Math.Cos(phi) * Math.Sin(theta), Math.Sin(phi) * Math.Sin(theta), Math.Cos(theta));
                    Vector3d ux = uz.z.EpsilonEquals(1.0, 0.0000001) ? new Vector3d(1.0, 0.0, 0.0) : new Vector3d(-uz.y, uz.x, 0.0).Normalized();
                    Vector3d uy = uz.Cross(ux);

                    Matrix3x3d toView = new Matrix3x3d(ux.x, ux.y, ux.z, uy.x, uy.y, uy.z, uz.x, uz.y, uz.z);
                    Matrix3x3d toVol = new Matrix3x3d(ux.x, uy.x, uz.x, ux.y, uy.y, uz.y, ux.z, uy.z, uz.z);

                    Box3d b = new Box3d();
                    b = b.Enlarge(toView * new Vector3d(-1.0, -1.0, zmin));
                    b = b.Enlarge(toView * new Vector3d(+1.0, -1.0, zmin));
                    b = b.Enlarge(toView * new Vector3d(-1.0, +1.0, zmin));
                    b = b.Enlarge(toView * new Vector3d(+1.0, +1.0, zmin));
                    b = b.Enlarge(toView * new Vector3d(-1.0, -1.0, zmax));
                    b = b.Enlarge(toView * new Vector3d(+1.0, -1.0, zmax));
                    b = b.Enlarge(toView * new Vector3d(-1.0, +1.0, zmax));
                    b = b.Enlarge(toView * new Vector3d(+1.0, +1.0, zmax));

                    int nx = (int)((b.Max.x - b.Min.x) * GRIDRES_AO / 2);
                    int ny = (int)((b.Max.y - b.Min.y) * GRIDRES_AO / 2);
                    int nz = (int)((b.Max.z - b.Min.z) * GRIDRES_AO / 2);

                    int[] occ = new int[nx * ny * nz];
                    for (int v = 0; v < nx * ny * nz; ++v) { occ[v] = 0; }

                    for (int iz = nz - 1; iz >= 0; --iz)
                    {
                        var z = b.Min.z + (iz + 0.5) / nz * (b.Max.z - b.Min.z);

                        for (int iy = 0; iy < ny; ++iy)
                        {
                            var y = b.Min.y + (iy + 0.5) / ny * (b.Max.y - b.Min.y);

                            for (int ix = 0; ix < nx; ++ix)
                            {
                                var x = b.Min.x + (ix + 0.5) / nx * (b.Max.x - b.Min.x);

                                Vector3d p = toVol * new Vector3d(x, y, z);

                                int val = 0;
                                int vx = (int)((p.x + 1.0) / 2.0 * GRIDRES_AO);
                                int vy = (int)((p.y + 1.0) / 2.0 * GRIDRES_AO);
                                int vz = (int)((p.z + 1.0) / 2.0 * GRIDRES_AO);

                                if (vx >= 0 && vx < GRIDRES_AO && vy >= 0 && vy < GRIDRES_AO && vz >= 0 && vz < GRIDRES_AO)
                                {
                                    val = buf[4 * (vx + vy * GRIDRES_AO + vz * GRIDRES_AO * GRIDRES_AO) + 3].EpsilonEquals(255.0f) ? 1 : 0;
                                }

                                occ[ix + iy * nx + iz * nx * ny] = val;

                                if (iz != nz - 1)
                                {
                                    occ[ix + iy * nx + iz * nx * ny] += occ[ix + iy * nx + (iz + 1) * nx * ny];
                                }
                            }
                        }
                    }

                    Parallel.For(0, GRIDRES_AO - 1, options, ix =>
                    {
                        var x = -1.0 + (ix + 0.5) / GRIDRES_AO * 2.0;

                        Parallel.For(0, GRIDRES_AO - 1, options, iy =>
                        {
                            var y = -1.0 + (iy + 0.5) / GRIDRES_AO * 2.0;

                            Parallel.For(0, GRIDRES_AO - 1, options, iz =>
                            {
                                var z = -1.0 + (iz + 0.5) / GRIDRES_AO * 2.0;

                                Vector3d p = toView * new Vector3d(x, y, z);

                                int vx = (int)((p.x - b.Min.x) / (b.Max.x - b.Min.x) * nx);
                                int vy = (int)((p.y - b.Min.y) / (b.Max.y - b.Min.y) * ny);
                                int vz = (int)((p.z - b.Min.z) / (b.Max.z - b.Min.z) * nz);

                                if (vx >= 0 && vx < nx && vy >= 0 && vy < ny && vz >= 0 && vz < nz)
                                {
                                    int occN = occ[vx + vy * nx + vz * nx * ny];

                                    if (occN > 6)
                                    {
                                        vocc[ix + iy * GRIDRES_AO + iz * GRIDRES_AO * GRIDRES_AO] -= docc;
                                    }
                                }
                            });
                        });
                    });
                });
            });

            for (int i = 0; i < GRIDRES_AO; ++i)
            {
                for (int j = 0; j < GRIDRES_AO; ++j)
                {
                    for (int k = 0; k < GRIDRES_AO; ++k)
                    {
                        int off = i + j * GRIDRES_AO + k * GRIDRES_AO * GRIDRES_AO;

                        if (buf[4 * off + 3].EpsilonEquals(255.0f))
                        {
                            var v = Math.Max(vocc[off], 0.0f) * 255;

                            buf[4 * off] = (float)v;
                            buf[4 * off + 1] = (float)v;
                            buf[4 * off + 2] = (float)v;
                        }
                    }
                }
            }

            GC.Collect();

            var cb = new ComputeBuffer(GRIDRES_AO * GRIDRES_AO * GRIDRES_AO, sizeof(float) * 4);

            PreProcessAORT = RTExtensions.CreateRTexture(GRIDRES_AO, 0, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, TextureWrapMode.Clamp, GRIDRES_AO);

            CBUtility.WriteIntoRenderTexture(PreProcessAORT, CBUtility.Channels.RGBA, cb, GodManager.Instance.WriteData);
            RTUtility.SaveAs8bit(GRIDRES_AO, GRIDRES_AO * GRIDRES_AO, CBUtility.Channels.RGBA, "TreeAO", DestinationFolder, buf, 0.00392156863f);

            cb.ReleaseAndDisposeBuffer();

            Debug.Log("Precomputing AO Completed!");
        }

        private IEnumerator CalculateViewsRoutine(Action<List<string>, Dictionary<int, Texture2D>> callback)
        {
            yield return Yielders.EndOfFrame; // Finish previous frame...

            List<string> viewsLines = new List<string>();
            Dictionary<int, Texture2D> viewsBilboards = new Dictionary<int, Texture2D>();

            int total = 2 * (N_VIEWS * N_VIEWS + N_VIEWS) + 1;
            int current = 0;

            double zmax = Math.Abs(Z);
            double zmin = -Math.Abs(Z);

            for (int i = -N_VIEWS; i <= N_VIEWS; i++)
            {
                for (int j = -N_VIEWS + Math.Abs(i); j <= N_VIEWS - Math.Abs(i); ++j)
                {
                    double x = (i + j) / (double)N_VIEWS;
                    double y = (j - i) / (double)N_VIEWS;
                    double angle = 90.0 - Math.Max(Math.Abs(x), Math.Abs(y)) * 90.0;
                    double alpha = x.EpsilonEquals(0.0, 0.00000001) && y.EpsilonEquals(0.0, 0.00000001) ? 0.0f : Math.Atan2(y, x) / Math.PI * 180.0;

                    Matrix4x4d cameraToWorld = Matrix4x4d.RotateX(90) * Matrix4x4d.RotateX(angle);
                    Matrix4x4d worldToCamera = cameraToWorld.Inverse();

                    Box3d b = new Box3d();
                    b = b.Enlarge((worldToCamera * new Vector4d(-1.0, -1.0, zmin, 1.0)).xyz);
                    b = b.Enlarge((worldToCamera * new Vector4d(+1.0, -1.0, zmin, 1.0)).xyz);
                    b = b.Enlarge((worldToCamera * new Vector4d(-1.0, +1.0, zmin, 1.0)).xyz);
                    b = b.Enlarge((worldToCamera * new Vector4d(+1.0, +1.0, zmin, 1.0)).xyz);
                    b = b.Enlarge((worldToCamera * new Vector4d(-1.0, -1.0, zmax, 1.0)).xyz);
                    b = b.Enlarge((worldToCamera * new Vector4d(+1.0, -1.0, zmax, 1.0)).xyz);
                    b = b.Enlarge((worldToCamera * new Vector4d(-1.0, +1.0, zmax, 1.0)).xyz);
                    b = b.Enlarge((worldToCamera * new Vector4d(+1.0, +1.0, zmax, 1.0)).xyz);

                    Matrix4x4d c2s = Matrix4x4d.Ortho(b.Max.x, b.Min.x, b.Max.y, b.Min.y, -2.0 * b.Max.z, -2.0 * b.Min.z + S);
                    Matrix4x4d w2s = c2s * worldToCamera * Matrix4x4d.RotateZ(-90 - alpha);

                    Vector3d dir = ((Matrix4x4d.RotateZ(90 + alpha) * cameraToWorld) * new Vector4d(0.0, 0.0, 1.0, 0.0)).xyz;

                    ViewMaterial.SetTexture("colorSampler", TreeSampler);
                    ViewMaterial.SetVector("dir", dir.ToVector3());
                    ViewMaterial.SetMatrix("worldToScreen", w2s.ToMatrix4x4());

                    Camera.main.projectionMatrix = w2s.ToMatrix4x4();

                    ViewMaterial.SetPass(0);
                    Graphics.DrawMeshNow(PreProcessMesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one));

                    ViewMaterial.SetPass(1);
                    Graphics.DrawMeshNow(PreProcessMesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one));

                    yield return Yielders.EndOfFrame; // Finish this frame...

                    var texture = new Texture2D(GRIDRES_VIEWS, GRIDRES_VIEWS, TextureFormat.RGBAFloat, false, true);

                    texture.ReadPixels(new Rect(0, 0, GRIDRES_VIEWS, GRIDRES_VIEWS), 0, 0, false);
                    texture.Apply(false);

                    viewsBilboards.Add(current, texture);

                    var view = i * (1 - Math.Abs(i)) + j + 2 * N_VIEWS * i + N_VIEWS * (N_VIEWS + 1);

                    RTUtility.ClearColor(RenderTexture.active);

                    current++;

                    Debug.Log(string.Format("Precomputing Views Step {0} of {1} : View {2}", current, total, view));

                    viewsLines.Add(string.Format("{0}f,{1}f,{2}f,{3}f,{4}f,{5}f,{6}f,{7}f,{8}f,", (float)w2s.m[0, 0], (float)w2s.m[0, 1], (float)w2s.m[0, 2],
                                                                                                  (float)w2s.m[1, 0], (float)w2s.m[1, 1], (float)w2s.m[1, 2],
                                                                                                  (float)w2s.m[2, 0], (float)w2s.m[2, 1], (float)w2s.m[2, 2]));
                }
            }

            if (callback != null) callback(viewsLines, viewsBilboards);
        }

        private void CalculateViews()
        {
            Debug.Log("Precomputing Views Started...");

            StreamWriter file = new StreamWriter(ApplicationDataPath + "/Resources/Preprocess/Forest/Views.txt");

            StartCoroutine(CalculateViewsRoutine((lines, billboards) =>
            {
                foreach (var line in lines)
                {
                    file.WriteLine(line);
                }

                foreach (var billboard in billboards)
                {
                    File.WriteAllBytes(ApplicationDataPath + DestinationFolder + string.Format("TestTree/{0}-{1}", "Trees3D", billboard.Key) + ".png", billboard.Value.EncodeToPNG());
                }

                file.Flush();
                file.Close();

                Debug.Log("Precomputing Views Completed!");
            }));
        }
    }
}