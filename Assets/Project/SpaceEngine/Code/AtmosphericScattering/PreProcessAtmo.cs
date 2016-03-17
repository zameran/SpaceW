#pragma warning disable 162

using UnityEngine;

namespace Proland
{
    public class PreProcessAtmo : MonoBehaviour
    {
        //Dont change these
        const int NUM_THREADS = 8;
        const int READ = 0;
        const int WRITE = 1;

        //You can change these
        //The radius of the planet (Rg), radius of the atmosphere (Rt)
        public float Rg = 6360.0f;
        public float Rt = 6420.0f;
        public float RL = 6421.0f;
        //public float Rg = 6360.0f; // default;
        //public float Rt = 6420.0f; // default;
        //public float RL = 6421.0f; // default;

        //Dimensions of the tables
        const int TRANSMITTANCE_W = 256;
        const int TRANSMITTANCE_H = 64;
        const int SKY_W = 64;
        const int SKY_H = 16;
        const int RES_R = 32;
        const int RES_MU = 128;
        const int RES_MU_S = 32;
        const int RES_NU = 8;

        //Physical settings, Mie and Rayliegh values
        const float AVERAGE_GROUND_REFLECTANCE = 0.1f;
        public Vector4 BETA_R = new Vector4(5.8e-3f, 1.35e-2f, 3.31e-2f, 0.0f);
        public Vector4 BETA_MSca = new Vector4(4e-6f, 4e-6f, 4e-6f, 0.0f);
        //public Vector4 BETA_R = new Vector4(5.8e-3f, 1.35e-2f, 3.31e-2f, 0.0f); // default;
        //public Vector4 BETA_MSca = new Vector4(4e-6f, 4e-6f, 4e-6f, 0.0f); // default;

        //Asymmetry factor for the mie phase function
        //A higher number meands more light is scattered in the forward direction
        public float MIE_G = 0.8f;
        //public float MIE_G = 0.8f; // default;

        //Half heights for the atmosphere air density (HR) and particle density (HM)
        //This is the height in km that half the particles are found below
        public float HR = 8.0f;
        public float HM = 1.2f;
        //public float HR = 8.0f; // default;
        //public float HM = 1.2f; // default;

        RenderTexture transmittanceT;
        RenderTexture deltaET, deltaSRT, deltaSMT, deltaJT;
        RenderTexture[] irradianceT, inscatterT;

        //This is where the tables will be saved to
        public string texturesPath = "/Resources/Textures/Atmosphere/";

        public ComputeShader copyInscatter1, copyInscatterN, copyIrradiance;
        public ComputeShader inscatter1, inscatterN, inscatterS;
        public ComputeShader irradiance1, irradianceN, transmittance;
        public ComputeShader readData;

        int step, order;
        bool finished = false;

        const bool WRITE_DEBUG_TEX = false;

