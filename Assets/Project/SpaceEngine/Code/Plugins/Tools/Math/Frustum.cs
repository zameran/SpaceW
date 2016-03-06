using UnityEngine;

public class Frustum
{
    public enum VISIBILITY { FULLY = 0, PARTIALLY = 1, INVISIBLE = 3 };

    static public Vector4d[] GetFrustumPlanes(Matrix4x4d mat)
    {
        //extract frustum planes from a projection matrix
        Vector4d[] frustumPlanes = new Vector4d[6];

        // Extract the LEFT plane
        frustumPlanes[0] = new Vector4d();
        frustumPlanes[0].x = mat.m[3, 0] + mat.m[0, 0];
        frustumPlanes[0].y = mat.m[3, 1] + mat.m[0, 1];
        frustumPlanes[0].z = mat.m[3, 2] + mat.m[0, 2];
        frustumPlanes[0].w = mat.m[3, 3] + mat.m[0, 3];
        // Extract the RIGHT plane
        frustumPlanes[1] = new Vector4d();
        frustumPlanes[1].x = mat.m[3, 0] - mat.m[0, 0];
        frustumPlanes[1].y = mat.m[3, 1] - mat.m[0, 1];
        frustumPlanes[1].z = mat.m[3, 2] - mat.m[0, 2];
        frustumPlanes[1].w = mat.m[3, 3] - mat.m[0, 3];
        // Extract the BOTTOM plane
        frustumPlanes[2] = new Vector4d();
        frustumPlanes[2].x = mat.m[3, 0] + mat.m[1, 0];
        frustumPlanes[2].y = mat.m[3, 1] + mat.m[1, 1];
        frustumPlanes[2].z = mat.m[3, 2] + mat.m[1, 2];
        frustumPlanes[2].w = mat.m[3, 3] + mat.m[1, 3];
        // Extract the TOP plane
        frustumPlanes[3] = new Vector4d();
        frustumPlanes[3].x = mat.m[3, 0] - mat.m[1, 0];
        frustumPlanes[3].y = mat.m[3, 1] - mat.m[1, 1];
        frustumPlanes[3].z = mat.m[3, 2] - mat.m[1, 2];
        frustumPlanes[3].w = mat.m[3, 3] - mat.m[1, 3];
        // Extract the NEAR plane
        frustumPlanes[4] = new Vector4d();
        frustumPlanes[4].x = mat.m[3, 0] + mat.m[2, 0];
        frustumPlanes[4].y = mat.m[3, 1] + mat.m[2, 1];
        frustumPlanes[4].z = mat.m[3, 2] + mat.m[2, 2];
        frustumPlanes[4].w = mat.m[3, 3] + mat.m[2, 3];
        // Extract the FAR plane
        frustumPlanes[5] = new Vector4d();
        frustumPlanes[5].x = mat.m[3, 0] - mat.m[2, 0];
        frustumPlanes[5].y = mat.m[3, 1] - mat.m[2, 1];
        frustumPlanes[5].z = mat.m[3, 2] - mat.m[2, 2];
        frustumPlanes[5].w = mat.m[3, 3] - mat.m[2, 3];

        return frustumPlanes;
    }

