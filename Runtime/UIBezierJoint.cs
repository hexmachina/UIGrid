using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;

namespace TW.UI
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(UIBezierRenderer))]
	[ExecuteInEditMode]
	public class UIBezierJoint : UIBehaviour
	{
		[FormerlySerializedAs("start")]
		public RectTransform _start;

		public RectTransform start
		{
			get { return _start; }
			set
			{
				if (value != _start)
				{
					_start = value;
					startChanged = true;
				}
			}
		}

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
		[FormerlySerializedAs("end")]
		public RectTransform _end;

		public RectTransform end
		{
			get { return _end; }
			set
			{
				if (value != _end)
				{
					_end = value;
					endChanged = true;
				}
			}
		}

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


		private void Update()
		{
			if (!bezier)
				return;

			if (_start && (_start.hasChanged || startChanged))
			{
				_start.hasChanged = false;
				startChanged = false;
				UpdateStart();
			}

			if (_end && (_end.hasChanged || endChanged))
			{
				endChanged = false;
				_end.hasChanged = false;
				UpdateEnd();
			}

		}

		private void UpdateStart()
		{
			if (!_start)
				return;

			bezier.startPoint = UpdateEdge(_start, overrideStartOffset, startOffset, _startPivotX, _startPivotY);
		}

		private void UpdateEnd()
		{
			if (!_end)
				return;

			bezier.endPoint = UpdateEdge(_end, overrideEndOffset, endOffset, _endPivotX, _endPivotY);

		}

		private Vector2 UpdateEdge(RectTransform target, bool overrideOffset, Vector2 offset, float pivotX, float pivotY)
		{
			var canvasPoint = root.InverseTransformPoint(target.position);

			if (overrideOffset)
			{
				canvasPoint.x += offset.x + (target.rect.width * 0.5f) * pivotX;
				canvasPoint.y += offset.y + (target.rect.height * 0.5f) * pivotY;
			}
			return canvasPoint;
		}

		public void ClearStart()
		{
			start = null;
		}
		public void ClearEnd()
		{
			end = null;
		}

		public void SetStartByGameObject(GameObject go)
		{
			var rect = go.transform as RectTransform;
			if (rect)
			{
				start = rect;
			}
		}

		public void SetEndByGameObject(GameObject go)
		{
			var rect = go.transform as RectTransform;
			if (rect)
			{
				end = rect;
			}
		}

		protected override void OnValidate()
		{
			endChanged = true;
			startChanged = true;
		}
	}

}

