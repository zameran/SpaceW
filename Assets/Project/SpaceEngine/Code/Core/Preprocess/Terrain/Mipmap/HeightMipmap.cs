using SpaceEngine.Core.Debugging;

using System;
using System.IO;

using UnityEngine;

using Logger = SpaceEngine.Core.Debugging.Logger;

namespace SpaceEngine.Core.Preprocess.Terrain
{
    [UseLogger(LoggerCategory.Core)]
    public class HeightMipmap : AbstractTileCache
    {
        public HeightMipmap Left;
        public HeightMipmap Right;
        public HeightMipmap Bottom;
        public HeightMipmap Top;

        public int LeftR;
        public int RightR;
        public int BottomR;
        public int TopR;

        IHeightFunction2D HeightFunction;

        int TopLevelSize;
        int BaseLevelSize;
        int Size;

        float Scale;

        int MinLevel;
        int MaxLevel;
        int CurrentMipLevel;

        string TempFolder;

        float[] TileData;

        int CurrentLevel;
        int ConstantTile;

        float[] MaxR;

        public void SetCurrentLevel(int level)
        {
            CurrentLevel = level;
        }

        public HeightMipmap(IHeightFunction2D heightFunction, int topLevelSize, int baseLevelSize, int tileSize, string tempFolder) : base(baseLevelSize, baseLevelSize, tileSize, 1, 200)
        {
            HeightFunction = heightFunction;
            TopLevelSize = topLevelSize;
            BaseLevelSize = baseLevelSize;

            TempFolder = tempFolder;

            Size = tileSize;
            Scale = 1.0f;
            MinLevel = 0;
            MaxLevel = 0;

            var size = tileSize;

            while (size > topLevelSize)
            {
                MinLevel += 1;
                size /= 2;
            }

            size = baseLevelSize;

            while (size > topLevelSize)
            {
                MaxLevel += 1;
                size /= 2;
            }

            MaxR = new float[MaxLevel + 1];
            TileData = new float[(tileSize + 5) * (tileSize + 5) * 1];
            ConstantTile = -1;

            Left = null;
            Right = null;
            Bottom = null;
            Top = null;

            if (!Directory.Exists(TempFolder)) { Directory.CreateDirectory(TempFolder); }

            Logger.Log(string.Format("HeightMipmap.ctor: TopLevelSize: {0}; BaseLevelSize: {1}; TileSize: {2}; Scale: {3}; MinLevel: {4}; MaxLevel: {5}", TopLevelSize, 
                                                                                                                                                          BaseLevelSize, 
                                                                                                                                                          Size, 
                                                                                                                                                          Scale, 
                                                                                                                                                          MinLevel, 
                                                                                                                                                          MaxLevel));
        }

        public void Compute()
        {
            BuildBaseLevelTiles();

            CurrentMipLevel = MaxLevel - 1;

            while (CurrentMipLevel >= 0)
            {
                BuildMipmapLevel(CurrentMipLevel--);
            }
        }

        public void Generate(int rootLevel, int rootTx, int rootTy, string file)
        {
            for (int level = 1; level <= MaxLevel; ++level)
            {
                BuildResiduals(level);
            }

            File.Delete(file);

            var tilesCount = MinLevel + ((1 << (Mathf.Max(MaxLevel - MinLevel, 0) * 2 + 2)) - 1) / 3;

            Logger.Log(string.Format("HeightMipmap.Generate: tiles count: {0}", tilesCount));

            long[] offsets = new long[tilesCount * 2];
            byte[] byteArray = new byte[(7 * 4) + (+MaxR.Length * 4) + (offsets.Length * 8)];
            long offset = byteArray.Length;

            using (Stream stream = new FileStream(file, FileMode.Create))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Write(byteArray, 0, byteArray.Length);
            }

            for (int l = 0; l < MinLevel; ++l)
            {
                ProduceTile(l, 0, 0, ref offset, offsets, file);
            }

            for (int l = MinLevel; l <= MaxLevel; ++l)
            {
                ProduceTilesLebeguesOrder(l - MinLevel, 0, 0, 0, ref offset, offsets, file);
            }

