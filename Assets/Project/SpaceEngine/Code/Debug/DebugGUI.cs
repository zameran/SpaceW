#region License
/* Procedural planet generator.
 *
 * Copyright (C) 2015-2016 Denis Ovchinnikov
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using UnityEngine;

public abstract class DebugGUI : MonoBehaviour, IDebug
{
    public Rect debugInfoBounds = new Rect(10, 10, 500, 500);

    [HideInInspector]
    public DebugGUISwitcher switcher;

    [HideInInspector]
    public GUIStyle boldLabel;

    [HideInInspector]
    public Vector2 scrollPosition = Vector2.zero;

    public GUISkin skin
    {
        get
        {
            var switcher = GetComponent<DebugGUISwitcher>();

            if (switcher != null)
                if (switcher.skin != null)
                    return switcher.skin;

            return GUI.skin;
        }
        private set { }
    }

    protected virtual void Awake()
    {
        if (switcher == null)
            switcher = GetComponent<DebugGUISwitcher>();
    }

    protected virtual void Start()
    {

    }

    protected virtual void OnGUI()
    {
        GUI.skin = skin;
        GUI.depth = -100;

        if (switcher != null)
            if (switcher.skin != null)
                if (GUI.skin.FindStyle("label_Bold") != null)
                    boldLabel = GUI.skin.FindStyle("label_Bold");
                else if (GUI.skin.FindStyle("label") != null)
                    boldLabel = GUI.skin.FindStyle("label");
    }
}