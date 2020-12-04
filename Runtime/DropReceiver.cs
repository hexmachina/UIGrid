using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TW.UI
{
	public class DropReceiver : UIBehaviour, IDropHandler
	{
		public UnityPointerEvent onDrop = new UnityPointerEvent();

		public void OnDrop(PointerEventData eventData)
		{
			if (!enabled)
				return;

			if (eventData.pointerDrag)
			{
				onDrop.Invoke(eventData);
				//Debug.Log(eventData.pointerDrag.name);
			}

		}
	}
}