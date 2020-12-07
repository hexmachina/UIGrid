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

		private bool _entered = false;

		protected override void OnEnable()
		{
			UITooltipManager.instance.RegisterReceiver(this);
		}

		protected override void OnDisable()
		{
			if (_entered)
			{
				_entered = false;

				//var eventData = new PointerEventData(EventSystem.current);
				onExit?.Invoke(this, null);
			}
			UITooltipManager.instance.UnregisterReceiver(this);

		}


		public void OnPointerEnter(PointerEventData eventData)
		{
			_entered = true;
			onEnter?.Invoke(this, eventData);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_entered = false;
			onExit?.Invoke(this, eventData);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			onDown?.Invoke(this, eventData);

		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (!enabled)
				return;

			onUp?.Invoke(this, eventData);

		}

	}

}
