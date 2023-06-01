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

using DG.Tweening;
using UnityEngine;

namespace SpaceEngine.UI.PanelSystem
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIOverlay : MonoBehaviour
    {
        [SerializeField]
        private float transitionTime = .5f;

        protected CanvasGroup canvasGroup;

        protected float TransitionTime => transitionTime;

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        protected static void ShowInternal(UIOverlay overlay)
        {
            overlay.canvasGroup.blocksRaycasts = true;

            DOTween.To(() => overlay.canvasGroup.alpha, value => overlay.canvasGroup.alpha = value, 1, overlay.TransitionTime);
        }

        protected static void HideInternal(UIOverlay overlay)
        {
            DOTween.To(() => overlay.canvasGroup.alpha, value => overlay.canvasGroup.alpha = value, 0, overlay.TransitionTime).OnComplete(() => { overlay.canvasGroup.blocksRaycasts = false; });
        }
    }

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIOverlay<T> : UIOverlay where T : UIOverlay
    {
        protected static T Instance;

        protected override void Awake()
        {
            Instance = GetComponent<T>();

            base.Awake();
        }

        protected static void ShowInternal()
        {
            ShowInternal(Instance);
        }

        protected static void HideInternal()
        {
            HideInternal(Instance);
        }
    }
}