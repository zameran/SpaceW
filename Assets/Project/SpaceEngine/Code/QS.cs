using System;

[Serializable]
public static class QS
{
    public static int nRealVertsCount { get { return 128; } }
    public static int nVertsPerEdge { get { return 128; } }
    public static int nVerts { get { return nVertsPerEdge * nVertsPerEdge; } }
    public static float nSpacing { get { return 2.0f / (nVertsPerEdge - 1.0f); } }
    public static int THREADS_PER_GROUP_X { get { return 32; } }
    public static int THREADS_PER_GROUP_Y { get { return 32; } }
    public static int THREADGROUP_SIZE_X { get { return nVertsPerEdge / THREADS_PER_GROUP_X; } }
    public static int THREADGROUP_SIZE_Y { get { return nVertsPerEdge / THREADS_PER_GROUP_Y; } }
    public static int THREADGROUP_SIZE_Z { get { return 1; } }
}