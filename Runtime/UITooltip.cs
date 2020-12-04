using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TW.UI
{

	public class UITooltip : UIBehaviour
	{
		[System.Serializable]
		public class UnityToolTipReveiverEvent : UnityEvent<UITooltipReceiver> { }

#if ENABLE_INPUT_SYSTEM
		[SerializeField] InputActionReference pointer;
#endif

		public UnityEvent onEngaged = new UnityEvent();
		public UnityEvent onDisengaged = new UnityEvent();
		public UnityToolTipReveiverEvent onReceiverChanged = new UnityToolTipReveiverEvent();

		private Vector2 screenPosition;



		private bool _engaged = false;

		public bool engaged => _engaged;

		private bool downState = false;

		private UITooltipReceiver _receiver;

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

		[System.NonSerialized] private RectTransform m_RectRoot;
		protected RectTransform rootRect
		{
			get
			{
				if (m_RectRoot == null)
					m_RectRoot = rectTransform.root as RectTransform;
				return m_RectRoot;
			}
		}

		protected override void OnEnable()
		{
			//pointer?.action?.Enable();
#if ENABLE_INPUT_SYSTEM
			if (pointer)
			{
				pointer.action.performed += OnPoint;
				pointer.action.canceled += OnPoint;
			}
#endif
			UITooltipManager.instance.onEnter += OnEnter;
			UITooltipManager.instance.onExit += OnExit;
			UITooltipManager.instance.onDown += OnDown;
			UITooltipManager.instance.onUp += OnUp;
		}

		private void OnUp(UITooltipReceiver arg1, PointerEventData arg2)
		{
			if (arg2.pointerCurrentRaycast.screenPosition != screenPosition)
				return;

			downState = false;
			if (enabled)
			{
				SetPivot();
				SetAnchoredPosition();
				onEngaged.Invoke();
			}
		}

		private void OnDown(UITooltipReceiver arg1, PointerEventData arg2)
		{
			if (arg2.pointerCurrentRaycast.screenPosition != screenPosition)
				return;

			downState = true;
			if (enabled)
			{
				onDisengaged.Invoke();
			}
		}

		private void OnExit(UITooltipReceiver arg1, PointerEventData arg2)
		{
			if (arg2.pointerCurrentRaycast.screenPosition != screenPosition)
				return;

			_engaged = false;
			onDisengaged.Invoke();
		}

		private void OnEnter(UITooltipReceiver arg1, PointerEventData arg2)
		{

			if (arg2.dragging || arg2.pointerCurrentRaycast.screenPosition != screenPosition)
				return;

			_engaged = true;
			SetPivot();
			SetAnchoredPosition();
			if (!downState)
			{
				onEngaged.Invoke();
			}
			if (arg1 != _receiver)
			{
				_receiver = arg1;
				onReceiverChanged.Invoke(_receiver);
			}
		}


#if ENABLE_INPUT_SYSTEM
		private void OnPoint(InputAction.CallbackContext obj)
		{
			var pos = obj.ReadValue<Vector2>();
			if (pos == screenPosition)
				return;

			screenPosition = pos;
			if (engaged)
			{

				SetAnchoredPosition();
			}

		}
#endif

		private void SetPivot()
		{
			Vector2 pivot = Vector2.zero;
			if (screenPosition.y > rootRect.rect.height * 0.5f)
			{
				pivot.y = 1f;
			}
			if (screenPosition.x > rootRect.rect.width * 0.5f)
			{
				pivot.x = 1f;
			}
			rectTransform.pivot = pivot;
		}

		private void SetAnchoredPosition()
		{
			var anchored = screenPosition / rectTransform.root.localScale.x;
			var pivotWidth = rectTransform.rect.width - (rectTransform.rect.width * rectTransform.pivot.x);
			var pivotHeight = rectTransform.rect.height - (rectTransform.rect.height * rectTransform.pivot.y);
			if (anchored.x + pivotWidth > rootRect.rect.width)
			{
				anchored.x = rootRect.rect.width - pivotWidth;
			}
			if (anchored.x < 0)
			{
				anchored.x = 0;
			}
			if (anchored.y + pivotHeight > rootRect.rect.height)
			{
				anchored.y = rootRect.rect.height - pivotHeight;
			}
			if (anchored.y < 0)
			{
				anchored.y = 0;
			}
			rectTransform.anchoredPosition = anchored;

		}

		protected override void OnDisable()
		{
#if ENABLE_INPUT_SYSTEM
			if (pointer)
			{
				pointer.action.performed -= OnPoint;
				pointer.action.canceled -= OnPoint;
			}
#endif
			UITooltipManager.instance.onEnter -= OnEnter;
			UITooltipManager.instance.onExit -= OnExit;
		}
	}

}
