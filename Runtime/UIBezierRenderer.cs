using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TW.UI
{
	[ExecuteAlways]
	[RequireComponent(typeof(CanvasRenderer))]
	public class UIBezierRenderer : MaskableGraphic
	{

		public enum Direction
		{
			Top,
			Down,
			Left,
			Right
		}
		private enum SegmentType
		{
			Start,
			Middle,
			End,
			Full,
		}

		[SerializeField] private Vector2 _startPoint = Vector2.zero;
		[SerializeField] private Direction _startDirection = Direction.Right;
		[SerializeField] private Vector2 _endPoint = Vector2.one;
		[SerializeField] private Direction _endDirection = Direction.Left;

		public Vector2 startPoint
		{
			get { return _startPoint; }
			set
			{
				if (value != _startPoint)
				{
					_startPoint = value;
					SetVerticesDirty();
				}
			}
		}

		public Direction startDirection
		{
			get { return _startDirection; }
			set
			{
				if (value != _startDirection)
				{
					_startDirection = value;
					SetVerticesDirty();
				}
			}
		}

		public Vector2 endPoint
		{
			get { return _endPoint; }
			set
			{
				if (value != _endPoint)
				{
					_endPoint = value;
					SetVerticesDirty();
				}
			}
		}

		public Direction endDirection
		{
			get { return _endDirection; }
			set
			{
				if (value != _startDirection)
				{
					_endDirection = value;
					SetVerticesDirty();
				}
			}
		}

		[SerializeField] private float _thicknessScale = 1;

		public float thicknessScale
		{
			get { return _thicknessScale; }
			set
			{
				if (value != _thicknessScale)
				{
					_thicknessScale = value;
					SetVerticesDirty();
				}
			}
		}

		public float thickness = 10f;

		public float handleDistance = 10;
		public int bezierSegments = 5;

		private const float MIN_BEVEL_NICE_JOIN = 30 * Mathf.Deg2Rad;

		private static Vector2 UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_TOP_CENTER_LEFT, UV_TOP_CENTER_RIGHT, UV_BOTTOM_CENTER_LEFT, UV_BOTTOM_CENTER_RIGHT, UV_TOP_RIGHT, UV_BOTTOM_RIGHT;

		private static Vector2[] startUvs, middleUvs, endUvs, fullUvs;

		private List<Vector2> points = new List<Vector2>(4);
		private List<Vector2> bezierPoints = new List<Vector2>(4);

		List<UIVertex[]> segments = new List<UIVertex[]>();
		protected override void OnPopulateMesh(VertexHelper vh)
		{
			if (points.Count < 4)
			{
				//base.OnPopulateMesh(vh);
				//return;

				for (int i = 0; i < 5; i++)
				{
					if (points.Count < i)
					{
						points.Add(Vector2.zero);
					}
				}
			}

			points[0] = _startPoint;
			points[1] = _startPoint + GetHeading(handleDistance, _startDirection);
			points[2] = _endPoint + GetHeading(handleDistance, _endDirection);
			points[3] = _endPoint;


			if (!GetImprovedDrawingPoints(bezierSegments, points, bezierPoints))
				return;

			vh.Clear();
			//var r = GetPixelAdjustedRect();
			//var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
			GeneratedUVs();
			var sizeX = 1;
			var sizeY = 1;
			//var offsetX = -rectTransform.pivot.x * sizeX;
			//var offsetY = -rectTransform.pivot.y * sizeY;
			var offsetX = 0;
			var offsetY = 0;

			segments.Clear();
			for (var i = 1; i < bezierPoints.Count; i++)
			{
				var start = bezierPoints[i - 1];
				var end = bezierPoints[i];
				start = new Vector2(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
				end = new Vector2(end.x * sizeX + offsetX, end.y * sizeY + offsetY);

				segments.Add(CreateLineSegment(start, end, SegmentType.Middle));


			}

			// Add the line segments to the vertex helper, creating any joins as needed
			for (var i = 0; i < segments.Count; i++)
			{
				if (i < segments.Count - 1)
				{
					var vec1 = segments[i][1].position - segments[i][2].position;
					var vec2 = segments[i + 1][2].position - segments[i + 1][1].position;
					var angle = Vector2.Angle(vec1, vec2) * Mathf.Deg2Rad;

					// Positive sign means the line is turning in a 'clockwise' direction
					var sign = Mathf.Sign(Vector3.Cross(vec1.normalized, vec2.normalized).z);

					// Calculate the miter point
					var miterDistance = (thickness * _thicknessScale) / (2 * Mathf.Tan(angle / 2));
					var miterPointA = segments[i][2].position - vec1.normalized * miterDistance * sign;
					var miterPointB = segments[i][3].position + vec1.normalized * miterDistance * sign;


					if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_BEVEL_NICE_JOIN)
					{
						if (sign < 0)
						{
							segments[i][2].position = miterPointA;
							segments[i + 1][1].position = miterPointA;
						}
						else
						{
							segments[i][3].position = miterPointB;
							segments[i + 1][0].position = miterPointB;
						}
					}

					var join = new UIVertex[] { segments[i][2], segments[i][3], segments[i + 1][0], segments[i + 1][1] };
					vh.AddUIVertexQuad(join);
				}

				vh.AddUIVertexQuad(segments[i]);
			}
		}

		private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end, SegmentType type, UIVertex[] previousVert = null)
		{
			Vector2 offset = new Vector2((start.y - end.y), end.x - start.x).normalized * (thickness * _thicknessScale) / 2;

			Vector2 v1 = Vector2.zero;
			Vector2 v2 = Vector2.zero;
			if (previousVert != null)
			{
				v1 = new Vector2(previousVert[3].position.x, previousVert[3].position.y);
				v2 = new Vector2(previousVert[2].position.x, previousVert[2].position.y);
			}
			else
			{
				v1 = start - offset;
				v2 = start + offset;
			}

			var v3 = end + offset;
			var v4 = end - offset;
			//Return the VDO with the correct uvs
			switch (type)
			{
				case SegmentType.Start:
					return SetVbo(new[] { v1, v2, v3, v4 }, startUvs);
				case SegmentType.End:
					return SetVbo(new[] { v1, v2, v3, v4 }, endUvs);
				case SegmentType.Full:
					return SetVbo(new[] { v1, v2, v3, v4 }, fullUvs);
				default:
					return SetVbo(new[] { v1, v2, v3, v4 }, middleUvs);
			}
		}

		protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs)
		{
			UIVertex[] vbo = new UIVertex[4];
			for (int i = 0; i < vertices.Length; i++)
			{
				var vert = UIVertex.simpleVert;
				vert.color = color;
				vert.position = vertices[i];
				vert.uv0 = uvs[i];
				vbo[i] = vert;
			}
			return vbo;
		}


		public Vector2 GetHeading(float distance, Direction direction)
		{
			switch (direction)
			{
				case Direction.Top:
					return new Vector2(0, 1f) * distance;
				case Direction.Down:
					return new Vector2(0, -1f) * distance;
				case Direction.Left:
					return new Vector2(-1f, 0) * distance;
				case Direction.Right:
					return new Vector2(1f, 0) * distance;
				default:
					return Vector2.zero;
			}
		}

		protected void GeneratedUVs()
		{
			//if (activeSprite != null)
			//{
			//	var outer = Sprites.DataUtility.GetOuterUV(activeSprite);
			//	var inner = Sprites.DataUtility.GetInnerUV(activeSprite);
			//	UV_TOP_LEFT = new Vector2(outer.x, outer.y);
			//	UV_BOTTOM_LEFT = new Vector2(outer.x, outer.w);
			//	UV_TOP_CENTER_LEFT = new Vector2(inner.x, inner.y);
			//	UV_TOP_CENTER_RIGHT = new Vector2(inner.z, inner.y);
			//	UV_BOTTOM_CENTER_LEFT = new Vector2(inner.x, inner.w);
			//	UV_BOTTOM_CENTER_RIGHT = new Vector2(inner.z, inner.w);
			//	UV_TOP_RIGHT = new Vector2(outer.z, outer.y);
			//	UV_BOTTOM_RIGHT = new Vector2(outer.z, outer.w);
			//}
			//else
			//{
			//}
			UV_TOP_LEFT = Vector2.zero;
			UV_BOTTOM_LEFT = new Vector2(0, 1);
			UV_TOP_CENTER_LEFT = new Vector2(0.5f, 0);
			UV_TOP_CENTER_RIGHT = new Vector2(0.5f, 0);
			UV_BOTTOM_CENTER_LEFT = new Vector2(0.5f, 1);
			UV_BOTTOM_CENTER_RIGHT = new Vector2(0.5f, 1);
			UV_TOP_RIGHT = new Vector2(1, 0);
			UV_BOTTOM_RIGHT = Vector2.one;


			startUvs = new[] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_CENTER_LEFT, UV_TOP_CENTER_LEFT };
			middleUvs = new[] { UV_TOP_CENTER_LEFT, UV_BOTTOM_CENTER_LEFT, UV_BOTTOM_CENTER_RIGHT, UV_TOP_CENTER_RIGHT };
			endUvs = new[] { UV_TOP_CENTER_RIGHT, UV_BOTTOM_CENTER_RIGHT, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };
			fullUvs = new[] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };
		}

		private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
		{
			float u = 1 - t;
			float tt = t * t;
			float uu = u * u;
			float uuu = uu * u;
			float ttt = tt * t;

			Vector2 p = uuu * p0; //first term

			p += 3 * uu * t * p1; //second term
			p += 3 * u * tt * p2; //third term
			p += ttt * p3; //fourth term

			return p;

		}


		private bool GetImprovedDrawingPoints(int segments, List<Vector2> points, List<Vector2> output)
		{
			if (points.Count < 4)
				return false;

			output.Clear();
			for (int i = 0; i < points.Count - 3; i += 3)
			{
				Vector2 p0 = points[i];
				Vector2 p1 = points[i + 1];
				Vector2 p2 = points[i + 2];
				Vector2 p3 = points[i + 3];

				if (i == 0) //only do this for the first end point. When i != 0, this coincides with the end point of the previous segment,
				{
					output.Add(CalculateBezierPoint(0, p0, p1, p2, p3));
				}

				for (int j = 1; j <= segments; j++)
				{
					float t = j / (float)segments;
					output.Add(CalculateBezierPoint(t, p0, p1, p2, p3));
				}
			}

			return true;
		}
	}

}

