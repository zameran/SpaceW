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

using System.Collections;
using SpaceEngine.Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SpaceEngine.UI.PanelSystem
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanel : UIBehaviour
    {
        public bool IsShown { get; set; }
        public bool IsShownByDefault;

        public bool Immune = false;
        public bool FromThisScene = true;
        public bool Overlay = false;

        public UnityEvent OnShow;
        public UnityEvent OnHide;
        public UnityEvent OnAfterShow;
        public UnityEvent OnAfterHide;
        public UnityEvent OnBeforeShow;
        public UnityEvent OnBeforeHide;

        private CanvasGroup canvasGroup;
        protected Coroutine routine;

        private bool hideAfter;

        protected override void Awake()
        {
            base.Start();

            canvasGroup = GetComponent<CanvasGroup>();
            IsShown = !IsShownByDefault;
            routine = StartCoroutine(ChangeShowness(0));
        }

        public void Show(float duration = 0.0f)
        {
            if (IsShown) return;

            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(ChangeShowness(duration));
        }

        public void Hide(float duration = 0.0f)
        {
            if (!IsShown)
            {
                hideAfter = true;
                return;
            }

            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(ChangeShowness(duration));
        }

        private IEnumerator ChangeShowness(float time)
        {
            if (IsShown)
            {
                OnBeforeHide.Invoke();

                if (time > 0)
                {
                    while (canvasGroup.alpha > 0)
                    {
                        canvasGroup.alpha -= Time.deltaTime / time;

                        yield return Yielders.EndOfFrame;
                    }

                    OnAfterHide.Invoke();
                }

                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                gameObject.SendMessage("OnHide", SendMessageOptions.DontRequireReceiver);

                OnHide.Invoke();
            }
            else
            {
                gameObject.SendMessage("OnShow", SendMessageOptions.DontRequireReceiver);

                OnBeforeShow.Invoke();

                if (Overlay)
                {
                    // Tweak to set overlay panel drawable over other panels
                    var p = transform.parent;
                    var ap = GetComponent<RectTransform>().anchoredPosition;
                    var sd = GetComponent<RectTransform>().sizeDelta;

                    transform.SetParent(null);
                    transform.SetParent(p);

                    GetComponent<RectTransform>().anchoredPosition = ap;
                    GetComponent<RectTransform>().sizeDelta = sd;
                }

                OnShow.Invoke();

                if (time > 0)
                {
                    while (canvasGroup.alpha < 1)
                    {
                        canvasGroup.alpha += Time.deltaTime / time;

                        yield return Yielders.EndOfFrame;
                    }
                }

                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;

                OnAfterShow.Invoke();
            }

            IsShown = !IsShown;

            yield return null;

            if (hideAfter)
            {
                ChangeShowness(time);
            }
        }
    }
}