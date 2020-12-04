using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TW.UI
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(UIBezierRenderer))]
	[ExecuteInEditMode]
	public class UIBezierJoint : UIBehaviour
	{
		public RectTransform start;

		public bool overrideStartOffset;
		public Vector2 startOffset;

		[SerializeField, Range(-1f, 1f)]
		private float _startPivotX = 0;

		public float startPivotX
		{
			get { return _startPivotX; }
			set
			{
				if (value != _startPivotX)
				{
					_startPivotX = value;
					startChanged = true;
				}
			}
		}

		[SerializeField, Range(-1f, 1f)]
		private float _startPivotY = 0;

		public float startPivotY
		{
			get { return _startPivotY; }
			set
			{
				if (value != _startPivotY)
				{
					_startPivotY = value;
					startChanged = true;
				}
			}
		}

		public RectTransform end;
		public bool overrideEndOffset;
		public Vector2 endOffset;

		[SerializeField, Range(-1f, 1f)]
		private float _endPivotX = 0;

		public float endPivotX
		{
			get { return _endPivotX; }
			set
			{
				if (value != _endPivotX)
				{
					_endPivotX = value;
					endChanged = true;
				}
			}
		}

		[SerializeField, Range(-1f, 1f)]
		private float _endPivotY = 0;

		public float endPivotY
		{
			get { return _endPivotY; }
			set
			{
				if (value != _endPivotY)
				{
					_endPivotY = value;
					endChanged = true;
				}
			}
		}

		private bool endChanged;
		private bool startChanged;

		private UIBezierRenderer bezier;

		private RectTransform root;

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

		protected override void Awake()
		{
			TryGetComponent(out bezier);
			root = rectTransform.root as RectTransform;
		}

		protected override void OnEnable()
		{

		}


		private void Update()
		{
			if (!bezier)
				return;

			//Vector2 thisPivot = rectTransform.pivot;
			if (start && (start.hasChanged || startChanged))
			{
				start.hasChanged = false;
				startChanged = false;
				UpdateStart();
			}

			if (end && (end.hasChanged || endChanged))
			{
				endChanged = false;
				end.hasChanged = false;
				UpdateEnd();
			}

		}

		private void UpdateStart()
		{
			if (!start)
				return;
			//var worldStart = start.TransformPoint(thisPivot);
			var canvasStart = root.InverseTransformPoint(start.position);

			if (overrideStartOffset)
			{
				canvasStart.x += startOffset.x + (start.rect.width * 0.5f) * _startPivotX;
				canvasStart.y += startOffset.y + (start.rect.height * 0.5f) * _startPivotY;
			}
			bezier.startPoint = canvasStart;
		}

		private void UpdateEnd()
		{
			if (!end)
				return;

			//var worldEnd = end.TransformPoint(thisPivot);
			var canvasEnd = root.InverseTransformPoint(end.position);

			if (overrideEndOffset)
			{
				canvasEnd.x += endOffset.x + (end.rect.width * 0.5f) * _endPivotX;
				canvasEnd.y += endOffset.y + (end.rect.height * 0.5f) * _endPivotY;
			}
			bezier.endPoint = canvasEnd;

		}

		protected override void OnValidate()
		{
			endChanged = true;
			startChanged = true;
		}
	}

}

