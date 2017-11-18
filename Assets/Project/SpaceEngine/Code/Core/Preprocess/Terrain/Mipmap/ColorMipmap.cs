using SpaceEngine.Core.Debugging;

using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using Logger = SpaceEngine.Core.Debugging.Logger;

namespace SpaceEngine.Core.Preprocess.Terrain
{
    [UseLogger(LoggerCategory.Core)]
    public class ColorMipmap : AbstractTileCache
    {
        public const int RESIDUAL_STEPS = 2;

        public ColorMipmap Left;
        public ColorMipmap Right;
        public ColorMipmap Bottom;
        public ColorMipmap Top;

        public int LeftR;
        public int RightR;
        public int BottomR;
        public int TopR;

        IColorFunction2D ColorFunction;

        int BaseLevelSize;
        int Size;
        int Border;
        int MaxLevel;
        int CurrentLevel;

        string TempFolder;

        byte[] TileData;

        Dictionary<int, int> ConstantTileIDs;

        public ColorMipmap(IColorFunction2D colorFunction, int baseLevelSize, int tileSize, int border, int channels, string tempFolder) : base(baseLevelSize, baseLevelSize, tileSize, channels, 200)
        {
            ColorFunction = colorFunction;
            BaseLevelSize = baseLevelSize;
            Size = tileSize;
            Border = Mathf.Max(0, border);

            TempFolder = tempFolder;

            MaxLevel = 0;

            var size = BaseLevelSize;

            while (size > this.Size)
            {
                MaxLevel += 1;
                size /= 2;
            }

            TileData = new byte[(this.Size + 2 * this.Border) * (this.Size + 2 * this.Border) * Channels];

            ConstantTileIDs = new Dictionary<int, int>();

            Left = null;
            Right = null;
            Bottom = null;
            Top = null;

            if (!Directory.Exists(TempFolder)) { Directory.CreateDirectory(TempFolder); }

            Logger.Log(string.Format("ColorMipmap.ctor: BaseLevelSize: {0}; TileSize: {1}; Border: {2}; MaxLevel: {3}; Channels: {4}", BaseLevelSize,
                                                                                                                                       Size,
                                                                                                                                       Border,
                                                                                                                                       MaxLevel,
                                                                                                                                       Channels));
        }

        public void Compute()
        {
            BuildBaseLevelTiles();

            for (int level = MaxLevel - 1; level >= 0; --level)
            {
                BuildMipmapLevel(level);
            }
        }

        public void Generate(int rootLevel, int rootTx, int rootTy, string file)
        {
            File.Delete(file);

            var tilesCount = ((1 << (MaxLevel * 2 + 2)) - 1) / 3;

            Logger.Log(string.Format("ColorMipmap.Generate: tiles count: {0}", tilesCount));

            long[] offsets = new long[tilesCount * 2];
            byte[] byteArray = new byte[(7 * 4) + (offsets.Length * 8)];
            long offset = byteArray.Length;

            using (Stream stream = new FileStream(file, FileMode.Create))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Write(byteArray, 0, byteArray.Length);
            }

            for (int l = 0; l <= MaxLevel; ++l)
            {
                ProduceTilesLebeguesOrder(l, 0, 0, 0, ref offset, offsets, file);
            }

            Buffer.BlockCopy(new int[] { MaxLevel }, 0, byteArray, 0, 4);
            Buffer.BlockCopy(new int[] { Size }, 0, byteArray, 4, 4);
            Buffer.BlockCopy(new int[] { Channels }, 0, byteArray, 8, 4);
            Buffer.BlockCopy(new int[] { Border }, 0, byteArray, 12, 4);
            Buffer.BlockCopy(new int[] { rootLevel }, 0, byteArray, 16, 4);
            Buffer.BlockCopy(new int[] { rootTx }, 0, byteArray, 20, 4);
            Buffer.BlockCopy(new int[] { rootTy }, 0, byteArray, 24, 4);
            Buffer.BlockCopy(offsets, 0, byteArray, 28, 8 * offsets.Length);

            using (Stream stream = new FileStream(file, FileMode.Open))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Write(byteArray, 0, byteArray.Length);
            }

            ConstantTileIDs.Clear();

