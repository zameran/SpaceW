using SpaceEngine.Core.Exceptions;

using System;
using System.IO;

using UnityEngine;

namespace SpaceEngine.Core.Preprocess.Terrain
{
    public class PreProcessTerrain : MonoBehaviour
    {
        [Serializable]
        public enum MODE
        {
            NONE,
            HEIGHT,
            COLOR
        };

        [SerializeField]
        MODE Mode = MODE.NONE;

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

        private void Start()
        {
            Source = GetComponent<InputMap>();

            if (Source == null) { throw new NullReferenceException("Input map is null. Have you added a Input map component to PreProcess game object?"); }

            try
            {
                switch ((int)Mode)
                {
                    case (int)MODE.HEIGHT:
                        PreprocessDem(Source, Application.dataPath + TempFolder, Application.dataPath + DestinationFolder);
                        break;
                    case (int)MODE.COLOR:
                        PreprocessOrtho(Source, Application.dataPath + TempFolder, Application.dataPath + DestinationFolder);
                        break;
                    default:
                        Debug.LogWarning("PreProcessTerrain.Start: Nothing to produce/precompute!");
                        break;
                }
            }
            finally
            {
                if (DeleteTempOnFinish)
                {
                    var directory = new DirectoryInfo(Application.dataPath + TempFolder);

                    foreach (var file in directory.GetFiles())
                    {
                        file.Delete();
                    }
                }
            }
        }

        /// <summary>
        /// Preprocess a map into files that can be used with a <see cref="ResidualProducer"/>.
        /// </summary>
        /// <param name="source">The map to be preprocessed.</param>
        /// <param name="tempFolder">Where temporary files must be saved.</param>
        /// <param name="destinationFolder">Where the precomputed file must be saved.</param>
        void PreprocessDem(InputMap source, string tempFolder, string destinationFolder)
        {
            if (DestinationTileSize % DestinationMinTileSize != 0) { throw new InvalidParameterException("DestinationTileSize must be a multiple of DestinationMinTileSize!"); }

            var startTime = Time.realtimeSinceStartup;
            var destinationSize = DestinationTileSize << DestinationMaxLevel;

            IHeightFunction2D function = new PlaneHeightFunction(source, destinationSize);
            HeightMipmap mipmap = new HeightMipmap(function, DestinationMinTileSize, destinationSize, DestinationTileSize, tempFolder);

            mipmap.Compute();
            mipmap.Generate(0, 0, 0, destinationFolder + "/" + FileName + ".proland");

            Debug.Log(string.Format("PreProcessTerrain.PreprocessDem: Computation time: {0} s", (Time.realtimeSinceStartup - startTime)));
        }

        /// <summary>
        /// Preprocess a map into files that can be used with a <see cref="OrthoCPUProducer"/>.
        /// </summary>
        /// <param name="source">The map to be preprocessed.</param>
        /// <param name="tempFolder">Where temporary files must be saved.</param>
        /// <param name="destinationFolder">Where the precomputed file must be saved.</param>
        void PreprocessOrtho(InputMap source, string tempFolder, string destinationFolder)
        {
            var startTime = Time.realtimeSinceStartup;
            var destinationSize = DestinationTileSize << DestinationMaxLevel;

            IColorFunction2D function = new PlaneColorFunction(source, destinationSize);
            ColorMipmap mipmap = new ColorMipmap(function, destinationSize, DestinationTileSize, 2, DestinationChannels, tempFolder);

            mipmap.Compute();
            mipmap.Generate(0, 0, 0, destinationFolder + "/" + FileName + ".proland");

            Debug.Log(string.Format("PreProcessTerrain.PreprocessOrtho: Computation time: {0} s", (Time.realtimeSinceStartup - startTime)));
        }
    }
}