using System;

[Serializable]
public static class QS
{
    public static int nVertsPerEdgeReal { get { return 64; } }
    public static int nVertsPerEdge { get { return 60; } }
    public static int nVertsPerEdgeSubReal { get { return nVertsPerEdgeReal * 4; } }
    public static int nVertsPerEdgeSub { get { return nVertsPerEdge * 4; } }

    public static int nVertsReal { get { return nVertsPerEdgeReal * nVertsPerEdgeReal; } }
    public static int nVerts { get { return nVertsPerEdge * nVertsPerEdge; } }
    public static int nRealVertsSub { get { return nVertsPerEdgeSubReal * nVertsPerEdgeSubReal; } }
    public static int nVertsSub { get { return nVertsPerEdgeSub * nVertsPerEdgeSub; } }

    public static float nSpacingReal { get { return 2.0f / (nVertsPerEdgeReal - 1.0f); } }
    public static float nSpacing { get { return 2.0f / (nVertsPerEdge - 1.0f); } }
    public static float nSpacingSub { get { return 2.0f / (nVertsPerEdgeSub - 1.0f); } }

    public static int THREADS_PER_GROUP_X { get { return 30; } }
    public static int THREADS_PER_GROUP_Y { get { return 30; } }

    public static int THREADS_PER_GROUP_X_REAL { get { return 32; } }
    public static int THREADS_PER_GROUP_Y_REAL { get { return 32; } }

    public static int THREADGROUP_SIZE_X { get { return nVertsPerEdge / THREADS_PER_GROUP_X; } }
    public static int THREADGROUP_SIZE_Y { get { return nVertsPerEdge / THREADS_PER_GROUP_Y; } }
    public static int THREADGROUP_SIZE_Z { get { return 1; } }

    public static int THREADGROUP_SIZE_X_REAL { get { return nVertsPerEdgeReal / THREADS_PER_GROUP_X_REAL; } }
    public static int THREADGROUP_SIZE_Y_REAL { get { return nVertsPerEdgeReal / THREADS_PER_GROUP_Y_REAL; } }
    public static int THREADGROUP_SIZE_Z_REAL { get { return 1; } }

    public static int THREADGROUP_SIZE_X_SUB { get { return nVertsPerEdgeSub / THREADS_PER_GROUP_X; } }
    public static int THREADGROUP_SIZE_Y_SUB { get { return nVertsPerEdgeSub / THREADS_PER_GROUP_Y; } }
    public static int THREADGROUP_SIZE_Z_SUB { get { return 1; } }

    public static int THREADGROUP_SIZE_X_SUB_REAL { get { return nVertsPerEdgeSubReal / THREADS_PER_GROUP_X_REAL; } }
    public static int THREADGROUP_SIZE_Y_SUB_REAL { get { return nVertsPerEdgeSubReal / THREADS_PER_GROUP_Y_REAL; } }
    public static int THREADGROUP_SIZE_Z_SUB_REAL { get { return 1; } }
}