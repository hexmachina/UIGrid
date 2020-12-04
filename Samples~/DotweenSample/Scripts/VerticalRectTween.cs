using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace TW.UI.Samples
{

	[RequireComponent(typeof(RectTransform))]
	public class VerticalRectTween : MonoBehaviour
	{
		[System.NonSerialized] private RectTransform m_Rect;
		protected RectTransform rectTransform
		{
			get
			{
				if (m_Rect == null)
					m_Rect = GetComponent<RectTransform>();
				return m_Rect;
			}
		}
		public float revealHeight = 200f;
		public float revealDuration = 0.5f;

		public UnityEvent onConcealed = new UnityEvent();

		private Sequence sequence;

		public void Reveal()
		{
			if (sequence == null)
			{
				sequence = DOTween.Sequence();
			}
			//var delta = new Vector2(rectTransform.sizeDelta.x, revealHeight);
			sequence.Join(rectTransform.DOPivotY(0, revealDuration));
			//sequence.Join(rectTransform.DOAnchorPosY(0, revealDuration));
		}

		public void Conceal()
		{
			if (sequence == null)
			{
				sequence = DOTween.Sequence();
			}

			sequence.Join(rectTransform.DOPivotY(1, revealDuration).OnComplete(ConcealedComplete)); //.OnComplete(ConcealedComplete);
																									//sequence.Join(rectTransform.DOAnchorPosY(0, revealDuration));
		}

		private void ConcealedComplete()
		{
			onConcealed.Invoke();
		}
	}
}

