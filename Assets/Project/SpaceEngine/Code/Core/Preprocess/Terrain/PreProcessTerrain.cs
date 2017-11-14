using SpaceEngine.Core.Debugging;
using SpaceEngine.Core.Exceptions;

using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

using Logger = SpaceEngine.Core.Debugging.Logger;

namespace SpaceEngine.Core.Preprocess.Terrain
{
    [UseLogger(LoggerCategory.Core)]
    public class PreProcessTerrain : MonoBehaviour
    {
        [Serializable]
        public enum MODE
        {
            NONE,
            HEIGHT,
            COLOR
        }

        [Serializable]
        public enum TYPE
        {
            NONE,
            PLANE,
            SPHERICAL
        }

        [SerializeField]
        MODE Mode = MODE.NONE;
        
        [SerializeField]
        TYPE Type = TYPE.NONE;

        [SerializeField]
        string TempFolder = "/Resources/Preprocess/Textures/Terrain/Tmp";

        [SerializeField]
        string DestinationFolder = "/Resources/Preprocess/Textures/Terrain";

        [SerializeField]
        string FileName = "DEM";

        [SerializeField]
        int DestinationTileSize = 96;

        [SerializeField]
        int DestinationMinTileSize = 96;

        [SerializeField]
        int DestinationMaxLevel = 2;

        [SerializeField]
        int DestinationChannels = 3;

        [SerializeField]
        bool DeleteTempOnFinish = true;

        InputMap Source;

        private string ApplicationDataPath = "";

