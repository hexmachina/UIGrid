using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TW.UI
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	public class DragListener : UIBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
	{
		[System.Serializable]
		public class Vector2UnityEvent : UnityEvent<Vector2> { }

		[System.Serializable]
		public class GameObjectUnityEvent : UnityEvent<GameObject> { }

		[SerializeField] private bool _invokeDrop = true;

		public UnityEvent onBeginDrag = new UnityEvent();
		public Vector2UnityEvent onDrag = new Vector2UnityEvent();
		public UnityEvent onEndDrag = new UnityEvent();
		public GameObjectUnityEvent onDropped = new GameObjectUnityEvent();
		public UnityEvent onDropFailed = new UnityEvent();

		List<RaycastResult> raycastResults = new List<RaycastResult>();


		private bool m_Dragging = true;

		[NonSerialized] private RectTransform m_RectTransform;

		public RectTransform rectTransform
		{
			get
			{
				// The RectTransform is a required component that must not be destroyed. Based on this assumption, a
				// null-reference check is sufficient.
				if (ReferenceEquals(m_RectTransform, null))
				{
					m_RectTransform = GetComponent<RectTransform>();
				}
				return m_RectTransform;
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (!IsActive())
				return;

			if (eventData.button != PointerEventData.InputButton.Left)
				return;
			m_Dragging = true;

			onBeginDrag.Invoke();
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (!IsActive())
				return;

			if (!m_Dragging)
				return;

			if (eventData.button != PointerEventData.InputButton.Left)
				return;


			Vector2 canvasPoint = rectTransform.root.InverseTransformPoint(eventData.position);
			onDrag.Invoke(canvasPoint);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (!IsActive())
				return;

			if (eventData.button != PointerEventData.InputButton.Left)
				return;
			onEndDrag.Invoke();
			m_Dragging = false;
			if (_invokeDrop)
			{
				raycastResults.Clear();
				EventSystem.current.RaycastAll(eventData, raycastResults);
				if (raycastResults.Count > 1)
				{
					for (int i = 0; i < raycastResults.Count; i++)
					{
						if (raycastResults[i].gameObject != gameObject)
						{
							var target = raycastResults[i].gameObject;
							if (ExecuteEvents.Execute(target, eventData, ExecuteEvents.dropHandler))
							{
								onDropped.Invoke(target);
							}
							else
							{
								onDropFailed.Invoke();
							}
							break;
						}
					}
				}
			}
		}

		protected override void OnDisable()
		{
			if (m_Dragging)
			{
				m_Dragging = false;
				onEndDrag.Invoke();
			}

		}
	}
}