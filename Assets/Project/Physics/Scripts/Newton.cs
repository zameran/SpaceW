using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class Newton : MonoBehaviour
{
    // Gravitational constant
    public float g = 0.01f;

    static List<NewtonBody> bodies;

    static Vector3 acceleration;
    static Vector3 direction;

    static float fixedDeltaTime;

    static Newton self;

    void Start()
    {
        self = this;

        NewtonSetup();
    }

    void FixedUpdate()
    {
        foreach (NewtonBody body in bodies)
        {
            NewtonUpdate(body);
        }
    }

    static void NewtonSetup()
    {
        fixedDeltaTime = Time.fixedDeltaTime;

        bodies = new List<NewtonBody>();
        bodies.AddRange(FindObjectsOfType(typeof(NewtonBody)) as NewtonBody[]);

        foreach (NewtonBody body in bodies)
        {
            body.velocity = body._transform.TransformDirection(body.initialForwardSpeed);
        }
    }

    static void NewtonUpdate(NewtonBody body)
    {
        acceleration = Vector3.zero;

        foreach (NewtonBody otherBody in bodies)
        {
            if (body == otherBody) continue;

            direction = (otherBody._transform.position - body._transform.position);
            acceleration += self.g * (direction.normalized * otherBody.mass) / direction.sqrMagnitude;
        }

        body.velocity += acceleration * fixedDeltaTime;
        body._transform.position += body.velocity * fixedDeltaTime;
    }
}