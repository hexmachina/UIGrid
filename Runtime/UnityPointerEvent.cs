using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TW.UI
{
	[Serializable]
	public class UnityPointerEvent : UnityEvent<PointerEventData> { }
}