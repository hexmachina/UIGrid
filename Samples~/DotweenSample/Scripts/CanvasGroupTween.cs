using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace TW.UI.Samples
{

	[RequireComponent(typeof(CanvasGroup))]
	public class CanvasGroupTween : MonoBehaviour
	{
		public float inDuration = 1f;
		public Ease inEase;
		public float outDuration = 1f;
		public Ease outEase;

		[System.NonSerialized] private CanvasGroup _canvasGroup;

		public CanvasGroup canvasGroup
		{
			get
			{
				// The RectTransform is a required component that must not be destroyed. Based on this assumption, a
				// null-reference check is sufficient.
				if (ReferenceEquals(_canvasGroup, null))
				{
					_canvasGroup = GetComponent<CanvasGroup>();
				}
				return _canvasGroup;
			}
		}

		public void FadeIn()
		{

			canvasGroup.DOFade(1f, Mathf.Max(0, inDuration)).SetEase(inEase);
		}

		public void FadeOut()
		{
			canvasGroup.DOFade(0, Mathf.Max(0, outDuration)).SetEase(outEase);

		}
	}
}
