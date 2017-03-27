using DG.Tweening;

using UnityEngine;

namespace SpaceEngine.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIOverlay : MonoSingleton<UIOverlay>
    {
        [SerializeField]
        protected float transitionTime = .5f;

        protected CanvasGroup canvasGroup;

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        protected static void ShowInternal(UIOverlay overlay)
        {
            overlay.canvasGroup.blocksRaycasts = true;

            DOTween.To(() => overlay.canvasGroup.alpha, value => overlay.canvasGroup.alpha = value, 1, overlay.transitionTime);
        }

        protected static void HideInternal(UIOverlay overlay)
        {
            DOTween.To(() => overlay.canvasGroup.alpha, value => overlay.canvasGroup.alpha = value, 0, overlay.transitionTime).OnComplete(() =>
            {
                overlay.canvasGroup.blocksRaycasts = false;
            });
        }
    }

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIOverlay<T> : UIOverlay where T : UIOverlay
    {
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