using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TW.UI.Samples
{
	public class ToggleEvent : MonoBehaviour
	{
		[SerializeField] bool _value;

		public bool value => _value;

		[SerializeField] bool invokeOnAwake;

		public UnityEvent onPositive = new UnityEvent();
		public UnityEvent onNegative = new UnityEvent();

		private void Awake()
		{
			if (invokeOnAwake)
			{
				InvokeValue(_value);
			}
		}

		public void Toggle()
		{
			_value = !_value;
			InvokeValue(_value);
		}

		public void ApplyValue(bool value)
		{
			if (value == _value)
				return;
			_value = value;
			InvokeValue(value);
		}

		private void InvokeValue(bool value)
		{
			if (value)
			{
				onPositive.Invoke();
			}
			else
			{
				onNegative.Invoke();
			}
		}
	}
}
