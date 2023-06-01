#region License
// Procedural planet generator.
//  
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: 2017.03.28
// Creation Time: 2:18 PM
// Creator: zameran
#endregion

#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.IO;
using SpaceEngine.Core.Debugging;
using SpaceEngine.Core.Exceptions;
using SpaceEngine.Core.Preprocess.Terrain.Function;
using SpaceEngine.Core.Preprocess.Terrain.Mipmap;
using SpaceEngine.Core.Producers.Ortho;
using SpaceEngine.Core.Producers.Residual;
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

        InputMap.InputMap Source;

        private string ApplicationDataPath = "";

        private void Start()
        {
            Source = GetComponent<InputMap.InputMap>();

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
        void PreprocessPlaneDem(InputMap.InputMap source, string tempFolder, string destinationFolder)
        {
            if (DestinationTileSize % DestinationMinTileSize != 0) { throw new InvalidParameterException("DestinationTileSize must be a multiple of DestinationMinTileSize!"); }

            var startTime = Time.realtimeSinceStartup;
            var destinationSize = DestinationTileSize << DestinationMaxLevel;

            IHeightFunction2D function = new PlaneHeightFunction(source, destinationSize);
            var mipmap = new HeightMipmap(function, DestinationMinTileSize, destinationSize, DestinationTileSize, tempFolder);

            mipmap.Compute();
            mipmap.Generate(0, 0, 0, $"{destinationFolder}/{FileName}.dat");

            Logger.Log($"PreProcessTerrain.PreprocessPlaneDem: Computation time: {(Time.realtimeSinceStartup - startTime)} s");
        }

        void PreprocessSphericalDem(InputMap.InputMap source, string tempFolder, string destinationFolder)
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

            var mipmap1 = new HeightMipmap(function1, DestinationMinTileSize, destinationSize, DestinationTileSize, tempFolder);
            var mipmap2 = new HeightMipmap(function2, DestinationMinTileSize, destinationSize, DestinationTileSize, tempFolder);
            var mipmap3 = new HeightMipmap(function3, DestinationMinTileSize, destinationSize, DestinationTileSize, tempFolder);
            var mipmap4 = new HeightMipmap(function4, DestinationMinTileSize, destinationSize, DestinationTileSize, tempFolder);
            var mipmap5 = new HeightMipmap(function5, DestinationMinTileSize, destinationSize, DestinationTileSize, tempFolder);
            var mipmap6 = new HeightMipmap(function6, DestinationMinTileSize, destinationSize, DestinationTileSize, tempFolder);

            HeightMipmap.SetCube(mipmap1, mipmap2, mipmap3, mipmap4, mipmap5, mipmap6);

            mipmap1.Compute(); mipmap1.Generate(0, 0, 0, $"{destinationFolder}/{FileName}1.dat");
            mipmap2.Compute(); mipmap2.Generate(0, 0, 0, $"{destinationFolder}/{FileName}2.dat");
            mipmap3.Compute(); mipmap3.Generate(0, 0, 0, $"{destinationFolder}/{FileName}3.dat");
            mipmap4.Compute(); mipmap4.Generate(0, 0, 0, $"{destinationFolder}/{FileName}4.dat");
            mipmap5.Compute(); mipmap5.Generate(0, 0, 0, $"{destinationFolder}/{FileName}5.dat");
            mipmap6.Compute(); mipmap6.Generate(0, 0, 0, $"{destinationFolder}/{FileName}6.dat");

            Logger.Log($"PreProcessTerrain.PreprocessDem: Computation time: {(Time.realtimeSinceStartup - startTime)} s");
        }

        /// <summary>
        /// Preprocess a map into files that can be used with a <see cref="OrthoCPUProducer"/>.
        /// </summary>
        /// <param name="source">The map to be preprocessed.</param>
        /// <param name="tempFolder">Where temporary files must be saved.</param>
        /// <param name="destinationFolder">Where the precomputed file must be saved.</param>
        void PreprocessPlaneOrtho(InputMap.InputMap source, string tempFolder, string destinationFolder)
        {
            var startTime = Time.realtimeSinceStartup;
            var destinationSize = DestinationTileSize << DestinationMaxLevel;

            IColorFunction2D function = new PlaneColorFunction(source, destinationSize);
            var mipmap = new ColorMipmap(function, destinationSize, DestinationTileSize, 2, DestinationChannels, tempFolder);

            mipmap.Compute();
            mipmap.Generate(0, 0, 0, $"{destinationFolder}/{FileName}.dat");

            Logger.Log($"PreProcessTerrain.PreprocessPlaneOrtho: Computation time: {(Time.realtimeSinceStartup - startTime)} s");
        }
    }
}