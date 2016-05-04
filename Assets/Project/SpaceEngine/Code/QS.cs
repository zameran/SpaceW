#region License
/* Procedural planet generator.
 *
 * Copyright (C) 2015-2016 Denis Ovchinnikov
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System;

[Serializable]
public static class QS
{
    public static int nVertsPerEdgeReal { get { return 128; } }
    public static int nVertsPerEdge { get { return 120; } }
    public static int nVertsPerEdgeSubReal { get { return nVertsPerEdgeReal * 2; } }
    public static int nVertsPerEdgeSub { get { return nVertsPerEdge * 2; } }

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

    public static int THREADGROUP_SIZE_X_UNIT { get { return 1; } }
    public static int THREADGROUP_SIZE_Y_UNIT { get { return 1; } }
    public static int THREADGROUP_SIZE_Z_UNIT { get { return 1; } }

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