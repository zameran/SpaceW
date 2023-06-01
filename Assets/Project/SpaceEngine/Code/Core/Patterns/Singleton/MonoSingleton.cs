using System.Linq;
using UnityEngine;

namespace SpaceEngine.Core.Patterns.Singleton
{
    /// <summary>
    ///     Be aware this will not prevent a non singleton constructor such as: <code>T instance = new T();</code>
    ///     To prevent that, add protected parameterless constructor to your singleton class.
    /// </summary>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static bool UnsaveSingleton = false;

        private static T m_Instance;
        private static readonly object m_LockObject = new();

        public static T Instance
        {
            get
            {
                if (IsApplicationQuitting)
                {
                    Debug.LogWarning($"Singleton: Instance '{typeof(T).Name}' already destroyed on application quit. Won't create again - returning null.");

                    return null;
                }

                lock (m_LockObject)
                {
                    if (UnsaveSingleton)
                    {
                        return m_Instance;
                    }

                    if (m_Instance == null)
                    {
                        var instances = FindObjectsByType<T>(FindObjectsSortMode.InstanceID).ToList();

                        m_Instance = instances.FirstOrDefault();

                        if (instances.Count > 1)
                        {
                            Debug.LogError($"Singleton: Something went really wrong - there should never be more than 1 singleton! Found count: {instances.Count}");

                            return m_Instance;
                        }

                        if (m_Instance == null)
                        {
                            var singleton = new GameObject();
                            m_Instance = singleton.AddComponent<T>();
                            singleton.name = $"{typeof(T).Name}_(MonoSingleton)";

                            DontDestroyOnLoad(singleton);

                            Debug.Log($"Singleton: An instance of '{typeof(T).Name}' is needed in the scene, so '{singleton.name}' was created with DontDestroyOnLoad.");
                        }
                        else
                        {
                            Debug.Log($"Singleton: Using instance already created: {m_Instance.gameObject.name}");
                        }
                    }

                    return m_Instance;
                }
            }
            protected set => m_Instance = value;
        }

        public static bool IsApplicationQuitting { get; private set; }

        /// <summary>
        ///     When Unity quits, it destroys objects in a random order.
        ///     In principle, a Singleton is only destroyed when application quits.
        ///     If any script calls Instance after it have been destroyed,
        ///     it will create a buggy ghost object that will stay on the Editor scene even after stopping playing the Application.
        ///     So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (UnstableSingleton())
            {
                IsApplicationQuitting = true;
            }
        }

        protected virtual bool UnstableSingleton()
        {
            return false;
        }
    }
}