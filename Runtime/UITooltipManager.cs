using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TW.UI
{
	public class UITooltipManager
	{
		static private bool _isInstantiated;
		static public bool isInstantiated { get { return _isInstantiated; } }

		static UITooltipManager s_Instance;

		public static UITooltipManager instance
		{
			get
			{
				if (s_Instance == null)
				{
					_isInstantiated = true;
					s_Instance = new UITooltipManager();
				}

				return s_Instance;
			}
		}

		public event Action<UITooltipReceiver, PointerEventData> onEnter;
		public event Action<UITooltipReceiver, PointerEventData> onExit;
		public event Action<UITooltipReceiver, PointerEventData> onDown;
		public event Action<UITooltipReceiver, PointerEventData> onUp;

		private void OnPointerEnter(UITooltipReceiver receiver, PointerEventData eventData)
		{
			onEnter?.Invoke(receiver, eventData);
		}

		private void OnPointerExit(UITooltipReceiver receiver, PointerEventData eventData)
		{
			onExit?.Invoke(receiver, eventData);
		}

		private void OnPointerDown(UITooltipReceiver receiver, PointerEventData eventData)
		{
			onDown?.Invoke(receiver, eventData);
		}

		private void OnPointerUp(UITooltipReceiver receiver, PointerEventData eventData)
		{
			onUp?.Invoke(receiver, eventData);
		}

		public void RegisterReceiver(UITooltipReceiver obj)
		{
			obj.onEnter += OnPointerEnter;
			obj.onExit += OnPointerExit;
			obj.onDown += OnPointerDown;
			obj.onUp += OnPointerUp;
		}

		public void UnregisterReceiver(UITooltipReceiver obj)
		{
			obj.onEnter -= OnPointerEnter;
			obj.onExit -= OnPointerExit;
			obj.onDown -= OnPointerDown;
			obj.onUp -= OnPointerUp;
		}
	}

}
