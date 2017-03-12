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

using SpaceEngine.Pluginator;
using UnityEngine;
using UnityEngine.UI;

public sealed class MainMenuUI : UserInterface, IUserInterface
{
    public AssemblyLoader loader = null;

    public CanvasRenderer addonsCanvasRenderer;
    public Toggle addonsShowToggle;
    public Text addonsCounterText;
    public ScrollRect addonsScrollView;
    public GameObject addonsItemPrefab;

    private void Awake()
    {
        if (loader == null) loader = Loader.Instance as AssemblyLoader;

        InitUI();
    }

    public void InitUI()
    {
        ShowAddonsToggle_OnValueChanged(addonsShowToggle);

        if (loader != null)
        {
            if (loader.ExternalAssemblies.Count != 0)
            {
                for (int i = 0; i < loader.ExternalAssemblies.Count; i++)
                {
                    AssemblyExternal assembly = loader.ExternalAssemblies[i];

                    if (assembly != null)
                        AddToScrollView(addonsItemPrefab, addonsScrollView, assembly.Name, assembly.Version);
                    else
                    {
                        // TODO : Logging...
                    }
                }
            }
            else
            {

            }
        }

        if (addonsShowToggle != null)
            addonsShowToggle.interactable = (loader != null);

        if (addonsCounterText != null)
            addonsCounterText.text = addonsCounterText.text + " " + ((loader == null) ? "0" : loader.ExternalAssemblies.Count.ToString());
    }

    public void AddToScrollView(GameObject item, ScrollRect scrollView, string name = "", string version = "")
    {
        GameObject createdItem = Instantiate(item);
        createdItem.transform.SetParent(scrollView.content.transform);
        createdItem.transform.localScale = Vector3.one; //Must have.

        AddonItemUI aiUI = createdItem.GetComponent<AddonItemUI>();
        aiUI.SetCaption(aiUI.addonNameText, name);
        aiUI.SetCaption(aiUI.addonVersionText, version);
    }

    public void ShowAddonsToggle_OnValueChanged(Toggle t)
    {
        if (addonsScrollView != null && t != null)
        {
            addonsScrollView.gameObject.SetActive(!t.isOn);
        }
    }
}