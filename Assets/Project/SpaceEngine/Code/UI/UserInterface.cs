﻿#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2018 Denis Ovchinnikov [zameran] 
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

using SpaceEngine.Core.Patterns.Strategy.Eventit;
using SpaceEngine.Enums;
using SpaceEngine.Managers;

using System;

using UnityEngine;
using UnityEngine.SceneManagement;

public class UserInterface : MonoBehaviour, IEventit
{
    public GameObject Controllable;

    #region API

    public void FreezeTime()
    {
        Time.timeScale = 0.001f;
    }

    public void UnFreezeTime()
    {
        Time.timeScale = 1.0f;
    }

    public void LoadScene(string sceneName)
    {
        LevelManager.Instance.LoadScene((EntryPoint)Enum.Parse(typeof(EntryPoint), sceneName));
    }

    public void Quit()
    {
#if UNITY_EDITOR
        Debug.Break();
#else
        Application.Quit();
#endif
    }

    #endregion

    #region Subscribtion/Unsubscription

    protected void OnEnable()
    {
        Eventit();
    }

    protected void OnDisable()
    {
        UnEventit();
    }

    protected void OnDestroy()
    {
        UnEventit();
    }

    #endregion

    #region Eventit

    public bool IsEventit { get; set; }

    public void Eventit()
    {
        if (IsEventit) return;

        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        IsEventit = true;
    }

    public void UnEventit()
    {
        if (!IsEventit) return;

        SceneManager.activeSceneChanged -= OnActiveSceneChanged;

        IsEventit = false;
    }

    #endregion

    #region Events

    private void OnActiveSceneChanged(Scene arg0, Scene arg1)
    {
        UnFreezeTime();
    }

    #endregion
}