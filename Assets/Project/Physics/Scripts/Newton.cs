using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using ZFramework.Unity.Common.PerfomanceMonitor;

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

        /* BENCHMARK
        for (int i = 0; i < 100; i++)
        {
            if (i == 0)
            {
                GameObject go = new GameObject("_BIG" + i);
                go.transform.position = new Vector3(0, 0, 0);
                go.transform.rotation = Quaternion.identity;

                NewtonBody nb = go.AddComponent<NewtonBody>();
                nb.mass = 1000000;
                nb.initialForwardSpeed = new Vector3(0, 0, 0);

                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));

                MeshFilter mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = MeshFactory.MakePlane(2, 2, MeshFactory.PLANE.XY, false, false, false);

                go.AddComponent<PlanetTrail>();
            }
            else
            {
                GameObject go = new GameObject("_NEWTONTEST_" + i);
                go.transform.position = new Vector3(Random.Range(-100, 100),
                                                    Random.Range(-100, 100),
                                                    Random.Range(-100, 100));
                go.transform.rotation = Quaternion.identity;

                NewtonBody nb = go.AddComponent<NewtonBody>();
                nb.mass = Mathf.Abs(Random.Range(-1.1f, 1.1f));
                nb.initialForwardSpeed = new Vector3(Mathf.Abs(Random.Range(-10.1f, 10.1f)),
                                                     Mathf.Abs(Random.Range(-10.1f, 10.1f)),
                                                     Mathf.Abs(Random.Range(-10.1f, 10.1f)));

                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));

                MeshFilter mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = MeshFactory.MakePlane(2, 2, MeshFactory.PLANE.XY, false, false, false);

                go.AddComponent<PlanetTrail>();
            }
        }
        */

        NewtonSetup();
    }

    void FixedUpdate()
    {
        foreach (NewtonBody body in bodies)
        {
            NewtonUpdate(body);
        }

        /* BENCHMARK
        bool useparallelism = true;

        if (!useparallelism)
        {
            using (Timer timer = new Timer("Newton 0"))
            {
                foreach (NewtonBody body in bodies)
                {
                    NewtonUpdate(body);
                }
            }
        }
        else
        {
            using (Timer timer = new Timer("Newton 0"))
            {
                ParallelManager.ForEach(bodies, body =>
                {
                    ThreadScheduler.RunOnMainThread(() =>
                    {
                        NewtonUpdate(body);
                    });
                });
            }
        }
        */
    }

    static void NewtonSetup()
    {
        fixedDeltaTime = Time.fixedDeltaTime;

        bodies = new List<NewtonBody>();
        bodies.AddRange(FindObjectsOfType(typeof(NewtonBody)) as NewtonBody[]);

        //Debug.Log("There are probably " + bodies.Count + " Newton bodies in space (±42).");

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