#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2016 Denis Ovchinnikov [zameran] 
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

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using ZFramework.Extensions;
using ZFramework.Unity.Common;

using UnityEngine;
using UnityEngine.SceneManagement;

using Logger = ZFramework.Unity.Common.Logger;

using SpaceEngine.Pluginator.Attributes;
using SpaceEngine.Pluginator.Enums;

namespace SpaceEngine.Pluginator
{
    [UseLogger(Category.Data)]
    [UseLoggerFile(false, "Log")]
    public sealed class AssemblyLoader : Loader, IEventit
    {
        public GUISkin UISkin = null;

        private bool Loaded = false;
        private bool ShowGUI = true;

        public int TotalDetected = 0;
        public int TotalLoaded = 0;

        public List<AssemblyExternal> ExternalAssemblies = new List<AssemblyExternal>();

        protected override void Start()
        {
            base.Start();
        }

        protected override void Awake()
        {
            base.Awake();

            Pass();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            if (ShowGUI)
            {
                if (UISkin != null) GUI.skin = UISkin;

                GUI.Box(new Rect(Screen.width / 2 - (Screen.width / 1.25f) / 2, Screen.height / 1.25f, Screen.width / 1.25f, 15), "");

                GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 1.25f, Screen.width / 1.25f, 25),
                    string.Format("Loading {0}/{1} dll's...", TotalLoaded, TotalDetected));
            }
        }

        #region Subscribtion/Unsubscription

        private void OnEnable()
        {
            Eventit();
        }

        private void OnDisable()
        {
            UnEventit();
        }

        private void OnDestroy()
        {
            UnEventit();
        }

        #endregion

        #region Eventit

        public bool isEventit { get; set; }

        public void Eventit()
        {
            if (isEventit) return;

            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            isEventit = true;
        }

        public void UnEventit()
        {
            if (!isEventit) return;

            SceneManager.activeSceneChanged -= OnActiveSceneChanged;

            isEventit = false;
        }

        #endregion

        #region Events

        private void OnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            ShowGUI = false;

            if (SceneManager.GetActiveScene().buildIndex != 0 && Loaded) FirePlugins(ExternalAssemblies);
        }

        #endregion

        protected override void Pass()
        {
            Logger.Log(string.Format("AssemblyLoader Initiated at scene №: {0}", SceneManager.GetActiveScene().buildIndex));

            if (!Loaded)
            {
                DetectAndLoadAssemblies();
                Loaded = true;
            }

            if (SceneManager.GetActiveScene().buildIndex == 0 && Loaded)
            {
                Delay((TotalDetected + 1) * 2, () => { SceneManager.LoadScene(1); });
            }

            base.Pass();
        }

        #region AssemblyLoader Logic

        private void DetectAssembies(out List<string> allPaths)
        {
            string path = PathGlobals.GlobalModFolderPath;

            allPaths = new List<string>();

            try
            {
                allPaths.AddRange(Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories));
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("DetectAssembies Exception: {0}", ex.Message));
            }

            TotalDetected = allPaths.Count;

            Logger.Log(string.Format("Assembies Detected: {0}", allPaths.Count));
        }

        private void DetectAndLoadAssemblies()
        {
            List<string> allPaths;
            DetectAssembies(out allPaths);

            LoadDetectedAssemblies(allPaths);
        }

        private void LoadAssembly(string path)
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(path);

                SpaceAddonAssembly[] attrbutes = assembly.GetCustomAttributes(typeof (SpaceAddonAssembly), false) as SpaceAddonAssembly[];

                if (attrbutes == null || attrbutes.Length == 0)
                {
                    Logger.Log(string.Format("This is not an adddon assembly! {0}", path));
                }
                else
                {
                    SpaceAddonAssembly ea = attrbutes[0];
                    List<Type> mb = GetAllSubclassesOf<Type, SpaceAddonMonoBehaviour, MonoBehaviour>(assembly);
                    AssemblyExternalTypes aet = new AssemblyExternalTypes(typeof (MonoBehaviour), mb);
                    AssemblyExternal ae = new AssemblyExternal(path, ea.Name, ea.Version, assembly, aet);

                    ExternalAssemblies.Add(ae);

                    TotalLoaded++;

                    FireHotPlugin(ae);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("LoadAssembly Exception: {0}", ex.Message));
            }
        }

        private void LoadDetectedAssemblies(List<string> allPaths)
        {
            if (allPaths == null)
            {
                DetectAssembies(out allPaths);
                Logger.Log("Something wrong with path's array! Detecting assemblies again!");
            }

            for (int i = 0; i < allPaths.Count; i++)
            {
                string path = allPaths[i];

                Delay(0.5f, () => { LoadAssembly(path); });
            }
        }

        private void FirePlugins(List<AssemblyExternal> ExternalAssemblies, int level)
        {
            int counter = 0;

            foreach (AssemblyExternal assembly in ExternalAssemblies)
            {
                counter += assembly.Types.SelectMany(kvp => kvp.Value).Count(v => FirePlugin(v, level));
            }

            Logger.Log(string.Format("{0} plugins fired at scene №: {1}", counter, level));
        }

        private void FirePlugins(List<AssemblyExternal> ExternalAssemblies)
        {
            int counter = 0;

            foreach (AssemblyExternal assembly in ExternalAssemblies)
            {
                counter += assembly.Types.SelectMany(kvp => kvp.Value).Count(v => FirePlugin(v));
            }

            Logger.Log(string.Format("{0} plugins fired at scene №: {1}", counter, SceneManager.GetActiveScene().buildIndex));
        }

        private void FireHotPlugin(AssemblyExternal Addon)
        {
            int counter = Addon.Types.SelectMany(kvp => kvp.Value).Count(v => FirePlugin(v, 0));

            Logger.Log(string.Format("{0} plugins fired at scene №: {1}", counter, SceneManager.GetActiveScene().buildIndex));
        }

        private bool FirePlugin(Type type, int level)
        {
            SpaceAddonMonoBehaviour atr = AttributeUtils.GetTypeAttribute<SpaceAddonMonoBehaviour>(type);

            if (atr != null)
            {
                if ((int)atr.EntryPoint == level)
                {
                    GameObject go = new GameObject(type.Name);
                    go.transform.position = Vector3.zero;
                    go.transform.rotation = Quaternion.identity;
                    go.AddComponent(type);

                    return true;
                }
            }

            return false;
        }

        private bool FirePlugin(Type type)
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;

            SpaceAddonMonoBehaviour atr = AttributeUtils.GetTypeAttribute<SpaceAddonMonoBehaviour>(type);

            if (atr != null)
            {
                if ((int)atr.EntryPoint == currentScene)
                {
                    GameObject go = new GameObject(type.Name);
                    go.transform.position = Vector3.zero;
                    go.transform.rotation = Quaternion.identity;
                    go.AddComponent(type);

                    return true;
                }
            }

            return false;
        }

        public List<T> GetAllSubclassesOf<T, U, Y>(Assembly assembly) where T : Type where U : Attribute
        {
            Type[] types = assembly.GetTypes();

            List<T> output = new List<T>();

            foreach (Type type in types)
            {
                if (type.IsSubclassOf(typeof (Y)))
                {
                    U atr = AttributeUtils.GetTypeAttribute<U>(type);

                    if (atr != null)
                        output.Add(type as T);
                }
            }

            return output;
        }

        #endregion
    }
}