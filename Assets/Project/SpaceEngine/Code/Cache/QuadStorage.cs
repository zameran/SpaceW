using UnityEngine;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class QuadStorage : MonoBehaviour
{
    public List<QuadTextureCache> Cache = new List<QuadTextureCache>();

    private void OnDestroy()
    {
        foreach (QuadTextureCache q in Cache)
        {
            if (q.HeightTexture != null)
                q.HeightTexture.ReleaseAndDestroy();

            if (q.NormalTexture != null)
                q.NormalTexture.ReleaseAndDestroy();
        }
    }
}