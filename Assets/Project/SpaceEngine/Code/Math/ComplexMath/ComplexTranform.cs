using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class ComplexTranform : MonoBehaviour
{
    public Transform Transform { get { return this.transform; } }

    public Vector3d RelativePosition { get { return this.transform.position; } set { this.transform.position = value; } }
    public Vector3d AbsolutePosition = Vector3d.zero;

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

    public void CalculateComplexPosition(float rim)
    {

    }
}