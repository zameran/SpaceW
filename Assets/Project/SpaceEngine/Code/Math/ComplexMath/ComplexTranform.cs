using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class ComplexTranform : MonoBehaviour
{
    public Transform Transform { get { return this.transform; } }

    public Vector3 RelativePosition { get { return this.transform.position; } set { this.transform.position = value; } }
    public Vector3 AbsolutePosition = Vector3.zero;

    public ComplexVector Position;

    private void Update()
    {

    }

    public void SetComplexPostion(Vector3d offset)
    {
        Position = new ComplexVector(new Complex(offset.x, Position.x.Imaginary), 
                                     new Complex(offset.y, Position.y.Imaginary), 
                                     new Complex(offset.z, Position.z.Imaginary));
    }

    public void IncrementComplexPosition(float rim)
    {

    }
}