        void Start()
        {
            irradianceT = new RenderTexture[2];
            inscatterT = new RenderTexture[2];

            transmittanceT = new RenderTexture(TRANSMITTANCE_W, TRANSMITTANCE_H, 0, RenderTextureFormat.ARGBFloat);
            transmittanceT.enableRandomWrite = true;
            transmittanceT.Create();
            Debug.Log("1");
            irradianceT[0] = new RenderTexture(SKY_W, SKY_H, 0, RenderTextureFormat.ARGBFloat);
            irradianceT[0].enableRandomWrite = true;
            irradianceT[0].Create();
            Debug.Log("2");
            irradianceT[1] = new RenderTexture(SKY_W, SKY_H, 0, RenderTextureFormat.ARGBFloat);
            irradianceT[1].enableRandomWrite = true;
            irradianceT[1].Create();
            Debug.Log("3");
            inscatterT[0] = new RenderTexture(RES_MU_S * RES_NU, RES_MU, 0, RenderTextureFormat.ARGBFloat);
            inscatterT[0].isVolume = true;
            inscatterT[0].enableRandomWrite = true;
            inscatterT[0].volumeDepth = RES_R;
            inscatterT[0].Create();
            Debug.Log("4");
            inscatterT[1] = new RenderTexture(RES_MU_S * RES_NU, RES_MU, 0, RenderTextureFormat.ARGBFloat);
            inscatterT[1].isVolume = true;
            inscatterT[1].enableRandomWrite = true;
            inscatterT[1].volumeDepth = RES_R;
            inscatterT[1].Create();
            Debug.Log("5");
            deltaET = new RenderTexture(SKY_W, SKY_H, 0, RenderTextureFormat.ARGBFloat);
            deltaET.enableRandomWrite = true;
            deltaET.Create();
            Debug.Log("6");
            deltaSRT = new RenderTexture(RES_MU_S * RES_NU, RES_MU, 0, RenderTextureFormat.ARGBFloat);
            deltaSRT.isVolume = true;
            deltaSRT.enableRandomWrite = true;
            deltaSRT.volumeDepth = RES_R;
            deltaSRT.Create();
            Debug.Log("7");
            deltaSMT = new RenderTexture(RES_MU_S * RES_NU, RES_MU, 0, RenderTextureFormat.ARGBFloat);
            deltaSMT.isVolume = true;
            deltaSMT.enableRandomWrite = true;
            deltaSMT.volumeDepth = RES_R;
            deltaSMT.Create();
            Debug.Log("8");
            deltaJT = new RenderTexture(RES_MU_S * RES_NU, RES_MU, 0, RenderTextureFormat.ARGBFloat);
            deltaJT.isVolume = true;
            deltaJT.enableRandomWrite = true;
            deltaJT.volumeDepth = RES_R;
            deltaJT.Create();
            Debug.Log("9");
            SetParameters(copyInscatter1);
            SetParameters(copyInscatterN);
            SetParameters(copyIrradiance);
            SetParameters(inscatter1);
            SetParameters(inscatterN);
            SetParameters(inscatterS);
            SetParameters(irradiance1);
            SetParameters(irradianceN);
            SetParameters(transmittance);

            step = 0;
            order = 2;

            RTUtility.ClearColor(irradianceT);

            while (!finished)
            {
                Preprocess();
            }
        }

        public void SetParameters(ComputeShader cs)
        {
            if (cs == null) return;

            cs.SetFloat("Rg", Rg);
            cs.SetFloat("Rt", Rt);
            cs.SetFloat("RL", RL);
            cs.SetInt("TRANSMITTANCE_W", TRANSMITTANCE_W);
            cs.SetInt("TRANSMITTANCE_H", TRANSMITTANCE_H);
            cs.SetInt("SKY_W", SKY_W);
            cs.SetInt("SKY_H", SKY_H);
            cs.SetInt("RES_R", RES_R);
            cs.SetInt("RES_MU", RES_MU);
            cs.SetInt("RES_MU_S", RES_MU_S);
            cs.SetInt("RES_NU", RES_NU);
            cs.SetFloat("AVERAGE_GROUND_REFLECTANCE", AVERAGE_GROUND_REFLECTANCE);
            cs.SetFloat("HR", HR);
            cs.SetFloat("HM", HM);
            cs.SetVector("betaR", BETA_R);
            cs.SetVector("betaMSca", BETA_MSca);
            cs.SetVector("betaMEx", BETA_MSca / 0.9f);
            cs.SetFloat("mieG", Mathf.Clamp(MIE_G, 0.0f, 0.99f));
        }

