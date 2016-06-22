public enum PatchQuality { Minimum, Low, Standard, High, Maximum };

public enum PatchResolution { Minimum, Low, Standard, High, Maximum };

public enum NeighborDirection { Top = 0, Right = 1, Bottom = 2, Left = 3 };


public class PatchConfig
{
    ushort[] levelHeightMapRes;

    public ushort PatchSize { get; set; }
    public ushort GridSize { get; set; }
    public ushort LevelHeightMapRes(ushort level) { return levelHeightMapRes[(level >= MaxSplitLevel ? MaxSplitLevel : level)]; }
    public ushort MaxSplitLevel { get { return (ushort)(levelHeightMapRes.Length - 1); } }

    public PatchConfig(PatchQuality patchQuality, PatchResolution normalQuality)
    {
        switch (patchQuality)
        {
            case PatchQuality.Maximum:
                {
                    PatchSize = 33;
                    break;
                }

            case PatchQuality.High:
                {
                    PatchSize = 21;
                    break;
                }

            case PatchQuality.Standard:
                {
                    PatchSize = 17;
                    break;
                }

            case PatchQuality.Low:
                {
                    PatchSize = 11;
                    break;
                }

            case PatchQuality.Minimum:
                {
                    PatchSize = 7;
                    break;
                }
        }

        //heightmap resolution at each level
        //starting from level 0 (higher resolution) to highest level (lower resolution).
        //the last entry in the array will be used again in deeper split levels.
        //for example, if you define only 512,256,128 then level 0=512, level 1=256, level 2=128, level 3=128, level 4=128...
        switch (normalQuality)
        {
            case PatchResolution.Maximum:
                {
                    levelHeightMapRes = new ushort[]
                    {
                    512, 256
                    };
                    break;
                }

            case PatchResolution.High:
                {
                    levelHeightMapRes = new ushort[]
                    {
                    256
                    };
                    break;
                }

            case PatchResolution.Standard:
                {
                    levelHeightMapRes = new ushort[]
                    {
                    256, 128
                    };
                    break;
                }

            case PatchResolution.Low:
                {
                    levelHeightMapRes = new ushort[]
                    {
                    128
                    };
                    break;
                }

            case PatchResolution.Minimum:
                {
                    levelHeightMapRes = new ushort[]
                    {
                    128, 64
                    };
                    break;
                }
        }

        GridSize = (ushort)(PatchSize * PatchSize);
    }
}