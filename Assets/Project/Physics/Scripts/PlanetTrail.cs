using UnityEngine;

using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]

public class PlanetTrail : MonoBehaviour
{

    LineRenderer _lineRenderer;
    Transform _transform;

    public int maxPositionCount = 1000;
    public float updateRate = 0.1f;

    List<Vector3> trailPositions;

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _transform = GetComponent<Transform>();
    }

    void Start()
    {
        trailPositions = new List<Vector3>();
        StartCoroutine(UpdateTrail());
    }

    IEnumerator UpdateTrail()
    {

        if (trailPositions.Count > maxPositionCount) trailPositions.RemoveAt(0);

        trailPositions.Add(_transform.position);
        _lineRenderer.SetVertexCount(trailPositions.Count);

        for (int p = 0; p < trailPositions.Count; p++)
        {
            _lineRenderer.SetPosition(p, trailPositions[p]);
        }

        yield return new WaitForSeconds(updateRate);

        StartCoroutine(UpdateTrail());
    }
}