            Buffer.BlockCopy(new int[] { MinLevel }, 0, byteArray, 0, 4);
            Buffer.BlockCopy(new int[] { MaxLevel }, 0, byteArray, 4, 4);
            Buffer.BlockCopy(new int[] { Size }, 0, byteArray, 8, 4);
            Buffer.BlockCopy(new int[] { rootLevel }, 0, byteArray, 12, 4);
            Buffer.BlockCopy(new int[] { rootTx }, 0, byteArray, 16, 4);
            Buffer.BlockCopy(new int[] { rootTy }, 0, byteArray, 20, 4);
            Buffer.BlockCopy(new float[] { Scale }, 0, byteArray, 24, 4);
            Buffer.BlockCopy(MaxR, 0, byteArray, 28, 4 * MaxR.Length);
            Buffer.BlockCopy(offsets, 0, byteArray, 28 + (4 * MaxR.Length), 8 * offsets.Length);

            using (Stream stream = new FileStream(file, FileMode.Open))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Write(byteArray, 0, byteArray.Length);
            }

            for (int i = 0; i < MaxR.Length; i++)
            {
                Logger.Log(string.Format("HeightMipmap.Generate: Level: {0}; MaxResidual: {1}", i, MaxR[i].ToString("F6")));
            }

            Logger.Log(string.Format("HeightMipmap.Generate: Saved file path: {0} ", file));
        }

        private void Rotation(int r, int n, int x, int y, out int xp, out int yp)
        {
            switch (r)
            {
                case 0:
                    xp = x;
                    yp = y;
                    break;
                case 1:
                    xp = y;
                    yp = n - 1 - x;
                    break;
                case 2:
                    xp = n - 1 - x;
                    yp = n - 1 - y;
                    break;
                case 3:
                    xp = n - 1 - y;
                    yp = x;
                    break;
                default:
                    xp = 0;
                    yp = 0;

                    Logger.LogError("HeightMipmap.Rotation: Something goes wrong!");
                    Debug.Break();

                    break;
            }
        }

        public override float GetTileHeight(int x, int y)
        {
            var levelSize = 1 + (BaseLevelSize >> (MaxLevel - CurrentLevel));

            if (x <= 2 && y <= 2 && Left != null && Bottom != null)
            {
                x = 0;
                y = 0;
            }
            else if (x > levelSize - 4 && y <= 2 && Right != null && Bottom != null)
            {
                x = levelSize - 1;
                y = 0;
            }
            else if (x <= 2 && y > levelSize - 4 && Left != null && Top != null)
            {
                x = 0;
                y = levelSize - 1;
            }
            else if (x > levelSize - 4 && y > levelSize - 4 && Right != null && Top != null)
            {
                x = levelSize - 1;
                y = levelSize - 1;
            }

            // NOTE : Looks like rotation stuff should not be applied to heightmaps...
            /*
            if (x < 0 && Left != null)
            {
                int xp;
                int yp;

                Rotation(LeftR, levelSize, levelSize - 1 + x, y, out xp, out yp);

                Debug.Assert(Left.CurrentLevel == CurrentLevel);

                return Left.GetTileHeight(xp, yp);
            }

            if (x >= levelSize && Right != null)
            {
                int xp;
                int yp;

                Rotation(RightR, levelSize, x - levelSize + 1, y, out xp, out yp);

                Debug.Assert(Right.CurrentLevel == CurrentLevel);

                return Right.GetTileHeight(xp, yp);
            }

            if (y < 0 && Bottom != null)
            {
                int xp;
                int yp;

                Rotation(BottomR, levelSize, x, levelSize - 1 + y, out xp, out yp);

                Debug.Assert(Bottom.CurrentLevel == CurrentLevel);

                return Bottom.GetTileHeight(xp, yp);
            }

            if (y >= levelSize && Top != null)
            {
                int xp;
                int yp;

                Rotation(TopR, levelSize, x, y - levelSize + 1, out xp, out yp);

                Debug.Assert(Top.CurrentLevel == CurrentLevel);

                return Top.GetTileHeight(xp, yp);
            }
            */

            return base.GetTileHeight(x, y);
        }

        public override void Reset(int width, int height, int tileSize)
        {
            if (Width != width || Height != height)
            {
                Logger.Log(string.Format("HeightMipmap.Reset: Resetting to width {0} and height {1}; TileSize: {2}", height, width, tileSize));

                base.Reset(width, height, tileSize);

                if (Left != null)
                {
                    Left.SetCurrentLevel(CurrentLevel);
                    Left.Reset(width, height, tileSize);
                }

                if (Right != null)
                {
                    Right.SetCurrentLevel(CurrentLevel);
                    Right.Reset(width, height, tileSize);
                }

                if (Bottom != null)
                {
                    Bottom.SetCurrentLevel(CurrentLevel);
                    Bottom.Reset(width, height, tileSize);
                }

                if (Top != null)
                {
                    Top.SetCurrentLevel(CurrentLevel);
                    Top.Reset(width, height, tileSize);
                }
            }
            else
            {
                if (Left != null)
                {
                    Left.SetCurrentLevel(CurrentLevel);
                }

                if (Right != null)
                {
                    Right.SetCurrentLevel(CurrentLevel);
                }

                if (Bottom != null)
                {
                    Bottom.SetCurrentLevel(CurrentLevel);
                }

                if (Top != null)
                {
                    Top.SetCurrentLevel(CurrentLevel);
                }
            }
        }

        private string FilePath(string tempFolder, string name, int level, int tx, int ty)
        {
            return string.Format("{0}/{1}-{2}-{3}-{4}.raw", tempFolder, name, level, tx, ty);
        }

        private void SaveTile(string name, int level, int tx, int ty, float[] tile)
        {
            var fileName = FilePath(TempFolder, name, level, tx, ty);
            var dataBuffer = new byte[tile.Length * 4];

            Buffer.BlockCopy(tile, 0, dataBuffer, 0, dataBuffer.Length);
            File.WriteAllBytes(fileName, dataBuffer);
        }

        private void LoadTile(string name, int level, int tx, int ty, float[] tile)
        {
            var fileName = FilePath(TempFolder, name, level, tx, ty);

            var fileInfo = new FileInfo(fileName);
            if (fileInfo == null) throw new FileNotFoundException("Could not read tile " + fileName);

            var data = new byte[fileInfo.Length];

            using (Stream stream = fileInfo.OpenRead())
            {
                stream.Read(data, 0, (int)fileInfo.Length);
            }

            for (int x = 0, i = 0; x < fileInfo.Length / 4; x++, i += 4)
            {
                tile[x] = BitConverter.ToSingle(data, i);
            }
        }

        protected override float[] ReadTile(int tx, int ty)
        {
            var outputTileData = new float[(Size + 5) * (Size + 5)];

            LoadTile("Base", CurrentLevel, tx, ty, outputTileData);

            return outputTileData;
        }

        protected void GetTile(int level, int tx, int ty, float[] tile)
        {
            var tileSize = Mathf.Min(TopLevelSize << level, this.Size);

            for (int j = 0; j <= tileSize + 4; ++j)
            {
                for (int i = 0; i <= tileSize + 4; ++i)
                {
                    tile[i + j * (this.Size + 5)] = GetTileHeight(i + tileSize * tx - 2, j + tileSize * ty - 2) / Scale;
                }
            }
        }

        private void BuildBaseLevelTiles()
        {
            var tilesCount = BaseLevelSize / Size;

            Logger.Log(string.Format("HeightMipmap.BuildBaseLevelTiles: Build mipmap level: {0}", MaxLevel));

            var maxR = float.NegativeInfinity;

            for (int ty = 0; ty < tilesCount; ++ty)
            {
                for (int tx = 0; tx < tilesCount; ++tx)
                {
                    var offset = (int)0;

                    for (int j = -2; j <= Size + 2; ++j)
                    {
                        for (int i = -2; i <= Size + 2; ++i)
                        {
                            var h = HeightFunction.GetValue(tx * Size + i, ty * Size + j);

                            TileData[offset++] = h;

                            if (h > maxR) maxR = h;
                        }
                    }

                    SaveTile("Base", MaxLevel, tx, ty, TileData);
                }
            }

            Logger.Log(string.Format("HeightMipmap.BuildBaseLevelTiles: Max Residual:  {0}", maxR.ToString("F6")));

            MaxR[0] = maxR / Scale;
        }

        private void BuildMipmapLevel(int level)
        {
            var tilesCount = Mathf.Max(1, (BaseLevelSize / Size) >> (MaxLevel - level));

            Logger.Log(string.Format("HeightMipmap.BuildMipmapLevel: Build mipmap level: {0}", level));

            CurrentLevel = level + 1;

            Reset(BaseLevelSize >> (MaxLevel - CurrentLevel), BaseLevelSize >> (MaxLevel - CurrentLevel), Mathf.Min(TopLevelSize << CurrentLevel, Size));

            for (int ty = 0; ty < tilesCount; ++ty)
            {
                for (int tx = 0; tx < tilesCount; ++tx)
                {
                    var offset = (int)0;
                    var currentTileSize = Mathf.Min(TopLevelSize << level, Size);

                    for (int j = -2; j <= currentTileSize + 2; ++j)
                    {
                        for (int i = -2; i <= currentTileSize + 2; ++i)
                        {
                            TileData[offset++] = GetTileHeight(2 * (tx * currentTileSize + i), 2 * (ty * currentTileSize + j));
                        }
                    }

                    SaveTile("Base", level, tx, ty, TileData);
                }
            }
        }

        private void BuildResiduals(int level)
        {
            var tilesCount = Mathf.Max(1, (BaseLevelSize / Size) >> (MaxLevel - level));

            Logger.Log(string.Format("HeightMipmap.BuildResiduals: Build residuals level: {0}", level));

            CurrentLevel = level;

            Reset(BaseLevelSize >> (MaxLevel - CurrentLevel), BaseLevelSize >> (MaxLevel - CurrentLevel), Mathf.Min(TopLevelSize << CurrentLevel, Size));

            var parentTile = new float[(Size + 5) * (Size + 5)];
            var currentTile = new float[(Size + 5) * (Size + 5)];
            var residualTile = new float[(Size + 5) * (Size + 5)];
            var levelMaxR = float.NegativeInfinity;

            for (int ty = 0; ty < tilesCount; ++ty)
            {
                for (int tx = 0; tx < tilesCount; ++tx)
                {
                    float maxR, meanR, maxErr;

                    GetApproxTile(level - 1, tx / 2, ty / 2, parentTile);
                    GetTile(level, tx, ty, currentTile);
                    ComputeResidual(parentTile, currentTile, level, tx, ty, residualTile, out maxR, out meanR);
                    ComputeApproxTile(parentTile, residualTile, level, tx, ty, currentTile, out maxErr);

                    if (level < MaxLevel)
                    {
                        SaveTile("Approx", level, tx, ty, currentTile);
                    }

                    SaveTile("Residual", level, tx, ty, residualTile);

                    if (maxR > levelMaxR) levelMaxR = maxR;

                    Logger.Log(string.Format("HeightMipmap.BuildResiduals: {0}-{1}-{2}; Max Residual: {3:F6}; Max Error: {4:F6}", level, tx, ty, maxR, maxErr));
                }
            }

            MaxR[level] = levelMaxR;
        }

        private void GetApproxTile(int level, int tx, int ty, float[] tile)
        {
            if (level == 0)
            {
                var oldLevel = CurrentLevel;
                CurrentLevel = 0;

                Reset(TopLevelSize, TopLevelSize, TopLevelSize);
                GetTile(level, tx, ty, tile);

                CurrentLevel = oldLevel;

                Reset(BaseLevelSize >> (MaxLevel - CurrentLevel), BaseLevelSize >> (MaxLevel - CurrentLevel), Mathf.Min(TopLevelSize << CurrentLevel, Size));

                return;
            }

            LoadTile("Approx", level, tx, ty, tile);
        }

        private void ComputeResidual(float[] parentTile, float[] tile, int level, int tx, int ty, float[] residual, out float maxR, out float meanR)
        {
            maxR = 0.0f;
            meanR = 0.0f;

            var tileSize = Mathf.Min(TopLevelSize << level, this.Size);
            var px = 1 + (tx % 2) * tileSize / 2;
            var py = 1 + (ty % 2) * tileSize / 2;
            var n = this.Size + 5;

            for (int j = 0; j <= tileSize + 4; ++j)
            {
                for (int i = 0; i <= tileSize + 4; ++i)
                {
                    float z;

                    if (j % 2 == 0)
                    {
                        if (i % 2 == 0)
                        {
                            z = parentTile[i / 2 + px + (j / 2 + py) * n];
                        }
                        else
                        {
                            float z0 = parentTile[i / 2 + px - 1 + (j / 2 + py) * n];
                            float z1 = parentTile[i / 2 + px + (j / 2 + py) * n];
                            float z2 = parentTile[i / 2 + px + 1 + (j / 2 + py) * n];
                            float z3 = parentTile[i / 2 + px + 2 + (j / 2 + py) * n];

                            z = ((z1 + z2) * 9.0f - (z0 + z3)) / 16.0f;
                        }
                    }
                    else
                    {
                        if (i % 2 == 0)
                        {
                            float z0 = parentTile[i / 2 + px + (j / 2 - 1 + py) * n];
                            float z1 = parentTile[i / 2 + px + (j / 2 + py) * n];
                            float z2 = parentTile[i / 2 + px + (j / 2 + 1 + py) * n];
                            float z3 = parentTile[i / 2 + px + (j / 2 + 2 + py) * n];

                            z = ((z1 + z2) * 9.0f - (z0 + z3)) / 16.0f;
                        }
                        else
                        {
                            int dj;

                            z = 0;

                            for (dj = -1; dj <= 2; ++dj)
                            {
                                var f = dj == -1 || dj == 2 ? -1.0f / 16.0f : 9.0f / 16.0f;

                                int di;

                                for (di = -1; di <= 2; ++di)
                                {
                                    var g = di == -1 || di == 2 ? -1.0f / 16.0f : 9.0f / 16.0f;

                                    z += f * g * parentTile[i / 2 + di + px + (j / 2 + dj + py) * n];
                                }
                            }
                        }
                    }

                    var offset = i + j * n;
                    var diff = tile[offset] - z;

                    residual[offset] = diff;
                    maxR = Mathf.Max(diff < 0.0f ? -diff : diff, maxR);
                    meanR += diff < 0.0f ? -diff : diff;
                }
            }

            meanR = meanR / (n * n);
        }

        private void ComputeApproxTile(float[] parentTile, float[] residual, int level, int tx, int ty, float[] tile, out float maxError)
        {
            maxError = 0.0f;

            var tileSize = Mathf.Min(TopLevelSize << level, this.Size);
            var px = 1 + (tx % 2) * tileSize / 2;
            var py = 1 + (ty % 2) * tileSize / 2;
            var n = this.Size + 5;

            for (int j = 0; j <= tileSize + 4; ++j)
            {
                for (int i = 0; i <= tileSize + 4; ++i)
                {
                    float z;

                    if (j % 2 == 0)
                    {
                        if (i % 2 == 0)
                        {
                            z = parentTile[i / 2 + px + (j / 2 + py) * n];
                        }
                        else
                        {
                            float z0 = parentTile[i / 2 + px - 1 + (j / 2 + py) * n];
                            float z1 = parentTile[i / 2 + px + (j / 2 + py) * n];
                            float z2 = parentTile[i / 2 + px + 1 + (j / 2 + py) * n];
                            float z3 = parentTile[i / 2 + px + 2 + (j / 2 + py) * n];

                            z = ((z1 + z2) * 9.0f - (z0 + z3)) / 16.0f;
                        }
                    }
                    else
                    {
                        if (i % 2 == 0)
                        {
                            float z0 = parentTile[i / 2 + px + (j / 2 - 1 + py) * n];
                            float z1 = parentTile[i / 2 + px + (j / 2 + py) * n];
                            float z2 = parentTile[i / 2 + px + (j / 2 + 1 + py) * n];
                            float z3 = parentTile[i / 2 + px + (j / 2 + 2 + py) * n];

                            z = ((z1 + z2) * 9.0f - (z0 + z3)) / 16.0f;
                        }
                        else
                        {
                            int dj;

                            z = 0.0f;

                            for (dj = -1; dj <= 2; ++dj)
                            {
                                var f = dj == -1 || dj == 2 ? -1.0f / 16.0f : 9 / 16.0f;

                                int di;

                                for (di = -1; di <= 2; ++di)
                                {
                                    var g = di == -1 || di == 2 ? -1.0f / 16.0f : 9 / 16.0f;

                                    z += f * g * parentTile[i / 2 + di + px + (j / 2 + dj + py) * n];
                                }
                            }
                        }
                    }

                    var offset = i + j * n;
                    var error = tile[offset] - (z + residual[offset]);

                    maxError = Mathf.Max(error < 0.0f ? -error : error, maxError);
                    tile[offset] = z + residual[offset];
                }
            }
        }

        private void ProduceTile(int level, int tx, int ty, ref long offset, long[] offsets, string file)
        {
            var tileSize = Mathf.Min(TopLevelSize << level, this.Size);

            Logger.Log(string.Format("HeightMipmap.ProduceTile: Producing tile {0}:{1}:{2}!", level, tx, ty));

            if (level == 0)
            {
                CurrentLevel = 0;

                Reset(tileSize, tileSize, tileSize);

                for (int j = 0; j <= tileSize + 4; ++j)
                {
                    for (int i = 0; i <= tileSize + 4; ++i)
                    {
                        var index = i + j * (tileSize + 5);

                        TileData[index] = GetTileHeight(i - 2, j - 2) / Scale;
                    }
                }
            }
            else
            {
                LoadTile("Residual", level, tx, ty, TileData);
            }

            int tileid;

            if (level < MinLevel)
            {
                tileid = level;
            }
            else
            {
                var levelLength = Mathf.Max(level - MinLevel, 0);

                tileid = MinLevel + tx + ty * (1 << levelLength) + ((1 << (2 * levelLength)) - 1) / 3;
            }

            var isConstant = true;

            for (int i = 0; i < (tileSize + 5) * (tileSize + 5) * 1; ++i)
            {
                if (!BrainFuckMath.AlmostEquals(TileData[i], 0.0f))
                {
                    isConstant = false;

                    break;
                }
            }

            if (isConstant && ConstantTile != -1)
            {
                Logger.Log("HeightMipmap.ProduceTile: tile is const (All zeros)!");

                offsets[2 * tileid] = offsets[2 * ConstantTile];
                offsets[2 * tileid + 1] = offsets[2 * ConstantTile + 1];
            }
            else
            {
                var data = new byte[TileData.Length * 2];

                for (int i = 0; i < TileData.Length; i++)
                {
                    short z = (short)Mathf.Round(TileData[i] / MaxR[level] * (float)short.MaxValue);

                    data[2 * i] = (byte)(z & 0xFF);
                    data[2 * i + 1] = (byte)(z >> 8);
                }

                using (Stream stream = new FileStream(file, FileMode.Open))
                {
                    stream.Seek(offset, SeekOrigin.Begin);
                    stream.Write(data, 0, data.Length);
                }

                offsets[2 * tileid] = offset;
                offset += data.Length;
                offsets[2 * tileid + 1] = offset;
            }

            if (isConstant && ConstantTile == -1)
            {
                ConstantTile = tileid;
            }
        }

        private void ProduceTilesLebeguesOrder(int l, int level, int tx, int ty, ref long offset, long[] offsets, string file)
        {
            if (level < l)
            {
                ProduceTilesLebeguesOrder(l, level + 1, 2 * tx, 2 * ty, ref offset, offsets, file);
                ProduceTilesLebeguesOrder(l, level + 1, 2 * tx + 1, 2 * ty, ref offset, offsets, file);
                ProduceTilesLebeguesOrder(l, level + 1, 2 * tx, 2 * ty + 1, ref offset, offsets, file);
                ProduceTilesLebeguesOrder(l, level + 1, 2 * tx + 1, 2 * ty + 1, ref offset, offsets, file);
            }
            else
            {
                ProduceTile(MinLevel + level, tx, ty, ref offset, offsets, file);
            }
        }

        public static void SetCube(HeightMipmap hm1, HeightMipmap hm2, HeightMipmap hm3, HeightMipmap hm4, HeightMipmap hm5, HeightMipmap hm6)
        {
            hm1.Left = hm5; hm1.Right = hm3; hm1.Bottom = hm2; hm1.Top = hm4;
            hm2.Left = hm5; hm2.Right = hm3; hm2.Bottom = hm6; hm2.Top = hm1;
            hm3.Left = hm2; hm3.Right = hm4; hm3.Bottom = hm6; hm3.Top = hm1;
            hm4.Left = hm3; hm4.Right = hm5; hm4.Bottom = hm6; hm4.Top = hm1;
            hm5.Left = hm4; hm5.Right = hm2; hm5.Bottom = hm6; hm5.Top = hm1;
            hm6.Left = hm5; hm6.Right = hm3; hm6.Bottom = hm4; hm6.Top = hm2;
            hm1.LeftR = 3; hm1.RightR = 1; hm1.BottomR = 0; hm1.TopR = 2;
            hm2.LeftR = 0; hm2.RightR = 0; hm2.BottomR = 0; hm2.TopR = 0;
            hm3.LeftR = 0; hm3.RightR = 0; hm3.BottomR = 1; hm3.TopR = 3;
            hm4.LeftR = 0; hm4.RightR = 0; hm4.BottomR = 2; hm4.TopR = 2;
            hm5.LeftR = 0; hm5.RightR = 0; hm5.BottomR = 3; hm5.TopR = 1;
            hm6.LeftR = 1; hm6.RightR = 3; hm6.BottomR = 2; hm6.TopR = 0;
        }
    }
}