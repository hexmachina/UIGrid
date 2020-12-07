using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TW.UI
{
	public class ScrollRectDrag : ScrollRect, IPointerExitHandler
	{
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

		public enum ConstraintType
		{
			None,
			Screen,
			Parent
		}

		public ConstraintType constraint;

		public DragDirection dragDirection;

		public UnityPointerEvent onPointerExit = new UnityPointerEvent();

		public UnityEvent onDropFailed = new UnityEvent();

		public UnityEvent onDragEnd = new UnityEvent();

		private Vector2 lastPointPosition;
		private Graphic itemDrag;

		private int dragSiblingIndex;

		List<RaycastResult> raycastResults = new List<RaycastResult>();

		public override void OnBeginDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if (eventData.pointerEnter && eventData.pointerEnter != gameObject && CheckDragDirection(eventData))
			{
				if (eventData.pointerEnter.TryGetComponent(out itemDrag))
				{
					itemDrag.raycastTarget = false;
					lastPointPosition = eventData.position;
				}

			}
			else
			{
				base.OnBeginDrag(eventData);
			}

		}

		public override void OnDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if (itemDrag)
			{
				Vector2 currentMousePosition = eventData.position;
				Vector2 diff = currentMousePosition - lastPointPosition;

				var rectTransform = itemDrag.transform as RectTransform;

				Vector3 newPosition = rectTransform.position + new Vector3(diff.x, diff.y, transform.position.z);
				Vector3 oldPos = rectTransform.position;
				rectTransform.position = newPosition;

				lastPointPosition = currentMousePosition;

			}
			else
			{
				base.OnDrag(eventData);
			}
		}

		public override void OnEndDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if (itemDrag)
			{
				eventData.pointerDrag = itemDrag.gameObject;
				raycastResults.Clear();
				EventSystem.current.RaycastAll(eventData, raycastResults);

				for (int i = 0; i < raycastResults.Count; i++)
				{
					if (raycastResults[i].gameObject != gameObject && raycastResults[i].gameObject != itemDrag.gameObject)
					{
						var target = raycastResults[i].gameObject;
						//Debug.Log(target.name);
						if (!ExecuteEvents.Execute(target, eventData, ExecuteEvents.dropHandler))
						{
							itemDrag.transform.SetParent(content);
							itemDrag.transform.SetSiblingIndex(dragSiblingIndex);
							onDropFailed.Invoke();
						}
						itemDrag.raycastTarget = true;
						break;
					}
				}
				itemDrag = null;
				onDragEnd.Invoke();
			}
			else
			{
				base.OnEndDrag(eventData);
			}
		}

		private bool CheckDragDirection(PointerEventData eventData)
		{
			//Debug.Log(eventData.delta);
			switch (dragDirection)
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
					return false;
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

		public void OnPointerExit(PointerEventData eventData)
		{
			if (itemDrag)
			{
				dragSiblingIndex = itemDrag.transform.GetSiblingIndex();
				itemDrag.transform.SetParent(transform.parent);
				itemDrag.raycastTarget = true;
				onPointerExit.Invoke(eventData);
			}
		}
	}
}