        void Preprocess()
        {
            if (step == 0)
            {
                Debug.Log("Step 0 start");
                // computes transmittance texture T (line 1 in algorithm 4.1)
                transmittance.SetTexture(0, "transmittanceWrite", transmittanceT);
                transmittance.Dispatch(0, TRANSMITTANCE_W / NUM_THREADS, TRANSMITTANCE_H / NUM_THREADS, 1);
            }
            else if (step == 1)
            {
                Debug.Log("Step 1 start");
                // computes irradiance texture deltaE (line 2 in algorithm 4.1)
                irradiance1.SetTexture(0, "transmittanceRead", transmittanceT);
                irradiance1.SetTexture(0, "deltaEWrite", deltaET);
                irradiance1.Dispatch(0, SKY_W / NUM_THREADS, SKY_H / NUM_THREADS, 1);

                if (WRITE_DEBUG_TEX)
                    SaveAs8bit(SKY_W, SKY_H, 4, "/deltaE_debug", deltaET);
            }
            else if (step == 2)
            {
                Debug.Log("Step 2 start");
                // computes single scattering texture deltaS (line 3 in algorithm 4.1)
                // Rayleigh and Mie separated in deltaSR + deltaSM
                inscatter1.SetTexture(0, "transmittanceRead", transmittanceT);
                inscatter1.SetTexture(0, "deltaSRWrite", deltaSRT);
                inscatter1.SetTexture(0, "deltaSMWrite", deltaSMT);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (int i = 0; i < RES_R; i++)
                {
                    inscatter1.SetInt("layer", i);
                    inscatter1.Dispatch(0, (RES_MU_S * RES_NU) / NUM_THREADS, RES_MU / NUM_THREADS, 1);
                }

                if (WRITE_DEBUG_TEX)
                    SaveAs8bit(RES_MU_S * RES_NU, RES_MU * RES_R, 4, "/deltaSR_debug", deltaSRT);

                if (WRITE_DEBUG_TEX)
                    SaveAs8bit(RES_MU_S * RES_NU, RES_MU * RES_R, 4, "/deltaSM_debug", deltaSMT);
            }
            else if (step == 3)
            {
                Debug.Log("Step 3 start");
                // copies deltaE into irradiance texture E (line 4 in algorithm 4.1)
                copyIrradiance.SetFloat("k", 0.0f);
                copyIrradiance.SetTexture(0, "deltaERead", deltaET);
                copyIrradiance.SetTexture(0, "irradianceRead", irradianceT[READ]);
                copyIrradiance.SetTexture(0, "irradianceWrite", irradianceT[WRITE]);
                copyIrradiance.Dispatch(0, SKY_W / NUM_THREADS, SKY_H / NUM_THREADS, 1);

                RTUtility.Swap(irradianceT);
            }
            else if (step == 4)
            {
                Debug.Log("Step 4 start");
                // copies deltaS into inscatter texture S (line 5 in algorithm 4.1)
                copyInscatter1.SetTexture(0, "deltaSRRead", deltaSRT);
                copyInscatter1.SetTexture(0, "deltaSMRead", deltaSMT);
                copyInscatter1.SetTexture(0, "inscatterWrite", inscatterT[WRITE]);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (int i = 0; i < RES_R; i++)
                {
                    copyInscatter1.SetInt("layer", i);
                    copyInscatter1.Dispatch(0, (RES_MU_S * RES_NU) / NUM_THREADS, RES_MU / NUM_THREADS, 1);
                }

                RTUtility.Swap(inscatterT);
            }
            else if (step == 5)
            {
                Debug.Log("Step 5 start");
                // computes deltaJ (line 7 in algorithm 4.1)
                inscatterS.SetInt("first", (order == 2) ? 1 : 0);
                inscatterS.SetTexture(0, "transmittanceRead", transmittanceT);
                inscatterS.SetTexture(0, "deltaERead", deltaET);
                inscatterS.SetTexture(0, "deltaSRRead", deltaSRT);
                inscatterS.SetTexture(0, "deltaSMRead", deltaSMT);
                inscatterS.SetTexture(0, "deltaJWrite", deltaJT);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (int i = 0; i < RES_R; i++)
                {
                    inscatterS.SetInt("layer", i);
                    inscatterS.Dispatch(0, (RES_MU_S * RES_NU) / NUM_THREADS, RES_MU / NUM_THREADS, 1);
                }
            }
            else if (step == 6)
            {
                Debug.Log("Step 6 start");
                // computes deltaE (line 8 in algorithm 4.1)
                irradianceN.SetInt("first", (order == 2) ? 1 : 0);
                irradianceN.SetTexture(0, "deltaSRRead", deltaSRT);
                irradianceN.SetTexture(0, "deltaSMRead", deltaSMT);
                irradianceN.SetTexture(0, "deltaEWrite", deltaET);
                irradianceN.Dispatch(0, SKY_W / NUM_THREADS, SKY_H / NUM_THREADS, 1);
            }
            else if (step == 7)
            {
                Debug.Log("Step 7 start");
                // computes deltaS (line 9 in algorithm 4.1)
                inscatterN.SetTexture(0, "transmittanceRead", transmittanceT);
                inscatterN.SetTexture(0, "deltaJRead", deltaJT);
                inscatterN.SetTexture(0, "deltaSRWrite", deltaSRT);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (int i = 0; i < RES_R; i++)
                {
                    inscatterN.SetInt("layer", i);
                    inscatterN.Dispatch(0, (RES_MU_S * RES_NU) / NUM_THREADS, RES_MU / NUM_THREADS, 1);
                }
            }
            else if (step == 8)
            {
                Debug.Log("Step 8 start");
                // adds deltaE into irradiance texture E (line 10 in algorithm 4.1)
                copyIrradiance.SetFloat("k", 1.0f);
                copyIrradiance.SetTexture(0, "deltaERead", deltaET);
                copyIrradiance.SetTexture(0, "irradianceRead", irradianceT[READ]);
                copyIrradiance.SetTexture(0, "irradianceWrite", irradianceT[WRITE]);
                copyIrradiance.Dispatch(0, SKY_W / NUM_THREADS, SKY_H / NUM_THREADS, 1);

                RTUtility.Swap(irradianceT);
            }
            else if (step == 9)
            {
                Debug.Log("Step 9 start");
                // adds deltaS into inscatter texture S (line 11 in algorithm 4.1)
                copyInscatterN.SetTexture(0, "deltaSRead", deltaSRT);
                copyInscatterN.SetTexture(0, "inscatterRead", inscatterT[READ]);
                copyInscatterN.SetTexture(0, "inscatterWrite", inscatterT[WRITE]);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (int i = 0; i < RES_R; i++)
                {
                    copyInscatterN.SetInt("layer", i);
                    copyInscatterN.Dispatch(0, (RES_MU_S * RES_NU) / NUM_THREADS, RES_MU / NUM_THREADS, 1);
                }

                RTUtility.Swap(inscatterT);

                if (order < 4)
                {
                    step = 4;
                    order += 1;
                }
            }
            else if (step == 10)
            {
                Debug.Log("Step 10 start");
                SaveAsRaw(TRANSMITTANCE_W * TRANSMITTANCE_H, 3, "/transmittance", transmittanceT);

                SaveAsRaw(SKY_W * SKY_H, 3, "/irradiance", irradianceT[READ]);

                SaveAsRaw((RES_MU_S * RES_NU) * RES_MU * RES_R, 4, "/inscatter", inscatterT[READ]);

                if (WRITE_DEBUG_TEX)
                {
                    SaveAs8bit(TRANSMITTANCE_W, TRANSMITTANCE_H, 4, "/transmittance_debug", transmittanceT);

                    SaveAs8bit(SKY_W, SKY_H, 4, "/irradiance_debug", irradianceT[READ], 10.0f);

                    SaveAs8bit(RES_MU_S * RES_NU, RES_MU * RES_R, 4, "/inscater_debug", inscatterT[READ]);
                }
            }
            else if (step == 11)
            {
                Debug.Log("Step 11 start");
                finished = true;
                Debug.Log("Proland::PreProcessAtmo::Preprocess - Preprocess done. Files saved to - " + texturesPath);
            }

            step += 1;
        }

