using UnityEngine;

[RequireComponent(typeof(Transform))]

public class NewtonBody : MonoBehaviour
{
    public Transform _transform;

    public float mass = 1f;

    public Vector3 velocity = Vector3.zero;
    public Vector3 initialForwardSpeed = Vector3.zero;

    void Awake()
    {
        _transform = GetComponent<Transform>();
    }
}