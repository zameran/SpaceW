using System;

[Serializable]
public static class QS
{
    public static int nRealVertsPerEdge { get { return 128; } }
    public static int nVertsPerEdge { get { return 120; } }
    public static int nRealVerts { get { return nRealVertsPerEdge * nRealVertsPerEdge; } }
    public static int nVerts { get { return nVertsPerEdge * nVertsPerEdge; } }
    public static float nRealSpacing { get { return 2.0f / (nRealVertsPerEdge - 1.0f); } }
    public static float nSpacing { get { return 2.0f / (nVertsPerEdge - 1.0f); } }
    public static int THREADS_PER_GROUP_X { get { return 30; } }
    public static int THREADS_PER_GROUP_Y { get { return 30; } }
    public static int THREADS_PER_GROUP_X_REAL { get { return 32; } }
    public static int THREADS_PER_GROUP_Y_REAL { get { return 32; } }
    public static int THREADGROUP_SIZE_X { get { return nVertsPerEdge / THREADS_PER_GROUP_X; } }
    public static int THREADGROUP_SIZE_Y { get { return nVertsPerEdge / THREADS_PER_GROUP_Y; } }
    public static int THREADGROUP_SIZE_Z { get { return 1; } }
    public static int THREADGROUP_SIZE_X_REAL { get { return nRealVertsPerEdge / THREADS_PER_GROUP_X_REAL; } }
    public static int THREADGROUP_SIZE_Y_REAL { get { return nRealVertsPerEdge / THREADS_PER_GROUP_Y_REAL; } }
    public static int THREADGROUP_SIZE_Z_REAL { get { return 1; } }
}