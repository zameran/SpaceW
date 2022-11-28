﻿using SpaceEngine.Core.Debugging;
using SpaceEngine.Core.Patterns.Strategy.Eventit;
using SpaceEngine.Enums;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

using Logger = SpaceEngine.Core.Debugging.Logger;

namespace SpaceEngine.UI
{
    [UseLogger(LoggerCategory.InGameUI)]
    public class UICore : MonoBehaviour, IEventit
    {
        public GameObject Root => this.gameObject;

        public List<UIPanel> Panels { get { return this.GetComponentsInChildren<UIPanel>().Where(x => x.transform.parent == transform).ToList(); } }

        public bool ForceDestroyAllPanels = false;

        private static UICore Instance = null;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                Eventit();

                EventManager.UIEvents.UIRemixed.Invoke(Instance);
            }
            else
            {
                Panels.ForEach(panel =>
                {
                    panel.transform.SetParent(Instance.gameObject.transform);

                    panel.transform.localScale = Vector3.one;
                    panel.GetComponent<RectTransform>().offsetMax = Vector2.zero;
                    panel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                });

                DestroyImmediate(this.gameObject);
            }
        }

        private void OnDestroy()
        {
            UnEventit();
        }

        #region API

        public void RemixUI(bool forceDestroyAllPanels)
        {
            if (forceDestroyAllPanels)
            {
                Panels.ForEach(panel =>
                {
                    if (!panel.Immune && !panel.FromThisScene)
                    {
                        DestroyImmediate(panel.gameObject);
                    }
                });
            }
        }

        #endregion

        #region IEventit

        public bool IsEventit { get; set; }

        public void Eventit()
        {
            if (IsEventit) return;

            EventManager.UIEvents.AllAdditiveUILoaded.OnEvent += OnAllAdditiveUILoaded;
            EventManager.UIEvents.UIRemixed.OnEvent += OnUIRemixed;

            EventManager.BaseEvents.OnSceneLoaded.OnEvent += OnLevelLoaded;
            EventManager.BaseEvents.OnSceneWillBeLoadedNow.OnEvent += OnSceneWillBeLoadedNow;

            IsEventit = true;
        }

        public void UnEventit()
        {
            if (!IsEventit) return;

            EventManager.UIEvents.AllAdditiveUILoaded.OnEvent -= OnAllAdditiveUILoaded;
            EventManager.UIEvents.UIRemixed.OnEvent -= OnUIRemixed;

            EventManager.BaseEvents.OnSceneLoaded.OnEvent -= OnLevelLoaded;
            EventManager.BaseEvents.OnSceneWillBeLoadedNow.OnEvent -= OnSceneWillBeLoadedNow;

            IsEventit = false;
        }

        #endregion

        #region Events

        private void OnLevelLoaded(EntryPoint scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
                RemixUI(ForceDestroyAllPanels);
        }

        private void OnAllAdditiveUILoaded()
        {
            var loadingScreenPanel = Panels.Find(panel => panel.gameObject.name == "Loading Panel");

            if (loadingScreenPanel != null)
            {
                loadingScreenPanel.Hide(0);
            }
        }

        private void OnUIRemixed(UICore obj)
        {
            Logger.Log("UICore.OnUIRemixed: UI remixed!");

            obj.Panels.ForEach(panel => { panel.FromThisScene = false; });
            obj.RemixUI(true);
        }

        private void OnSceneWillBeLoadedNow(EntryPoint sceneName, LoadSceneMode loadSceneMode)
        {
            Logger.Log("UICore.OnSceneWillBeLoadedNow: OnSceneWillBeLoadedNow!");

            Panels.ForEach(panel => { panel.FromThisScene = false; });

            RemixUI(true);
        }

        #endregion
    }
}