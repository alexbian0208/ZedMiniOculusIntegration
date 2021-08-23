using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{
public class LuxTeleportArea : LuxTeleportMarkerBase
{
   //Public properties
		public Bounds meshBounds { get; private set; }

		//Private data
		private MeshRenderer areaMesh;
		private int tintColorId = 0;
		private Color visibleTintColor = Color.clear;
		private Color highlightedTintColor = Color.clear;
		private Color lockedTintColor = Color.clear;
		private bool highlighted = false;

		//-------------------------------------------------
		protected override void Awake()
		{
			base.Awake();
			
			areaMesh = GetComponent<MeshRenderer>();

			tintColorId = Shader.PropertyToID( "_TintColor" );

			CalculateBounds();
		}


		//-------------------------------------------------
		protected override void Start()
		{
			base.Start();
			visibleTintColor = LuxTeleport2.instance.areaVisibleMaterial.GetColor( tintColorId );
			highlightedTintColor = LuxTeleport2.instance.areaHighlightedMaterial.GetColor( tintColorId );
			lockedTintColor = LuxTeleport2.instance.areaLockedMaterial.GetColor( tintColorId );
		}


		//-------------------------------------------------
		public override bool ShouldActivate( Vector3 playerPosition )
		{
			return true;
		}


		//-------------------------------------------------
		public override bool ShouldMovePlayer()
		{
			return true;
		}


		//-------------------------------------------------
		public override void Highlight( bool highlight )
		{
			if ( !locked )
			{
				highlighted = highlight;

				if ( highlight )
				{
					areaMesh.material = LuxTeleport2.instance.areaHighlightedMaterial;
				}
				else
				{
					areaMesh.material = LuxTeleport2.instance.areaVisibleMaterial;
				}
			}
		}


		//-------------------------------------------------
		public override void SetAlpha( float tintAlpha, float alphaPercent )
		{
			Color tintedColor = GetTintColor();
			tintedColor.a *= alphaPercent;
			areaMesh.material.SetColor( tintColorId, tintedColor );
		}


		//-------------------------------------------------
		public override void UpdateVisuals()
		{
			if ( locked )
			{
				areaMesh.material = LuxTeleport2.instance.areaLockedMaterial;
			}
			else
			{
				areaMesh.material = LuxTeleport2.instance.areaVisibleMaterial;
			}
		}


		//-------------------------------------------------
		public void UpdateVisualsInEditor()
		{
            if (LuxTeleport2.instance == null)
                return;

			areaMesh = GetComponent<MeshRenderer>();

			if ( locked )
			{
				areaMesh.sharedMaterial = LuxTeleport2.instance.areaLockedMaterial;
			}
			else
			{
				areaMesh.sharedMaterial = LuxTeleport2.instance.areaVisibleMaterial;
			}
		}


		//-------------------------------------------------
		private bool CalculateBounds()
		{
			MeshFilter meshFilter = GetComponent<MeshFilter>();
			if ( meshFilter == null )
			{
				return false;
			}

			Mesh mesh = meshFilter.sharedMesh;
			if ( mesh == null )
			{
				return false;
			}

			meshBounds = mesh.bounds;
			return true;
		}


		//-------------------------------------------------
		private Color GetTintColor()
		{
			if ( locked )
			{
				return lockedTintColor;
			}
			else
			{
				if ( highlighted )
				{
					return highlightedTintColor;
				}
				else
				{
					return visibleTintColor;
				}
			}
		}
	}
}
