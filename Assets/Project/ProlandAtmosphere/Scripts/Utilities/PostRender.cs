using UnityEngine;

using System.Collections.Generic;

namespace Proland
{
    [RequireComponent(typeof(Camera))]
    public class PostRender : MonoBehaviour
    {
        [SerializeField]
        GameObject[] postRendersGO;
        List<Node> postRenders = new List<Node>();

        void Start()
        {
            foreach (GameObject go in postRendersGO)
            {
                Node n = go.GetComponent<Node>();

                if (n != null)
                    postRenders.Add(n);
                else
                    Debug.Log("Proland::PostRender::Start - Attached game object does not contain a Node component");
            }
        }

        void OnPostRender()
        {
            foreach (Node n in postRenders)
            {
                if (n.gameObject.activeInHierarchy)
                    n.PostRender();
            }
        }
    }
}