using UnityEngine;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class QuadStorage : MonoBehaviour
{
    public bool Multithreaded = false;

    public List<QuadFullCache> Cache = new List<QuadFullCache>();

    private void OnDestroy()
    {
        foreach (QuadFullCache q in Cache)
        {
            if (q != null)
                q.OnDestroy();
        }
    }
}