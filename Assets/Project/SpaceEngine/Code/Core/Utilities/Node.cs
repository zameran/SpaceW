//using Proland;

using UnityEngine;

namespace SpaceEngine.Core.Utilities
{
    /// <summary>
    /// Provides a common interface for nodes.
    /// Also for tile samplers and producers. Provides access to the manager so common data can be shared.
    /// </summary>
    public abstract class Node : MonoBehaviour
    {
        //protected Manager Manager;

        //public TerrainView GetView()
        //{
        //    return Manager.GetController().View;
        //}

        protected virtual void Awake()
        {
            FindManger();
        }

        protected virtual void Start()
        {
            //if (Manager == null) FindManger();
        }

        protected virtual void OnDestroy()
        {

        }

        /// <summary>
        /// Used if the node has data that nees to be drawn by a camera in the OnPostRender function.
        /// </summary>
        public virtual void PostRender()
        {

        }

        private void FindManger()
        {
            var transformComponent = transform;

            while (transformComponent != null)
            {
                //var manager = transformComponent.GetComponent<Manager>();

                //if (manager != null)
                //{
                //    Manager = manager;

                //    break;
                //}

                transformComponent = transformComponent.parent;
            }

            //if (Manager == null)
            //{
            //    Debug.Log("Proland::Node - Could not find manager. This gameObject must be a child of the manager");
            //    Debug.Break();
            //}
        }
    }
}