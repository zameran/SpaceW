#region License
// Procedural planet generator.
//  
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//     notice, this list of conditions and the following disclaimer.
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
// Creation Date: 2017.03.16
// Creation Time: 3:09 PM
// Creator: zameran
#endregion

using SpaceEngine.Enums;
using SpaceEngine.UI.Panels;

using System;
using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceEngine.Managers
{
    public class LevelManager : MonoSingleton<LevelManager>
    {
        private Coroutine InjectedWaiter;

        public enum SceneLoadType
        {
            Async,
            Default
        }

        private void Awake()
        {
            Instance = this;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        #region API

        public void InjectWaiter(Coroutine routine)
        {
            InjectedWaiter = routine;
        }

        private IEnumerator Delay(float delay, Action OnDone = null)
        {
            yield return Yielders.Get(delay);

            if (InjectedWaiter != null) yield return InjectedWaiter;

            if (OnDone != null) OnDone();
        }

        public AsyncOperation UnloadScene(EntryPoint sceneName)
        {
            return SceneManager.UnloadSceneAsync(sceneName.ToString());
        }

        #region SingleScene

        public void LoadScene(EntryPoint sceneName)
        {
            LoadSceneInternal(sceneName, SceneLoadType.Default, LoadSceneMode.Single);
        }

        public void LoadSceneDelayed(EntryPoint sceneName, float delay)
        {
            LoadingPanel.Show();

            StartCoroutine(Delay(delay, () => { LoadScene(sceneName); }));
        }

        public AsyncOperation LoadSceneAsync(EntryPoint sceneName)
        {
            return LoadSceneInternal(sceneName, SceneLoadType.Async, LoadSceneMode.Single);
        }

        public void LoadSceneAsyncDelayed(EntryPoint sceneName, float delay = 2.0f)
        {
            LoadingPanel.Show();

            EventManager.BaseEvents.OnSceneWillBeLoaded.Invoke(sceneName, LoadSceneMode.Additive);

            StartCoroutine(Delay(delay, () => { LoadSceneInternal(sceneName, SceneLoadType.Async, LoadSceneMode.Single, false); }));
        }

        #endregion

        #region AdditiveScene

        public void LoadSceneAdditive(EntryPoint sceneName)
        {
            LoadSceneInternal(sceneName, SceneLoadType.Default, LoadSceneMode.Additive);
        }

        public void LoadSceneAdditiveDelayed(EntryPoint sceneName, float delay)
        {
            EventManager.BaseEvents.OnSceneWillBeLoaded.SafeInvoke(sceneName, LoadSceneMode.Additive);

            StartCoroutine(Delay(delay, () => { LoadSceneInternal(sceneName, SceneLoadType.Async, LoadSceneMode.Additive, false); }));
        }

        public AsyncOperation LoadSceneAdditiveAsync(EntryPoint sceneName)
        {
            return LoadSceneInternal(sceneName, SceneLoadType.Async, LoadSceneMode.Additive);
        }

        #endregion

        /// <summary>
        /// Load special defined scene with specified Load Type and Mode.
        /// </summary>
        /// <param name="sceneName">Special scene.</param>
        /// <param name="sceneLoadType">Load Type.</param>
        /// <param name="loadSceneMode">Load Mode.</param>
        /// <returns>Returns AsyncOperation, if Load Type is Async.</returns>
        private AsyncOperation LoadSceneInternal(EntryPoint sceneName, SceneLoadType sceneLoadType, LoadSceneMode loadSceneMode, bool fireEvent = true)
        {
            //var currentSceneNameEnum = (EntryPoint)Enum.Parse(typeof(EntryPoint), SceneManager.GetActiveScene().name);
            var sceneNameParsed = sceneName.ToString();

            try
            {
                if (loadSceneMode == LoadSceneMode.Single && fireEvent)
                {
                    EventManager.BaseEvents.OnSceneWillBeLoaded.Invoke(sceneName, loadSceneMode);
                }

                EventManager.BaseEvents.OnSceneWillBeLoadedNow.Invoke(sceneName, loadSceneMode);

                switch (sceneLoadType)
                {
                    case SceneLoadType.Async:
                        return SceneManager.LoadSceneAsync(sceneNameParsed, loadSceneMode);
                    case SceneLoadType.Default:
                        SceneManager.LoadScene(sceneNameParsed, loadSceneMode);
                        return null;
                    default:
                        SceneManager.LoadScene(sceneNameParsed, loadSceneMode);
                        return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("LevelManager.LoadScene: Exception!\n" + ex.Message);
            }

            return null;
        }

        #endregion

        #region Events

        private void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
        {
            EventManager.BaseEvents.OnSceneLoaded.Invoke((EntryPoint)Enum.Parse(typeof(EntryPoint), loadedScene.name, true), mode);

            LoadingPanel.Hide();
        }

        #endregion
    }
}