            Logger.Log(string.Format("ColorMipmap.Generate: Saved file path: {0} ", file));
        }

        private string FilePath(string tempFolder, string name, int level, int tx, int ty)
        {
            return string.Format("{0}/{1}-{2}-{3}-{4}.raw", tempFolder, name, level, tx, ty);
        }

        private void SaveTile(string name, int level, int tx, int ty, byte[] tile)
        {
            File.WriteAllBytes(FilePath(TempFolder, name, level, tx, ty), tile);
        }

        private void LoadTile(string name, int level, int tx, int ty, byte[] tile)
        {
            var fileName = FilePath(TempFolder, name, level, tx, ty);

            var fileInfo = new FileInfo(fileName);
            if (fileInfo == null) throw new FileNotFoundException(string.Format("Could not read tile: {0}", fileName));

            using (Stream stream = fileInfo.OpenRead())
            {
                stream.Read(tile, 0, (int)fileInfo.Length);
            }
        }

        protected override float[] ReadTile(int tx, int ty)
        {
            var size = (Size + 2 * Border) * (Size + 2 * Border) * Channels;
            var outputTileData = new float[size];
            var data = new byte[size];

            LoadTile("Base", CurrentLevel, tx, ty, data);

            for (int i = 0; i < size; i++)
            {
                outputTileData[i] = (float)data[i];
            }

            return outputTileData;
        }

        private void BuildBaseLevelTiles()
        {
            int tilesCount = BaseLevelSize / Size;

            Logger.Log(string.Format("ColorMipmap.BuildBaseLevelTiles: Build mipmap level: {0}!", MaxLevel));

            for (int ty = 0; ty < tilesCount; ++ty)
            {
                for (int tx = 0; tx < tilesCount; ++tx)
                {
                    var offset = (int)0;

                    for (int j = -Border; j < Size + Border; ++j)
                    {
                        for (int i = -Border; i < Size + Border; ++i)
                        {
                            var color = ColorFunction.GetValue(tx * Size + i, ty * Size + j) * 255.0f;

                            TileData[offset++] = (byte)Mathf.Round(color.x);

                            if (Channels > 1)
                            {
                                TileData[offset++] = (byte)Mathf.Round(color.y);
                            }

                            if (Channels > 2)
                            {
                                TileData[offset++] = (byte)Mathf.Round(color.z);
                            }

                            if (Channels > 3)
                            {
                                TileData[offset++] = (byte)Mathf.Round(color.w);
                            }
                        }
                    }

                    SaveTile("Base", MaxLevel, tx, ty, TileData);
                }
            }

        }

        void BuildMipmapLevel(int level)
        {
            var tilesCount = 1 << level;

            Logger.Log(string.Format("ColorMipmap.BuildMipmapLevel: Build mipmap level: {0}!", level));

            CurrentLevel = level + 1;

            Reset(Size << CurrentLevel, Size << CurrentLevel, Size);

            for (int ty = 0; ty < tilesCount; ++ty)
            {
                for (int tx = 0; tx < tilesCount; ++tx)
                {
                    var offset = (int)0;

                    for (int j = -Border; j < Size + Border; ++j)
                    {
                        for (int i = -Border; i < Size + Border; ++i)
                        {
                            int ix = 2 * (tx * Size + i);
                            int iy = 2 * (ty * Size + j);

                            Vector4 c1 = GetTileColor(ix, iy);
                            Vector4 c2 = GetTileColor(ix + 1, iy);
                            Vector4 c3 = GetTileColor(ix, iy + 1);
                            Vector4 c4 = GetTileColor(ix + 1, iy + 1);

                            TileData[offset++] = (byte)Mathf.Round((c1.x + c2.x + c3.x + c4.x) / 4.0f);

                            if (Channels > 1)
                            {
                                TileData[offset++] = (byte)Mathf.Round((c1.y + c2.y + c3.y + c4.y) / 4.0f);
                            }

                            if (Channels > 2)
                            {
                                TileData[offset++] = (byte)Mathf.Round((c1.z + c2.z + c3.z + c4.z) / 4.0f);
                            }

                            if (Channels > 3)
                            {
                                float w1 = Mathf.Max(2.0f * c1.w - 255.0f, 0.0f);
                                float n1 = Mathf.Max(255.0f - 2.0f * c1.w, 0.0f);
                                float w2 = Mathf.Max(2.0f * c2.w - 255.0f, 0.0f);
                                float n2 = Mathf.Max(255.0f - 2.0f * c2.w, 0.0f);
                                float w3 = Mathf.Max(2.0f * c3.w - 255.0f, 0.0f);
                                float n3 = Mathf.Max(255.0f - 2.0f * c3.w, 0.0f);
                                float w4 = Mathf.Max(2.0f * c4.w - 255.0f, 0.0f);
                                float n4 = Mathf.Max(255.0f - 2.0f * c4.w, 0.0f);

                                byte w = (byte)Mathf.Round((w1 + w2 + w3 + w4) / 4.0f);
                                byte n = (byte)Mathf.Round((n1 + n2 + n3 + n4) / 4.0f);

                                TileData[offset++] = (byte)(127 + w / 2 - n / 2);
                            }
                        }
                    }

                    SaveTile("Base", level, tx, ty, TileData);
                }
            }

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

                    Logger.LogError("ColorMipmap.Rotation: Something goes wrong!");
                    Debug.Break();

                    break;
            }
        }

        public void ProduceRawTile(int level, int tx, int ty)
        {
            LoadTile("Base", level, tx, ty, TileData);
        }

        private void ProduceTile(int level, int tx, int ty)
        {
            var tilesCount = 1 << level;
            var tileWidth = Size + 2 * Border;

            ProduceRawTile(level, tx, ty);

            if (tx == 0 && Border > 0 && Left != null)
            {
                int txp, typ;

                Rotation(LeftR, tilesCount, tilesCount - 1, ty, out txp, out typ);

                Left.ProduceRawTile(level, txp, typ);

                for (int y = 0; y < tileWidth; ++y)
                {
                    for (int x = 0; x < Border; ++x)
                    {
                        int xp, yp;

                        Rotation(LeftR, tileWidth, Size - x, y, out xp, out yp);

                        for (int c = 0; c < Channels; ++c)
                        {
                            TileData[(x + y * tileWidth) * Channels + c] = Left.TileData[(xp + yp * tileWidth) * Channels + c];
                        }
                    }
                }
            }

            if (tx == tilesCount - 1 && Border > 0 && Right != null)
            {
                int txp, typ;

                Rotation(RightR, tilesCount, 0, ty, out txp, out typ);

                Right.ProduceRawTile(level, txp, typ);

                for (int y = 0; y < tileWidth; ++y)
                {
                    for (int x = Size + Border; x < tileWidth; ++x)
                    {
                        int xp, yp;

                        Rotation(RightR, tileWidth, x - Size, y, out xp, out yp);

                        for (int c = 0; c < Channels; ++c)
                        {
                            TileData[(x + y * tileWidth) * Channels + c] = Right.TileData[(xp + yp * tileWidth) * Channels + c];
                        }
                    }
                }
            }

            if (ty == 0 && Border > 0 && Bottom != null)
            {
                int txp, typ;

                Rotation(BottomR, tilesCount, tx, tilesCount - 1, out txp, out typ);

                Bottom.ProduceRawTile(level, txp, typ);

                for (int y = 0; y < Border; ++y)
                {
                    for (int x = 0; x < tileWidth; ++x)
                    {
                        int xp, yp;

                        Rotation(BottomR, tileWidth, x, Size - y, out xp, out yp);

                        for (int c = 0; c < Channels; ++c)
                        {
                            TileData[(x + y * tileWidth) * Channels + c] = Bottom.TileData[(xp + yp * tileWidth) * Channels + c];
                        }
                    }
                }
            }

            if (ty == tilesCount - 1 && Border > 0 && Top != null)
            {
                int txp, typ;

                Rotation(TopR, tilesCount, tx, 0, out txp, out typ);

                Top.ProduceRawTile(level, txp, typ);

                for (int y = Size + Border; y < tileWidth; ++y)
                {
                    for (int x = 0; x < tileWidth; ++x)
                    {
                        int xp, yp;

                        Rotation(TopR, tileWidth, x, y - Size, out xp, out yp);

                        for (int c = 0; c < Channels; ++c)
                        {
                            TileData[(x + y * tileWidth) * Channels + c] = Top.TileData[(xp + yp * tileWidth) * Channels + c];
                        }
                    }
                }
            }

            if (tx == 0 && ty == 0 && Border > 0 && Left != null && Bottom != null)
            {
                for (int c = 0; c < Channels; ++c)
                {
                    int x1 = Border;
                    int y1 = Border;
                    int x2;
                    int y2;
                    int x3;
                    int y3;

                    Rotation(LeftR, tileWidth, tileWidth - 1 - Border, Border, out x2, out y2);
                    Rotation(BottomR, tileWidth, Border, tileWidth - 1 - Border, out x3, out y3);

                    int corner1 = TileData[(x1 + y1 * tileWidth) * Channels + c];
                    int corner2 = Left.TileData[(x2 + y2 * tileWidth) * Channels + c];
                    int corner3 = Bottom.TileData[(x3 + y3 * tileWidth) * Channels + c];
                    int corner = (corner1 + corner2 + corner3) / 3;

                    for (int y = 0; y < 2 * Border; ++y)
                    {
                        for (int x = 0; x < 2 * Border; ++x)
                        {
                            TileData[(x + y * tileWidth) * Channels + c] = (byte)corner;
                        }
                    }
                }
            }

            if (tx == tilesCount - 1 && ty == 0 && Border > 0 && Right != null && Bottom != null)
            {
                for (int c = 0; c < Channels; ++c)
                {
                    int x1 = tileWidth - 1 - Border;
                    int y1 = Border;
                    int x2;
                    int y2;
                    int x3;
                    int y3;

                    Rotation(RightR, tileWidth, Border, Border, out x2, out y2);
                    Rotation(BottomR, tileWidth, tileWidth - 1 - Border, tileWidth - 1 - Border, out x3, out y3);

                    int corner1 = TileData[(x1 + y1 * tileWidth) * Channels + c];
                    int corner2 = Right.TileData[(x2 + y2 * tileWidth) * Channels + c];
                    int corner3 = Bottom.TileData[(x3 + y3 * tileWidth) * Channels + c];
                    int corner = (corner1 + corner2 + corner3) / 3;

                    for (int y = 0; y < 2 * Border; ++y)
                    {
                        for (int x = Size; x < tileWidth; ++x)
                        {
                            TileData[(x + y * tileWidth) * Channels + c] = (byte)corner;
                        }
                    }
                }
            }

            if (tx == 0 && ty == tilesCount - 1 && Border > 0 && Left != null && Top != null)
            {
                for (int c = 0; c < Channels; ++c)
                {
                    int x1 = Border;
                    int y1 = tileWidth - 1 - Border;
                    int x2;
                    int y2;
                    int x3;
                    int y3;

                    Rotation(LeftR, tileWidth, tileWidth - 1 - Border, tileWidth - 1 - Border, out x2, out y2);
                    Rotation(TopR, tileWidth, Border, Border, out x3, out y3);

                    int corner1 = TileData[(x1 + y1 * tileWidth) * Channels + c];
                    int corner2 = Left.TileData[(x2 + y2 * tileWidth) * Channels + c];
                    int corner3 = Top.TileData[(x3 + y3 * tileWidth) * Channels + c];
                    int corner = (corner1 + corner2 + corner3) / 3;

                    for (int y = Size; y < tileWidth; ++y)
                    {
                        for (int x = 0; x < 2 * Border; ++x)
                        {
                            TileData[(x + y * tileWidth) * Channels + c] = (byte)corner;
                        }
                    }
                }
            }

            if (tx == tilesCount - 1 && ty == tilesCount - 1 && Border > 0 && Right != null && Top != null)
            {
                for (int c = 0; c < Channels; ++c)
                {
                    int x1 = tileWidth - 1 - Border;
                    int y1 = tileWidth - 1 - Border;
                    int x2;
                    int y2;
                    int x3;
                    int y3;

                    Rotation(RightR, tileWidth, Border, tileWidth - 1 - Border, out x2, out y2);
                    Rotation(TopR, tileWidth, tileWidth - 1 - Border, Border, out x3, out y3);

                    int corner1 = TileData[(x1 + y1 * tileWidth) * Channels + c];
                    int corner2 = Right.TileData[(x2 + y2 * tileWidth) * Channels + c];
                    int corner3 = Top.TileData[(x3 + y3 * tileWidth) * Channels + c];
                    int corner = (corner1 + corner2 + corner3) / 3;

                    for (int y = Size; y < tileWidth; ++y)
                    {
                        for (int x = Size; x < tileWidth; ++x)
                        {
                            TileData[(x + y * tileWidth) * Channels + c] = (byte)corner;
                        }
                    }
                }
            }
        }

        private void ProduceTile(int level, int tx, int ty, ref long offset, long[] offsets, string file)
        {
            Logger.Log(string.Format("ColorMipmap.ProduceTile: Producing tile {0}:{1}:{2}!", level, tx, ty));

            ProduceTile(level, tx, ty);

            var tileID = tx + ty * (1 << level) + ((1 << (2 * level)) - 1) / 3;
            var isConstant = true;
            var constantValue = TileData[0];

            for (int i = 1; i < (Size + 2 * Border) * (Size + 2 * Border); ++i)
            {
                if (TileData[i] != TileData[i - 1])
                {
                    isConstant = false;

                    break;
                }
            }

            if (isConstant && ConstantTileIDs.ContainsKey(constantValue))
            {
                Logger.Log("ColorMipmap.ProduceTile: tile is const (All same value)!");

                var constantId = ConstantTileIDs[constantValue];

                offsets[2 * tileID] = offsets[2 * constantId];
                offsets[2 * tileID + 1] = offsets[2 * constantId + 1];
            }
            else
            {
                using (Stream stream = new FileStream(file, FileMode.Open))
                {
                    stream.Seek(offset, SeekOrigin.Begin);
                    stream.Write(TileData, 0, TileData.Length);
                }

                offsets[2 * tileID] = offset;
                offset += TileData.Length;
                offsets[2 * tileID + 1] = offset;
            }

            if (isConstant && !ConstantTileIDs.ContainsKey(constantValue))
            {
                ConstantTileIDs.Add(constantValue, tileID);
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
                ProduceTile(level, tx, ty, ref offset, offsets, file);
            }
        }

        public void SetCube(ColorMipmap hm1, ColorMipmap hm2, ColorMipmap hm3, ColorMipmap hm4, ColorMipmap hm5, ColorMipmap hm6)
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