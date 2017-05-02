using System.Collections;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SpaceEngine.UI
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

        private bool _hideAfter;

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
                _hideAfter = true;
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

            if (_hideAfter)
            {
                ChangeShowness(time);
            }
        }
    }
}