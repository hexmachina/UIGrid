using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TW.UI
{
	[CustomEditor(typeof(UIBezierRenderer))]
	public class UIBezierRendererEditor : Editor
	{
		private void OnSceneGUI()
		{
			UIBezierRenderer curveRenderer = target as UIBezierRenderer;

			var oldMatrix = Handles.matrix;
			var transform = curveRenderer.rectTransform;
			//Pivot must be 0,0 to edit
			//transform.pivot = Vector2.zero;
			Handles.matrix = transform.localToWorldMatrix;

			Handles.DrawLine(curveRenderer.startPoint, curveRenderer.startPoint + curveRenderer.GetHeading(curveRenderer.handleDistance, curveRenderer.startDirection));
			Handles.DrawLine(curveRenderer.endPoint, curveRenderer.endPoint + curveRenderer.GetHeading(curveRenderer.handleDistance, curveRenderer.endDirection));

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				var p = Handles.PositionHandle(curveRenderer.startPoint, Quaternion.identity);

				if (check.changed)
				{
					Undo.RecordObject(curveRenderer, "Changed Curve Position");

					curveRenderer.startPoint = p;
					EditorUtility.SetDirty(curveRenderer);

				}
			}

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				var p = Handles.PositionHandle(curveRenderer.endPoint, Quaternion.identity);

				if (check.changed)
				{
					Undo.RecordObject(curveRenderer, "Changed Curve Position");

					curveRenderer.endPoint = p;

					EditorUtility.SetDirty(curveRenderer);
				}
			}


			Handles.matrix = oldMatrix;
		}
	}

}
