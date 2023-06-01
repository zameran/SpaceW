#region License

// Procedural planet generator.
// 
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran

#endregion

using System.Collections.Generic;
using System.Linq;
using SpaceEngine.Core.Debugging;
using SpaceEngine.Core.Patterns.Strategy;
using SpaceEngine.Enums;
using SpaceEngine.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = SpaceEngine.Core.Debugging.Logger;

namespace SpaceEngine.UI.PanelSystem
{
    [UseLogger(LoggerCategory.InGameUI)]
    public class UICore : MonoBehaviour, IEventit
    {
        private static UICore Instance;

        public bool ForceDestroyAllPanels;
        public GameObject Root => gameObject;

        public List<UIPanel> Panels => GetComponentsInChildren<UIPanel>().Where(x => x.transform.parent == transform).ToList();

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

                DestroyImmediate(gameObject);
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
            if (IsEventit)
            {
                return;
            }

            EventManager.UIEvents.AllAdditiveUILoaded.OnEvent += OnAllAdditiveUILoaded;
            EventManager.UIEvents.UIRemixed.OnEvent += OnUIRemixed;

            EventManager.BaseEvents.OnSceneLoaded.OnEvent += OnLevelLoaded;
            EventManager.BaseEvents.OnSceneWillBeLoadedNow.OnEvent += OnSceneWillBeLoadedNow;

            IsEventit = true;
        }

        public void UnEventit()
        {
            if (!IsEventit)
            {
                return;
            }

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
            {
                RemixUI(ForceDestroyAllPanels);
            }
        }

        private void OnAllAdditiveUILoaded()
        {
            var loadingScreenPanel = Panels.Find(panel => panel.gameObject.name == "Loading Panel");

            if (loadingScreenPanel != null)
            {
                loadingScreenPanel.Hide();
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