    static public Vector4d[] GetFrustumPlanes(Matrix4x4 mat)
    {
        //extract frustum planes from a projection matrix
        Vector4d[] frustumPlanes = new Vector4d[6];

        // Extract the LEFT plane
        frustumPlanes[0] = new Vector4d();
        frustumPlanes[0].x = mat.m30 + mat.m00;
        frustumPlanes[0].y = mat.m31 + mat.m01;
        frustumPlanes[0].z = mat.m32 + mat.m02;
        frustumPlanes[0].w = mat.m33 + mat.m03;
        // Extract the RIGHT plane
        frustumPlanes[1] = new Vector4d();
        frustumPlanes[1].x = mat.m30 - mat.m00;
        frustumPlanes[1].y = mat.m31 - mat.m01;
        frustumPlanes[1].z = mat.m32 - mat.m02;
        frustumPlanes[1].w = mat.m33 - mat.m03;
        // Extract the BOTTOM plane
        frustumPlanes[2] = new Vector4d();
        frustumPlanes[2].x = mat.m30 + mat.m10;
        frustumPlanes[2].y = mat.m31 + mat.m11;
        frustumPlanes[2].z = mat.m32 + mat.m12;
        frustumPlanes[2].w = mat.m33 + mat.m13;
        // Extract the TOP plane
        frustumPlanes[3] = new Vector4d();
        frustumPlanes[3].x = mat.m30 - mat.m10;
        frustumPlanes[3].y = mat.m31 - mat.m11;
        frustumPlanes[3].z = mat.m32 - mat.m12;
        frustumPlanes[3].w = mat.m33 - mat.m13;
        // Extract the NEAR plane
        frustumPlanes[4] = new Vector4d();
        frustumPlanes[4].x = mat.m30 + mat.m20;
        frustumPlanes[4].y = mat.m31 + mat.m21;
        frustumPlanes[4].z = mat.m32 + mat.m22;
        frustumPlanes[4].w = mat.m33 + mat.m23;
        // Extract the FAR plane
        frustumPlanes[5] = new Vector4d();
        frustumPlanes[5].x = mat.m30 - mat.m20;
        frustumPlanes[5].y = mat.m31 - mat.m21;
        frustumPlanes[5].z = mat.m32 - mat.m22;
        frustumPlanes[5].w = mat.m33 - mat.m23;

        return frustumPlanes;
    }

    static public VISIBILITY GetVisibility(Vector4d[] frustumPlanes, Box3d box)
    {
        VISIBILITY v0 = GetVisibility(frustumPlanes[0], box);
        if (v0 == VISIBILITY.INVISIBLE)
        {
            return VISIBILITY.INVISIBLE;
        }

        VISIBILITY v1 = GetVisibility(frustumPlanes[1], box);
        if (v1 == VISIBILITY.INVISIBLE)
        {
            return VISIBILITY.INVISIBLE;
        }

        VISIBILITY v2 = GetVisibility(frustumPlanes[2], box);
        if (v2 == VISIBILITY.INVISIBLE)
        {
            return VISIBILITY.INVISIBLE;
        }

        VISIBILITY v3 = GetVisibility(frustumPlanes[3], box);
        if (v3 == VISIBILITY.INVISIBLE)
        {
            return VISIBILITY.INVISIBLE;
        }

        VISIBILITY v4 = GetVisibility(frustumPlanes[4], box);
        if (v4 == VISIBILITY.INVISIBLE)
        {
            return VISIBILITY.INVISIBLE;
        }

        if (v0 == VISIBILITY.FULLY && v1 == VISIBILITY.FULLY &&
            v2 == VISIBILITY.FULLY && v3 == VISIBILITY.FULLY &&
            v4 == VISIBILITY.FULLY)
        {
            return VISIBILITY.FULLY;
        }

        return VISIBILITY.PARTIALLY;
    }

    static VISIBILITY GetVisibility(Vector4d clip, Box3d box)
    {
        double x0 = box.xmin * clip.x;
        double x1 = box.xmax * clip.x;
        double y0 = box.ymin * clip.y;
        double y1 = box.ymax * clip.y;
        double z0 = box.zmin * clip.z + clip.w;
        double z1 = box.zmax * clip.z + clip.w;
        double p1 = x0 + y0 + z0;
        double p2 = x1 + y0 + z0;
        double p3 = x1 + y1 + z0;
        double p4 = x0 + y1 + z0;
        double p5 = x0 + y0 + z1;
        double p6 = x1 + y0 + z1;
        double p7 = x1 + y1 + z1;
        double p8 = x0 + y1 + z1;

        if (p1 <= 0 && p2 <= 0 && p3 <= 0 && p4 <= 0 && p5 <= 0 && p6 <= 0 && p7 <= 0 && p8 <= 0)
        {
            return VISIBILITY.INVISIBLE;
        }

        if (p1 > 0 && p2 > 0 && p3 > 0 && p4 > 0 && p5 > 0 && p6 > 0 && p7 > 0 && p8 > 0)
        {
            return VISIBILITY.FULLY;
        }

        return VISIBILITY.PARTIALLY;
    }
}