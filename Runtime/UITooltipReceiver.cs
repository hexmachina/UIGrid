using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;


namespace TW.UI
{
	public class UITooltipReceiver : UIBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		public event Action<UITooltipReceiver, PointerEventData> onEnter;
		public event Action<UITooltipReceiver, PointerEventData> onExit;
		public event Action<UITooltipReceiver, PointerEventData> onDown;
		public event Action<UITooltipReceiver, PointerEventData> onUp;

		protected override void OnEnable()
		{
			UITooltipManager.instance.RegisterReceiver(this);
		}

		protected override void OnDisable()
		{
			UITooltipManager.instance.UnregisterReceiver(this);

		}


		public void OnPointerEnter(PointerEventData eventData)
		{
			onEnter?.Invoke(this, eventData);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			onExit?.Invoke(this, eventData);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			onDown?.Invoke(this, eventData);

		}

		public void OnPointerUp(PointerEventData eventData)
		{
			onUp?.Invoke(this, eventData);

		}
	}

}
