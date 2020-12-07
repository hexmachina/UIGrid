using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TW.UI
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	public class Draggable : UIBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
	{

		public enum ConstraintType
		{
			None,
			Screen,
			Parent
		}

		public enum SortingType
		{
			None,
			Front,
			Back
		}

		public enum DragDirection
		{
			None,
			Horizontal,
			Vertical,
			Up,
			Down,
			Left,
			Right
		}


		[SerializeField] private ConstraintType _constraint;

		public ConstraintType constraint { get { return _constraint; } set { _constraint = value; } }

		[SerializeField] private SortingType _dragSorting;

		public SortingType dragSorting { get { return _dragSorting; } set { _dragSorting = value; } }

		[SerializeField] private DragDirection _dragDirection;

		public DragDirection dragDirection { get { return _dragDirection; } set { _dragDirection = value; } }


		private Vector2 lastPointPosition;

		private int order;

		List<RaycastResult> raycastResults = new List<RaycastResult>();

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

		private bool m_Dragging = true;

		public virtual void OnBeginDrag(PointerEventData eventData)
		{
			if (!enabled)
				return;

			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if (!CheckDragDirection(eventData))
			{
				eventData.pointerDrag = null;
				raycastResults.Clear();
				EventSystem.current.RaycastAll(eventData, raycastResults);
				if (raycastResults.Count > 1)
				{
					for (int i = 0; i < raycastResults.Count; i++)
					{
						if (raycastResults[i].gameObject != gameObject)
						{
							var target = raycastResults[i].gameObject;
							Debug.Log(target.name);
							ExecuteEvents.Execute(target, eventData, ExecuteEvents.beginDragHandler);
							break;
						}
					}
				}

				return;
			}


			lastPointPosition = eventData.position;

			switch (_dragSorting)
			{
				case SortingType.None:
					break;
				case SortingType.Front:
					order = rectTransform.GetSiblingIndex();
					rectTransform.SetAsLastSibling();
					break;
				case SortingType.Back:
					order = rectTransform.GetSiblingIndex();
					rectTransform.SetAsFirstSibling();
					break;
				default:
					break;
			}
			m_Dragging = true;
		}

		public virtual void OnDrag(PointerEventData eventData)
		{
			if (!enabled)
				return;

			if (!m_Dragging)
				return;

			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			Vector2 currentMousePosition = eventData.position;
			Vector2 diff = currentMousePosition - lastPointPosition;

			Vector3 newPosition = rectTransform.position + new Vector3(diff.x, diff.y, transform.position.z);
			Vector3 oldPos = rectTransform.position;
			rectTransform.position = newPosition;

			switch (_constraint)
			{
				case ConstraintType.None:
					break;
				case ConstraintType.Screen:
					if (!IsRectTransformInsideSreen(rectTransform))
					{
						rectTransform.position = oldPos;
					}
					//if (rectTransform.root)
					//{
					//	var parentRect = rectTransform.root as RectTransform;
					//	rectTransform.anchoredPosition = GetOffset(rectTransform, parentRect);

					//}
					break;
				case ConstraintType.Parent:
					if (rectTransform.parent)
					{
						var parentRect = rectTransform.parent as RectTransform;
						rectTransform.anchoredPosition = GetOffset(rectTransform, parentRect);
					}
					break;
				default:
					break;
			}

			lastPointPosition = currentMousePosition;
		}

		public virtual void OnEndDrag(PointerEventData eventData)
		{
			if (!enabled)
				return;

			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			m_Dragging = false;
			if (_dragSorting != SortingType.None)
			{
				rectTransform.SetSiblingIndex(order);
			}
		}

		private Vector3 GetOffset(RectTransform target, RectTransform frame)
		{
			Vector3 offset = target.anchoredPosition;

			var pivotMaxWidth = target.rect.width - (target.rect.width * target.pivot.x);
			var pivotMinWidth = (target.rect.width * target.pivot.x) - target.rect.width;
			var pivotMaxHeight = target.rect.height - (target.rect.height * target.pivot.y);
			var pivotMinHeight = (target.rect.height * target.pivot.y) - target.rect.height;

			var parentAnchorMaxWidth = frame.rect.width - (frame.rect.width * target.anchorMin.x);
			var parentAnchorMinWidth = -frame.rect.width * target.anchorMin.x;
			var parentAnchorMaxHeight = frame.rect.height - (frame.rect.height * target.anchorMin.y);
			var parentAnchorMinHeight = -frame.rect.height * target.anchorMin.y;

			if (offset.x + pivotMaxWidth > parentAnchorMaxWidth)
			{
				offset.x = parentAnchorMaxWidth - pivotMaxWidth;
			}
			if (offset.x + pivotMinWidth < parentAnchorMinWidth)
			{
				offset.x = parentAnchorMinWidth - pivotMinWidth;
			}
			if (offset.y + pivotMaxHeight > parentAnchorMaxHeight)
			{
				offset.y = parentAnchorMaxHeight - pivotMaxHeight;
			}
			if (offset.y + pivotMinHeight < parentAnchorMinHeight)
			{
				offset.y = parentAnchorMinHeight - pivotMinHeight;
			}
			return offset;
		}

		private bool IsRectTransformInsideSreen(RectTransform rectTransform)
		{
			bool isInside = false;
			Vector3[] corners = new Vector3[4];
			rectTransform.GetWorldCorners(corners);
			int visibleCorners = 0;
			Rect rect = new Rect(0, 0, Screen.width, Screen.height);
			foreach (Vector3 corner in corners)
			{
				if (rect.Contains(corner))
				{
					visibleCorners++;
				}
			}
			if (visibleCorners == 4)
			{
				isInside = true;
			}
			return isInside;
		}

		private bool CheckDragDirection(PointerEventData eventData)
		{

			switch (_dragDirection)
			{
				case DragDirection.Horizontal:
					return Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y);
				case DragDirection.Vertical:
					return Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x);
				case DragDirection.Up:
					return eventData.delta.y > 0f && eventData.delta.y > Mathf.Abs(eventData.delta.x);
				case DragDirection.Down:
					return eventData.delta.y < 0f && Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x);
				case DragDirection.Left:
					return eventData.delta.x < 0f && Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y);
				case DragDirection.Right:
					return eventData.delta.x > 0f && Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y);
				case DragDirection.None:
				default:
					return true;
			}
		}
	}

}

