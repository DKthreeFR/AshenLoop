using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractView : UIViewBase
{
   public CanvasGroup canvasGroup;
   [SerializeField] private float fadeDuration = 0.2f;
   private Coroutine _fadeCoroutine;

   public override void Bind(object controller)
   {
       canvasGroup.alpha = 0f;
       canvasGroup.interactable = false;
       canvasGroup.blocksRaycasts = false;
   }
   public override void Unbind()
   {
       if (_fadeCoroutine != null)
       {
           StopCoroutine(_fadeCoroutine);
           _fadeCoroutine = null;
       }
       canvasGroup.alpha = 0f;
       canvasGroup.interactable = false;
       canvasGroup.blocksRaycasts = false;
   }
   public override void Show()
   {
       if (_fadeCoroutine != null)
       {
           StopCoroutine(_fadeCoroutine);
       }
       _fadeCoroutine = StartCoroutine(FadeTo(1f));
   }
   public override void Hide()
   {
       if (_fadeCoroutine != null)
       {
           StopCoroutine(_fadeCoroutine);
       }
       _fadeCoroutine = StartCoroutine(FadeTo(0f));
   }

   private IEnumerator FadeTo(float targetAlpha)
   {
       float startAlpha = canvasGroup.alpha;
       float elapsed = 0f;

       while (elapsed < fadeDuration)
       {
           elapsed += Time.deltaTime;
           float t = Mathf.Clamp01(elapsed / fadeDuration);
           canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
           yield return null;
       }

       canvasGroup.alpha = targetAlpha;
       bool visible = targetAlpha > 0.99f;
       canvasGroup.interactable = visible;
       canvasGroup.blocksRaycasts = visible;
       _fadeCoroutine = null;
   }
}
