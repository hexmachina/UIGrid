using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TW.UI
{
	public class RightClickHandler : MonoBehaviour, IPointerClickHandler
	{
		public UnityPointerEvent onRightClick = new UnityPointerEvent();

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Right)
				return;

			if (!enabled)
				return;

			onRightClick.Invoke(eventData);
		}
	}

}
