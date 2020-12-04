using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TW.UI
{
	[CustomEditor(typeof(ScrollRectDrag), true)]
	public class ScrollRectDragEditor : Editor
	{

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

		}
	}

}
