using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TW.UI
{
	[ExecuteAlways]
	[RequireComponent(typeof(CanvasRenderer))]
	public class UIGridRenderer : MaskableGraphic
	{
		public Vector2Int gridSize = new Vector2Int(1, 1);
		public float thickness = 10f;

		float cellWidth;
		float cellHeight;

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			Vector4 v = GetDrawingDimensions();

			float width = rectTransform.rect.width;
			float height = rectTransform.rect.height;

			cellWidth = width / (float)gridSize.x;
			cellHeight = height / (float)gridSize.y;

			int count = 0;

			for (int y = 0; y < gridSize.y; y++)
			{
				for (int x = 0; x < gridSize.x; x++)
				{
					DrawCell(x, y, count, v, vh);
					count++;
				}
			}

		}

		private void DrawCell(int x, int y, int index, Vector4 v, VertexHelper vh)
		{
			float xPos = v.x + cellWidth * x;
			float yPos = v.y + cellHeight * y;

			UIVertex vertex = UIVertex.simpleVert;
			vertex.color = color;

			vertex.position = new Vector3(xPos, yPos);
			vh.AddVert(vertex);

			vertex.position = new Vector3(xPos, yPos + cellHeight);
			vh.AddVert(vertex);

			vertex.position = new Vector3(xPos + cellWidth, yPos + cellHeight);
			vh.AddVert(vertex);

			vertex.position = new Vector3(xPos + cellWidth, yPos);
			vh.AddVert(vertex);

			var widthSqr = thickness * thickness;
			var distanceSqr = widthSqr / 2f;
			var distance = Mathf.Sqrt(distanceSqr);

			vertex.position = new Vector3(xPos + distance, yPos + distance);
			vh.AddVert(vertex);

			vertex.position = new Vector3(xPos + distance, yPos + cellHeight - distance);
			vh.AddVert(vertex);

			vertex.position = new Vector3(xPos + cellWidth - distance, yPos + cellHeight - distance);
			vh.AddVert(vertex);

			vertex.position = new Vector3(xPos + cellWidth - distance, yPos + distance);
			vh.AddVert(vertex);

			int offset = index * 8;

			vh.AddTriangle(offset + 0, offset + 1, offset + 5);
			vh.AddTriangle(offset + 5, offset + 4, offset + 0);

			vh.AddTriangle(offset + 1, offset + 2, offset + 6);
			vh.AddTriangle(offset + 6, offset + 5, offset + 1);

			vh.AddTriangle(offset + 2, offset + 3, offset + 7);
			vh.AddTriangle(offset + 7, offset + 6, offset + 2);

			vh.AddTriangle(offset + 3, offset + 0, offset + 4);
			vh.AddTriangle(offset + 4, offset + 7, offset + 3);
		}


		private Vector4 GetDrawingDimensions()
		{
			Rect r = GetPixelAdjustedRect();

			var v = new Vector4(
				r.x,
				r.y,
				r.x + r.width,
				r.y + r.height
			);

			return v;
		}
	}

}