        void OnDestroy()
        {
            transmittanceT.Release();
            irradianceT[0].Release();
            irradianceT[1].Release();
            inscatterT[0].Release();
            inscatterT[1].Release();
            deltaET.Release();
            deltaSRT.Release();
            deltaSMT.Release();
            deltaJT.Release();
        }

        void SaveAsRaw(int size, int channels, string fileName, RenderTexture rtex)
        {
            ComputeBuffer buffer = new ComputeBuffer(size, sizeof(float) * channels);

            CBUtility.ReadFromRenderTexture(rtex, channels, buffer, readData);

            float[] data = new float[size * channels];

            buffer.GetData(data);

            byte[] byteArray = new byte[size * 4 * channels];
            System.Buffer.BlockCopy(data, 0, byteArray, 0, byteArray.Length);
            System.IO.File.WriteAllBytes(Application.dataPath + texturesPath + fileName + ".raw", byteArray);

            buffer.Release();
        }

        void SaveAs8bit(int width, int height, int channels, string fileName, RenderTexture rtex, float scale = 1.0f)
        {
            ComputeBuffer buffer = new ComputeBuffer(width * height, sizeof(float) * channels);

            CBUtility.ReadFromRenderTexture(rtex, channels, buffer, readData);

            float[] data = new float[width * height * channels];

            buffer.GetData(data);

            Texture2D tex = new Texture2D(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color col = new Color(0, 0, 0, 1);

                    col.r = data[(x + y * width) * channels + 0];

                    if (channels > 1)
                        col.g = data[(x + y * width) * channels + 1];

                    if (channels > 2)
                        col.b = data[(x + y * width) * channels + 2];

                    tex.SetPixel(x, y, col * scale);
                }
            }

            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();

            System.IO.File.WriteAllBytes(Application.dataPath + texturesPath + fileName + ".png", bytes);

            buffer.Release();
        }
    }
}