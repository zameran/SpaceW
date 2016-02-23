using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

public static class NormalSolver
{
    public static void RecalculateNormals(this Mesh mesh)
    {
        throw new NotImplementedException();
    }

    public static void RecalculateNormals(this Mesh mesh, float angle)
    {
        var triangles = mesh.GetTriangles(0);
        var vertices = mesh.vertices;
        var triNormals = new Vector3[triangles.Length / 3];
        var normals = new Vector3[vertices.Length];

        angle = angle * Mathf.Deg2Rad;

        var dictionary = new Dictionary<VertexKey, VertexEntry>(vertices.Length);

        for (var i = 0; i < triangles.Length; i += 3)
        {
            int i1 = triangles[i];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            Vector3 p1 = vertices[i2] - vertices[i1];
            Vector3 p2 = vertices[i3] - vertices[i1];
            Vector3 normal = Vector3.Cross(p1, p2).normalized;

            int triIndex = i / 3;
            triNormals[triIndex] = normal;

            VertexEntry entry;
            VertexKey key;

            if (!dictionary.TryGetValue(key = new VertexKey(vertices[i1]), out entry))
            {
                entry = new VertexEntry();
                dictionary.Add(key, entry);
            }
            entry.Add(i1, triIndex);

            if (!dictionary.TryGetValue(key = new VertexKey(vertices[i2]), out entry))
            {
                entry = new VertexEntry();
                dictionary.Add(key, entry);
            }
            entry.Add(i2, triIndex);

            if (!dictionary.TryGetValue(key = new VertexKey(vertices[i3]), out entry))
            {
                entry = new VertexEntry();
                dictionary.Add(key, entry);
            }
            entry.Add(i3, triIndex);
        }

        foreach (var value in dictionary.Values)
        {
            for (var i = 0; i < value.Count; ++i)
            {
                var sum = new Vector3();
                for (var j = 0; j < value.Count; ++j)
                {
                    if (value.VertexIndex[i] == value.VertexIndex[j])
                    {
                        sum += triNormals[value.TriangleIndex[j]];
                    }

                    else
                    {
                        float dot = Vector3.Dot(triNormals[value.TriangleIndex[i]],
                                                triNormals[value.TriangleIndex[j]]);

                        dot = Mathf.Clamp(dot, -0.99999f, 0.99999f);

                        float acos = Mathf.Acos(dot);

                        if (acos <= angle)
                        {
                            sum += triNormals[value.TriangleIndex[j]];
                        }
                    }
                }

                normals[value.VertexIndex[i]] = sum.normalized;
            }
        }

        mesh.normals = normals;
    }

    private struct VertexKey
    {
        private readonly long _x;
        private readonly long _y;
        private readonly long _z;

        private const int Tolerance = 100000;

        public VertexKey(Vector3 position)
        {
            _x = (long)(Mathf.Round(position.x * Tolerance));
            _y = (long)(Mathf.Round(position.y * Tolerance));
            _z = (long)(Mathf.Round(position.z * Tolerance));
        }

        public override bool Equals(object obj)
        {
            var key = (VertexKey)obj;
            return _x == key._x && _y == key._y && _z == key._z;
        }

        public override int GetHashCode()
        {
            return (_x * 7 ^ _y * 13 ^ _z * 27).GetHashCode();
        }
    }

    private sealed class VertexEntry
    {
        public int[] TriangleIndex = new int[4];
        public int[] VertexIndex = new int[4];

        private int _reserved = 4;
        private int _count;

        public int Count { get { return _count; } }

        public void Add(int vertIndex, int triIndex)
        {
            if (_reserved == _count)
            {
                _reserved *= 2;
                Array.Resize(ref TriangleIndex, _reserved);
                Array.Resize(ref VertexIndex, _reserved);
            }

            TriangleIndex[_count] = triIndex;
            VertexIndex[_count] = vertIndex;
            ++_count;
        }
    }
}