using NUnit.Framework;

using System;

using Random = System.Random;

public class NumericsMigrationUnitTest
{
    public static readonly double X = Math.PI;
    public static readonly double Y = Math.E;
    public static readonly double Z = Math.PI * Math.E;
    public static readonly double W = Math.PI * Math.E * Math.E;

    public static readonly Random Random = new Random(0);

    [Test]
    public void Matrix2x2dInitIdentity()
    {
        var matrix1 = UnityEngine.Matrix2x2d.identity;
        var matrix2 = SpaceEngine.Core.Numerics.Matrix2x2d.Identity;

        Iterate2D(2, 2, (i, j) => Assert.IsTrue(BrainFuckMath.NearlyEqual(matrix1.m[i, j], matrix2[i, j])));
    }

    [Test]
    public void Matrix2x2dInitRandom()
    {
        UnityEngine.Matrix2x2d matrix1;
        SpaceEngine.Core.Numerics.Matrix2x2d matrix2;

        InitializeRandomMatrix2x2d(out matrix1, out matrix2);

        Iterate2D(2, 2, (i, j) => Assert.IsTrue(BrainFuckMath.NearlyEqual(matrix1.m[i, j], matrix2[i, j])));
    }

    [Test]
    public void Matrix2x2Inverse()
    {
        UnityEngine.Matrix2x2d matrix1;
        SpaceEngine.Core.Numerics.Matrix2x2d matrix2;

        InitializeRandomMatrix2x2d(out matrix1, out matrix2);

        var result1 = matrix1.Inverse();
        var result2 = SpaceEngine.Core.Numerics.Matrix.Inverse(matrix2);

        Iterate2D(2, 2, (i, j) => Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.m[i, j], result2[i, j])));
    }

    [Test]
    public void Matrix2x2Determinant()
    {
        UnityEngine.Matrix2x2d matrix1;
        SpaceEngine.Core.Numerics.Matrix2x2d matrix2;

        InitializeRandomMatrix2x2d(out matrix1, out matrix2);

        var result1 = matrix1.Determinant();
        var result2 = SpaceEngine.Core.Numerics.Matrix.Determinant(matrix2);

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1, result2));
    }

    [Test]
    public void Matrix3x3Determinant()
    {
        UnityEngine.Matrix3x3d matrix1;
        SpaceEngine.Core.Numerics.Matrix3x3d matrix2;

        InitializeRandomMatrix3x3d(out matrix1, out matrix2);

        var result1 = matrix1.Determinant();
        var result2 = SpaceEngine.Core.Numerics.Matrix.Determinant(matrix2);

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1, result2));
    }

    [Test]
    public void Matrix4x4dInitIdentity()
    {
        var matrix1 = UnityEngine.Matrix4x4d.identity;
        var matrix2 = SpaceEngine.Core.Numerics.Matrix4x4d.Identity;

        Iterate2D(4, 4, (i, j) => Assert.IsTrue(BrainFuckMath.NearlyEqual(matrix1.m[i, j], matrix2[i, j])));
    }

    [Test]
    public void Matrix4x4dInitRandom()
    {
        UnityEngine.Matrix4x4d matrix1;
        SpaceEngine.Core.Numerics.Matrix4x4d matrix2;

        InitializeRandomMatrix4x4d(out matrix1, out matrix2);

        Iterate2D(4, 4, (i, j) => Assert.IsTrue(BrainFuckMath.NearlyEqual(matrix1.m[i, j], matrix2[i, j])));
    }

    [Test]
    public void Matrix4x4Inverse()
    {
        UnityEngine.Matrix4x4d matrix1;
        SpaceEngine.Core.Numerics.Matrix4x4d matrix2;

        InitializeRandomMatrix4x4d(out matrix1, out matrix2);

        var result1 = matrix1.Inverse();
        var result2 = SpaceEngine.Core.Numerics.Matrix.Inverse(matrix2);

        // NOTE : INVERTED INVERSE?!
        Iterate2D(4, 4, (i, j) => Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.m[i, j], result2[j, i])));
    }

    [Test]
    public void Matrix4x4Determinant()
    {
        UnityEngine.Matrix4x4d matrix1;
        SpaceEngine.Core.Numerics.Matrix4x4d matrix2;

        InitializeRandomMatrix4x4d(out matrix1, out matrix2);

        var result1 = matrix1.Determinant();
        var result2 = SpaceEngine.Core.Numerics.Matrix.Determinant(matrix2);

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1, result2));
    }

    [Test]
    public void Matrix4x4Rotate()
    {
        UnityEngine.Matrix4x4d matrix1;
        SpaceEngine.Core.Numerics.Matrix4x4d matrix2;

        InitializeRandomMatrix4x4d(out matrix1, out matrix2);

        var vector1 = new UnityEngine.Vector3d(X, Y, Z);
        var vector2 = new SpaceEngine.Core.Numerics.Vector3d(X, Y, Z);

        var result1 = UnityEngine.Matrix4x4d.Rotate(vector1);
        var result2 = SpaceEngine.Core.Numerics.Matrix.Rotate(vector2);

        Iterate2D(4, 4, (i, j) => Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.m[i, j], result2[i, j])));
    }

    [Test]
    public void Vector2dSimpleMath()
    {
        var vector1 = new UnityEngine.Vector2d(X, Y);
        var vector2 = new SpaceEngine.Core.Numerics.Vector2d(X, Y);

        var result1 = vector1 * Z;
        var result2 = vector2 * Z;

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.x, result2.X));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.y, result2.Y));

        result1 = vector1 + vector1;
        result2 = vector2 + vector2;

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.x, result2.X));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.y, result2.Y));
    }

    [Test]
    public void Vector3dSimpleMath()
    {
        var vector1 = new UnityEngine.Vector3d(X, Y, Z);
        var vector2 = new SpaceEngine.Core.Numerics.Vector3d(X, Y, Z);

        var result1 = vector1 * Z;
        var result2 = vector2 * Z;

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.x, result2.X));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.y, result2.Y));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.z, result2.Z));

        result1 = vector1 + vector1;
        result2 = vector2 + vector2;

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.x, result2.X));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.y, result2.Y));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.z, result2.Z));
    }

    [Test]
    public void Vector4dSimpleMath()
    {
        var vector1 = new UnityEngine.Vector4d(X, Y, Z, W);
        var vector2 = new SpaceEngine.Core.Numerics.Vector4d(X, Y, Z, W);

        var result1 = vector1 * Z;
        var result2 = vector2 * Z;

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.x, result2.X));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.y, result2.Y));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.z, result2.Z));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.w, result2.W));

        result1 = vector1 + vector1;
        result2 = vector2 + vector2;

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.x, result2.X));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.y, result2.Y));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.z, result2.Z));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.w, result2.W));
    }

    [Test]
    public void Vector2dMultiplyByMatrix()
    {
        var matrix1 = UnityEngine.Matrix2x2d.one * Z;
        var matrix2 = SpaceEngine.Core.Numerics.Matrix2x2d.One * Z;

        var vector1 = new UnityEngine.Vector2d(X, Y);
        var vector2 = new SpaceEngine.Core.Numerics.Vector2d(X, Y);

        var result1 = matrix1 * vector1;
        var result2 = matrix2 * vector2;

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.x, result2.X));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.y, result2.Y));

        result1 = matrix1 * vector1;
        result2 = vector2 * matrix2;

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.x, result2.X));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.y, result2.Y));
    }

    [Test]
    public void Vector3dMultiplyByMatrix()
    {
        var matrix1 = UnityEngine.Matrix3x3d.one * Z;
        var matrix2 = SpaceEngine.Core.Numerics.Matrix3x3d.One * Z;

        var vector1 = new UnityEngine.Vector3d(X, Y, Z);
        var vector2 = new SpaceEngine.Core.Numerics.Vector3d(X, Y, Z);

        var result1 = matrix1 * vector1;
        var result2 = matrix2 * vector2;

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.x, result2.X));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.y, result2.Y));

        result1 = matrix1 * vector1;
        result2 = vector2 * matrix2;

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.x, result2.X));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.y, result2.Y));
    }

    [Test]
    public void Vector4dMultiplyByMatrix()
    {
        var matrix1 = UnityEngine.Matrix4x4d.one * Z;
        var matrix2 = SpaceEngine.Core.Numerics.Matrix4x4d.One * Z;

        var vector1 = new UnityEngine.Vector4d(X, Y, Z, W);
        var vector2 = new SpaceEngine.Core.Numerics.Vector4d(X, Y, Z, W);

        var result1 = matrix1 * vector1;
        var result2 = matrix2 * vector2;

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.x, result2.X));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.y, result2.Y));

        result1 = matrix1 * vector1;
        result2 = vector2 * matrix2;

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.x, result2.X));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.y, result2.Y));
    }

    [Test]
    public void VectorMultiplyByMatrix()
    {
        var vector1 = UnityEngine.Vector4d.zero;
        var vector2 = SpaceEngine.Core.Numerics.Vector4d.Zero;
        var vector3 = UnityEngine.Vector3d.zero;
        var vector4 = SpaceEngine.Core.Numerics.Vector3d.Zero;

        var matrix1 = UnityEngine.Matrix4x4d.one * Z;
        var matrix2 = SpaceEngine.Core.Numerics.Matrix4x4d.One * Z;

        var result1 = matrix1 * vector1;
        var result2 = matrix2 * vector2;
        var result3 = matrix1 * vector3;
        var result4 = matrix2 * vector4;

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.x, result2.X));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.y, result2.Y));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.z, result2.Z));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.w, result2.W));

        Assert.IsFalse(BrainFuckMath.NearlyEqual(result3.x, result4.X));
        Assert.IsFalse(BrainFuckMath.NearlyEqual(result3.y, result4.Y));
        Assert.IsFalse(BrainFuckMath.NearlyEqual(result3.z, result4.Z));

        var result5 = matrix1 * vector3;
        var result6 = SpaceEngine.Core.Numerics.Vector.Mul(matrix2, vector4);

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result5.x, result6.X));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result5.y, result6.Y));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result5.z, result6.Z));
    }

    [Test]
    public void QuaternionSimpleMath()
    {
        var quaternion1 = new UnityEngine.Quaternion4d(X, Y, Z, W);
        var quaternion2 = new SpaceEngine.Core.Numerics.Geometry.Quaterniond(X, Y, Z, W);
        var quaternion3 = new UnityEngine.Quaternion4d(W, Z, Y, X);
        var quaternion4 = new SpaceEngine.Core.Numerics.Geometry.Quaterniond(W, Z, Y, X);

        Assert.IsTrue(BrainFuckMath.NearlyEqual(quaternion1.x, quaternion2.A));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(quaternion1.y, quaternion2.B));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(quaternion1.z, quaternion2.C));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(quaternion1.w, quaternion2.D));
        
        var result1 = quaternion1 * quaternion3;
        var result2 = SpaceEngine.Core.Numerics.Geometry.Quaternion.Mul(quaternion2, quaternion4);

        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.x, result2.A));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.y, result2.B));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.z, result2.C));
        Assert.IsTrue(BrainFuckMath.NearlyEqual(result1.w, result2.D));

        var result3 = quaternion1.ToMatrix4x4d();
        var result4 = SpaceEngine.Core.Numerics.Geometry.Quaternion.ToMatrix4x4d(quaternion2);

        Iterate2D(4, 4, (i, j) => Assert.IsTrue(BrainFuckMath.NearlyEqual(result3.m[i, j], result4[i, j])));
    }

    [Test]
    public void TestAPI()
    {
        UnityEngine.Matrix4x4d matrix1;
        SpaceEngine.Core.Numerics.Matrix4x4d matrix2;
        UnityEngine.Matrix3x3d matrix3;
        SpaceEngine.Core.Numerics.Matrix3x3d matrix4;
        UnityEngine.Matrix2x2d matrix5;
        SpaceEngine.Core.Numerics.Matrix2x2d matrix6;

        InitializeRandomMatrix4x4d(out matrix1, out matrix2);
        InitializeRandomMatrix3x3d(out matrix3, out matrix4);
        InitializeRandomMatrix2x2d(out matrix5, out matrix6);

        Iterate2D(4, 4, (i, j) => Assert.IsTrue(BrainFuckMath.NearlyEqual(matrix1.m[i, j], matrix2[i, j])));
        Iterate2D(3, 3, (i, j) => Assert.IsTrue(BrainFuckMath.NearlyEqual(matrix3.m[i, j], matrix4[i, j])));
        Iterate2D(2, 2, (i, j) => Assert.IsTrue(BrainFuckMath.NearlyEqual(matrix5.m[i, j], matrix6[i, j])));
    }

    #region API

    public void InitializeRandomMatrix2x2d(out UnityEngine.Matrix2x2d oldMatrix, out SpaceEngine.Core.Numerics.Matrix2x2d newMatrix)
    {
        var randomArray = InitializeRandomArray(2, 2);

        oldMatrix = new UnityEngine.Matrix2x2d(randomArray[0, 0], randomArray[0, 1], 
                                               randomArray[1, 0], randomArray[1, 1]);
        newMatrix = new SpaceEngine.Core.Numerics.Matrix2x2d(randomArray[0, 0], randomArray[0, 1], 
                                                             randomArray[1, 0], randomArray[1, 1]);
    }

    public void InitializeRandomMatrix3x3d(out UnityEngine.Matrix3x3d oldMatrix, out SpaceEngine.Core.Numerics.Matrix3x3d newMatrix)
    {
        var randomArray = InitializeRandomArray(3, 3);

        oldMatrix = new UnityEngine.Matrix3x3d(randomArray[0, 0], randomArray[0, 1], randomArray[0, 2],
                                               randomArray[1, 0], randomArray[1, 1], randomArray[1, 2],
                                               randomArray[2, 0], randomArray[2, 1], randomArray[2, 2]);
        newMatrix = new SpaceEngine.Core.Numerics.Matrix3x3d(randomArray[0, 0], randomArray[0, 1], randomArray[0, 2],
                                                             randomArray[1, 0], randomArray[1, 1], randomArray[1, 2],
                                                             randomArray[2, 0], randomArray[2, 1], randomArray[2, 2]);
    }

    public void InitializeRandomMatrix4x4d(out UnityEngine.Matrix4x4d oldMatrix, out SpaceEngine.Core.Numerics.Matrix4x4d newMatrix)
    {
        var randomArray = InitializeRandomArray(4, 4);

        oldMatrix = new UnityEngine.Matrix4x4d(randomArray[0, 0], randomArray[0, 1], randomArray[0, 2], randomArray[0, 3],
                                               randomArray[1, 0], randomArray[1, 1], randomArray[1, 2], randomArray[1, 3],
                                               randomArray[2, 0], randomArray[2, 1], randomArray[2, 2], randomArray[2, 3],
                                               randomArray[3, 0], randomArray[3, 1], randomArray[3, 2], randomArray[3, 3]);
        newMatrix = new SpaceEngine.Core.Numerics.Matrix4x4d(randomArray[0, 0], randomArray[0, 1], randomArray[0, 2], randomArray[0, 3],
                                                             randomArray[1, 0], randomArray[1, 1], randomArray[1, 2], randomArray[1, 3],
                                                             randomArray[2, 0], randomArray[2, 1], randomArray[2, 2], randomArray[2, 3],
                                                             randomArray[3, 0], randomArray[3, 1], randomArray[3, 2], randomArray[3, 3]);
    }

    public double[,] InitializeRandomArray(int x = 0, int y = 0)
    {
        var randomArray = new double[x, y];

        Iterate2D(x, y, (i, j) => randomArray[i, j] = Random.NextDouble());

        return randomArray;
    }

    public void Iterate2D(int x = 0, int y = 0, Action<int, int> body = null)
    {
        for (var i = 0; i < x; i++)
        {
            for (var j = 0; j < y; j++)
            {
                if (body != null) body(i, j);
            }
        }
    }

    #endregion
}