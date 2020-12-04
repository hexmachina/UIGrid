using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TW.UI
{
	[RequireComponent(typeof(CanvasRenderer))]
	public class UICircle : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter
	{

		[SerializeField] private Sprite m_Sprite;
		public Sprite sprite { get { return m_Sprite; } set { if (SetClass(ref m_Sprite, value)) GeneratedUVs(); SetAllDirty(); } }

		[NonSerialized]
		private Sprite m_OverrideSprite;
		public Sprite overrideSprite { get { return activeSprite; } set { if (SetClass(ref m_OverrideSprite, value)) GeneratedUVs(); SetAllDirty(); } }

		protected Sprite activeSprite { get { return m_OverrideSprite != null ? m_OverrideSprite : sprite; } }

		internal float m_EventAlphaThreshold = 1;
		public float eventAlphaThreshold { get { return m_EventAlphaThreshold; } set { m_EventAlphaThreshold = value; } }

		public float pixelsPerUnit
		{
			get
			{
				float spritePixelsPerUnit = 100;
				if (activeSprite)
					spritePixelsPerUnit = activeSprite.pixelsPerUnit;

				float referencePixelsPerUnit = 100;
				if (canvas)
					referencePixelsPerUnit = canvas.referencePixelsPerUnit;

				return spritePixelsPerUnit / referencePixelsPerUnit;
			}
		}

		[Tooltip("The Arc Invert property will invert the construction of the Arc.")]
		public bool ArcInvert = true;

		[Tooltip("The Arc property is a percentage of the entire circumference of the circle.")]
		[Range(0, 1)]
		public float Arc = 1;

		[Tooltip("The Arc Steps property defines the number of segments that the Arc will be divided into.")]
		[Range(0, 1000)]
		public int ArcSteps = 100;

		[Tooltip("The Arc Rotation property permits adjusting the geometry orientation around the Z axis.")]
		[Range(0, 360)]
		public int ArcRotation = 0;

		[Tooltip("The Progress property allows the primitive to be used as a progression indicator.")]
		[Range(0, 1)]
		public float Progress = 0;
		private float _progress = 0;

		public Color ProgressColor = new Color(255, 255, 255, 255);
		public bool Fill = true; //solid circle
		public float Thickness = 5;
		public int Padding = 0;

		private List<int> indices = new List<int>();  //ordered list of vertices per tri
		private List<UIVertex> vertices = new List<UIVertex>();
		private Vector2 uvCenter = new Vector2(0.5f, 0.5f);

		protected override void OnEnable()
		{
			base.OnEnable();
			SetAllDirty();
		}

		#region ILayoutElement Interface

		public virtual void CalculateLayoutInputHorizontal() { }
		public virtual void CalculateLayoutInputVertical() { }

		public virtual float minWidth { get { return 0; } }

		public virtual float preferredWidth
		{
			get
			{
				if (overrideSprite == null)
					return 0;
				return overrideSprite.rect.size.x / pixelsPerUnit;
			}
		}

		public virtual float flexibleWidth { get { return -1; } }

		public virtual float minHeight { get { return 0; } }

		public virtual float preferredHeight
		{
			get
			{
				if (overrideSprite == null)
					return 0;
				return overrideSprite.rect.size.y / pixelsPerUnit;
			}
		}

		public virtual float flexibleHeight { get { return -1; } }

		public virtual int layoutPriority { get { return 0; } }

		static protected Material s_ETC1DefaultUI = null;
		static public Material defaultETC1GraphicMaterial
		{
			get
			{
				if (s_ETC1DefaultUI == null)
					s_ETC1DefaultUI = Canvas.GetETC1SupportedCanvasMaterial();
				return s_ETC1DefaultUI;
			}
		}

		#endregion

		public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
			// add test for line check
			if (m_EventAlphaThreshold >= 1)
				return true;

			Sprite sprite = overrideSprite;
			if (sprite == null)
				return true;

			Vector2 local;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out local);

			Rect rect = GetPixelAdjustedRect();

			// Convert to have lower left corner as reference point.
			local.x += rectTransform.pivot.x * rect.width;
			local.y += rectTransform.pivot.y * rect.height;

			local = MapCoordinate(local, rect);

			//test local coord with Mesh

			// Normalize local coordinates.
			Rect spriteRect = sprite.textureRect;
			Vector2 normalized = new Vector2(local.x / spriteRect.width, local.y / spriteRect.height);

			// Convert to texture space.
			float x = Mathf.Lerp(spriteRect.x, spriteRect.xMax, normalized.x) / sprite.texture.width;
			float y = Mathf.Lerp(spriteRect.y, spriteRect.yMax, normalized.y) / sprite.texture.height;

			try
			{
				return sprite.texture.GetPixelBilinear(x, y).a >= m_EventAlphaThreshold;
			}
			catch (UnityException e)
			{
				Debug.LogError("Using clickAlphaThreshold lower than 1 on Image whose sprite texture cannot be read. " + e.Message + " Also make sure to disable sprite packing for this sprite.", this);
				return true;
			}
		}

		private Vector2 MapCoordinate(Vector2 local, Rect rect)
		{
			Rect spriteRect = sprite.rect;
			return new Vector2(local.x * rect.width, local.y * rect.height);

		}

		bool SetClass<T>(ref T currentValue, T newValue) where T : class
		{
			if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
				return false;

			currentValue = newValue;
			return true;
		}

		public override Material material
		{
			get
			{
				if (m_Material != null)
					return m_Material;

				if (activeSprite && activeSprite.associatedAlphaSplitTexture != null)
					return defaultETC1GraphicMaterial;

				return defaultMaterial;
			}

			set
			{
				base.material = value;
			}
		}

		public override Texture mainTexture
		{
			get
			{
				if (activeSprite == null)
				{
					if (material != null && material.mainTexture != null)
					{
						return material.mainTexture;
					}
					return s_WhiteTexture;
				}

				return activeSprite.texture;
			}
		}

		protected virtual void GeneratedUVs() { }

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			int _inversion = ArcInvert ? -1 : 1;
			float Diameter = (rectTransform.rect.width < rectTransform.rect.height ? rectTransform.rect.width : rectTransform.rect.height) - Padding; //correct for padding and always fit RectTransform
			float outerDiameter = -rectTransform.pivot.x * Diameter;
			float innerDiameter = -rectTransform.pivot.x * Diameter + Thickness;

			vh.Clear();
			indices.Clear();
			vertices.Clear();

			int i = 0;
			int j = 1;
			int k = 0;

			float stepDegree = (Arc * 360f) / ArcSteps;
			_progress = ArcSteps * Progress;
			float rad = _inversion * Mathf.Deg2Rad * ArcRotation;
			float X = Mathf.Cos(rad);
			float Y = Mathf.Sin(rad);

			var vertex = UIVertex.simpleVert;
			vertex.color = _progress > 0 ? ProgressColor : color;

			//initial vertex
			vertex.position = new Vector2(outerDiameter * X, outerDiameter * Y);
			vertex.uv0 = new Vector2(vertex.position.x / Diameter + 0.5f, vertex.position.y / Diameter + 0.5f);
			vertices.Add(vertex);

			var iV = new Vector2(innerDiameter * X, innerDiameter * Y);
			if (Fill) iV = Vector2.zero; //center vertex to pivot
			vertex.position = iV;
			vertex.uv0 = Fill ? uvCenter : new Vector2(vertex.position.x / Diameter + 0.5f, vertex.position.y / Diameter + 0.5f);
			vertices.Add(vertex);

			for (int counter = 1; counter <= ArcSteps; counter++)
			{
				rad = _inversion * Mathf.Deg2Rad * (counter * stepDegree + ArcRotation);
				X = Mathf.Cos(rad);
				Y = Mathf.Sin(rad);

				vertex.color = counter > _progress ? color : ProgressColor;
				vertex.position = new Vector2(outerDiameter * X, outerDiameter * Y);
				vertex.uv0 = new Vector2(vertex.position.x / Diameter + 0.5f, vertex.position.y / Diameter + 0.5f);
				vertices.Add(vertex);

				//add additional vertex if required and generate indices for tris in clockwise order
				if (!Fill)
				{
					vertex.position = new Vector2(innerDiameter * X, innerDiameter * Y);
					vertex.uv0 = new Vector2(vertex.position.x / Diameter + 0.5f, vertex.position.y / Diameter + 0.5f);
					vertices.Add(vertex);
					k = j;
					indices.Add(i);
					indices.Add(j + 1);
					indices.Add(j);
					j++;
					i = j;
					j++;
					indices.Add(i);
					indices.Add(j);
					indices.Add(k);
				}
				else
				{
					indices.Add(i);
					indices.Add(j + 1);
					//Fills (solid circle) with progress require an additional vertex to 
					// prevent the base circle from becoming a gradient from center to edge
					if (counter > _progress)
					{
						indices.Add(ArcSteps + 2);
					}
					else
					{
						indices.Add(1);
					}

					j++;
					i = j;
				}
			}

			//this vertex is added to the end of the list to simplify index ordering on geometry fill
			if (Fill)
			{
				vertex.position = iV;
				vertex.color = color;
				vertex.uv0 = uvCenter;
				vertices.Add(vertex);
			}
			vh.AddUIVertexStream(vertices, indices);
		}

		//the following methods may be used during run-time
		//to update the properties of the component
		public void SetProgress(float progress)
		{
			Progress = progress;
			SetVerticesDirty();
		}

		public void SetArcSteps(int steps)
		{
			ArcSteps = steps;
			SetVerticesDirty();
		}

		public void SetInvertArc(bool invert)
		{
			ArcInvert = invert;
			SetVerticesDirty();
		}

		public void SetArcRotation(int rotation)
		{
			ArcRotation = rotation;
			SetVerticesDirty();
		}

		public void SetFill(bool fill)
		{
			Fill = fill;
			SetVerticesDirty();
		}

		public void SetBaseColor(Color color)
		{
			this.color = color;
			SetVerticesDirty();
		}

		public void UpdateBaseAlpha(float value)
		{
			var _color = this.color;
			_color.a = value;
			this.color = _color;
			SetVerticesDirty();
		}

		public void SetProgressColor(Color color)
		{
			ProgressColor = color;
			SetVerticesDirty();
		}

		public void UpdateProgressAlpha(float value)
		{
			ProgressColor.a = value;
			SetVerticesDirty();
		}

		public void SetPadding(int padding)
		{
			Padding = padding;
			SetVerticesDirty();
		}

		public void SetThickness(int thickness)
		{
			Thickness = thickness;
			SetVerticesDirty();
		}
	}

}