        private void Start()
        {
            Source = GetComponent<InputMap>();

            if (Source == null) { throw new NullReferenceException("Input map is null. Have you added a Input map component to PreProcess game object?"); }

            ApplicationDataPath = Application.dataPath;

            try
            {
                switch ((int)Mode)
                {
                    case (int)MODE.HEIGHT:
                        PreprocessDem();
                        break;
                    case (int)MODE.COLOR:
                        PreprocessOrtho();
                        break;
                    default:
                        Logger.LogWarning("PreProcessTerrain.Start: Nothing to produce/precompute!");
                        break;
                }
            }
            finally
            {
                if (DeleteTempOnFinish)
                {
                    var directory = new DirectoryInfo(ApplicationDataPath + TempFolder);

                    if (directory.Exists)
                    {
                        foreach (var file in directory.GetFiles())
                        {
                            file.Delete();
                        }
                    }
                }

#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
        }

        void PreprocessDem()
        {
            switch ((int)Type)
            {
                case (int)TYPE.PLANE:
                    PreprocessPlaneDem(Source, ApplicationDataPath + TempFolder, ApplicationDataPath + DestinationFolder);
                    break;
                case (int)TYPE.SPHERICAL:
                    PreprocessSphericalDem(Source, ApplicationDataPath + TempFolder, ApplicationDataPath + DestinationFolder);
                    break;
                default:
                    Logger.LogWarning("PreProcessTerrain.Preprocess: Nothing to produce/precompute!");
                    break;
            }
        }

        void PreprocessOrtho()
        {
            switch ((int)Type)
            {
                case (int)TYPE.PLANE:
                    PreprocessPlaneOrtho(Source, ApplicationDataPath + TempFolder, ApplicationDataPath + DestinationFolder);
                    break;
                case (int)TYPE.SPHERICAL:
                    throw new NotImplementedException();
                default:
                    Logger.LogWarning("PreProcessTerrain.Preprocess: Nothing to produce/precompute!");
                    break;
            }
        }

        /// <summary>
        /// Preprocess a map into files that can be used with a <see cref="ResidualProducer"/>.
        /// </summary>
        /// <param name="source">The map to be preprocessed.</param>
        /// <param name="tempFolder">Where temporary files must be saved.</param>
        /// <param name="destinationFolder">Where the precomputed file must be saved.</param>
        void PreprocessPlaneDem(InputMap source, string tempFolder, string destinationFolder)
        {
            if (DestinationTileSize % DestinationMinTileSize != 0) { throw new InvalidParameterException("DestinationTileSize must be a multiple of DestinationMinTileSize!"); }

            var startTime = Time.realtimeSinceStartup;
            var destinationSize = DestinationTileSize << DestinationMaxLevel;

            IHeightFunction2D function = new PlaneHeightFunction(source, destinationSize);
            HeightMipmap mipmap = new HeightMipmap(function, DestinationMinTileSize, destinationSize, DestinationTileSize, tempFolder);

            mipmap.Compute();
            mipmap.Generate(0, 0, 0, destinationFolder + "/" + FileName + ".dat");

            Logger.Log(string.Format("PreProcessTerrain.PreprocessPlaneDem: Computation time: {0} s", (Time.realtimeSinceStartup - startTime)));
        }

        void PreprocessSphericalDem(InputMap source, string tempFolder, string destinationFolder)
        {
            if (DestinationTileSize % DestinationMinTileSize != 0) { throw new InvalidParameterException("DestinationTileSize must be a multiple of DestinationMinTileSize!"); }

            var startTime = Time.realtimeSinceStartup;
            var destinationSize = DestinationTileSize << DestinationMaxLevel;

            IHeightFunction2D function1 = new SphericalHeightFunction(source, ProjectionHelper.Projection1, destinationSize);
            IHeightFunction2D function2 = new SphericalHeightFunction(source, ProjectionHelper.Projection2, destinationSize);
            IHeightFunction2D function3 = new SphericalHeightFunction(source, ProjectionHelper.Projection3, destinationSize);
            IHeightFunction2D function4 = new SphericalHeightFunction(source, ProjectionHelper.Projection4, destinationSize);
            IHeightFunction2D function5 = new SphericalHeightFunction(source, ProjectionHelper.Projection5, destinationSize);
            IHeightFunction2D function6 = new SphericalHeightFunction(source, ProjectionHelper.Projection6, destinationSize);

            HeightMipmap mipmap1 = new HeightMipmap(function1, DestinationMinTileSize, destinationSize, DestinationTileSize, tempFolder);
            HeightMipmap mipmap2 = new HeightMipmap(function2, DestinationMinTileSize, destinationSize, DestinationTileSize, tempFolder);
            HeightMipmap mipmap3 = new HeightMipmap(function3, DestinationMinTileSize, destinationSize, DestinationTileSize, tempFolder);
            HeightMipmap mipmap4 = new HeightMipmap(function4, DestinationMinTileSize, destinationSize, DestinationTileSize, tempFolder);
            HeightMipmap mipmap5 = new HeightMipmap(function5, DestinationMinTileSize, destinationSize, DestinationTileSize, tempFolder);
            HeightMipmap mipmap6 = new HeightMipmap(function6, DestinationMinTileSize, destinationSize, DestinationTileSize, tempFolder);

            HeightMipmap.SetCube(mipmap1, mipmap2, mipmap3, mipmap4, mipmap5, mipmap6);

            mipmap1.Compute(); mipmap1.Generate(0, 0, 0, destinationFolder + "/" + FileName + "1" + ".dat");
            mipmap2.Compute(); mipmap2.Generate(0, 0, 0, destinationFolder + "/" + FileName + "2" + ".dat");
            mipmap3.Compute(); mipmap3.Generate(0, 0, 0, destinationFolder + "/" + FileName + "3" + ".dat");
            mipmap4.Compute(); mipmap4.Generate(0, 0, 0, destinationFolder + "/" + FileName + "4" + ".dat");
            mipmap5.Compute(); mipmap5.Generate(0, 0, 0, destinationFolder + "/" + FileName + "5" + ".dat");
            mipmap6.Compute(); mipmap6.Generate(0, 0, 0, destinationFolder + "/" + FileName + "6" + ".dat");

            Logger.Log(string.Format("PreProcessTerrain.PreprocessDem: Computation time: {0} s", (Time.realtimeSinceStartup - startTime)));
        }

        /// <summary>
        /// Preprocess a map into files that can be used with a <see cref="OrthoCPUProducer"/>.
        /// </summary>
        /// <param name="source">The map to be preprocessed.</param>
        /// <param name="tempFolder">Where temporary files must be saved.</param>
        /// <param name="destinationFolder">Where the precomputed file must be saved.</param>
        void PreprocessPlaneOrtho(InputMap source, string tempFolder, string destinationFolder)
        {
            var startTime = Time.realtimeSinceStartup;
            var destinationSize = DestinationTileSize << DestinationMaxLevel;

            IColorFunction2D function = new PlaneColorFunction(source, destinationSize);
            ColorMipmap mipmap = new ColorMipmap(function, destinationSize, DestinationTileSize, 2, DestinationChannels, tempFolder);

            mipmap.Compute();
            mipmap.Generate(0, 0, 0, destinationFolder + "/" + FileName + ".dat");

            Logger.Log(string.Format("PreProcessTerrain.PreprocessPlaneOrtho: Computation time: {0} s", (Time.realtimeSinceStartup - startTime)));
        }
    }
}