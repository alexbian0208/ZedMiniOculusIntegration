using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{
	public class HighlightRenderer : MonoBehaviour
	{

		public bool highlightEnabled;
		protected MeshRenderer[] highlightRenderers;
		protected MeshRenderer[] existingRenderers;
		protected GameObject highlightHolder;
		protected SkinnedMeshRenderer[] highlightSkinnedRenderers;
		protected SkinnedMeshRenderer[] existingSkinnedRenderers;
		public Material highlightMat;

		[Tooltip("An array of child gameObjects to not render a highlight for. Things like transparent parts, vfx, etc.")]
		public GameObject[] hideHighlight;
		public bool overrideColor = false;
		public Color highlightColor;
		public float highlightWidth = 0.005f;
		private MaterialPropertyBlock materialPropertyBlock;
		public bool highlightDisabledRenderers;

		void Start()
		{
			materialPropertyBlock = new MaterialPropertyBlock();
			if (highlightMat == null)
			{
				highlightMat = Resources.Load<Material>("Caregivr_HighlightMaterial");
				if (highlightMat == null)
				{
					Debug.Assert(false, "[HighlightRenderer] Hover Highlight Material is missing.");
				}
			}
		}

		void Update()
		{
			if (highlightEnabled)
			{
				if (highlightHolder == null)
				{
					CreateHighlightRenderers();
				}
				UpdateHighlightRenderers();
			}
			else
			{
				if (highlightHolder != null)
				{
					Destroy(highlightHolder);
				}
			}
		}

		void OnDestroy()
		{
			if (highlightHolder != null)
			{
				Destroy(highlightHolder);
			}
		}

		bool ShouldIgnoreHighlight(Component component)
		{
			return ShouldIgnore(component.gameObject);
		}

		bool ShouldIgnore(GameObject check)
		{
			if (hideHighlight != null)
			{
				for (int ignoreIndex = 0; ignoreIndex < hideHighlight.Length; ignoreIndex++)
				{
					if (check == hideHighlight[ignoreIndex])
						return true;
				}
			}

			return false;
		}

		public void DisableHighlight()
		{
			highlightEnabled = false;
		}

		public void EnableHighlight()
		{
			highlightEnabled = true;
		}
		
		protected virtual void CreateHighlightRenderers()
		{
			existingSkinnedRenderers = this.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			highlightHolder = new GameObject("Highlighter");
			highlightSkinnedRenderers = new SkinnedMeshRenderer[existingSkinnedRenderers.Length];

			for (int skinnedIndex = 0; skinnedIndex < existingSkinnedRenderers.Length; skinnedIndex++)
			{
				SkinnedMeshRenderer existingSkinned = existingSkinnedRenderers[skinnedIndex];

				if (ShouldIgnoreHighlight(existingSkinned))
					continue;

				GameObject newSkinnedHolder = new GameObject("SkinnedHolder");
				newSkinnedHolder.transform.parent = highlightHolder.transform;
				SkinnedMeshRenderer newSkinned = newSkinnedHolder.AddComponent<SkinnedMeshRenderer>();
				Material[] materials = new Material[existingSkinned.sharedMaterials.Length];
				for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
				{
					materials[materialIndex] = highlightMat;
				}
				newSkinned.sharedMaterials = materials;
				newSkinned.sharedMesh = existingSkinned.sharedMesh;
				newSkinned.rootBone = existingSkinned.rootBone;
				newSkinned.updateWhenOffscreen = existingSkinned.updateWhenOffscreen;
				newSkinned.bones = existingSkinned.bones;

				highlightSkinnedRenderers[skinnedIndex] = newSkinned;
			}

			MeshFilter[] existingFilters = this.GetComponentsInChildren<MeshFilter>(true);
			existingRenderers = new MeshRenderer[existingFilters.Length];
			highlightRenderers = new MeshRenderer[existingFilters.Length];

			for (int filterIndex = 0; filterIndex < existingFilters.Length; filterIndex++)
			{
				MeshFilter existingFilter = existingFilters[filterIndex];
				MeshRenderer existingRenderer = existingFilter.GetComponent<MeshRenderer>();

				if (existingFilter == null || existingRenderer == null || ShouldIgnoreHighlight(existingFilter))
					continue;

				GameObject newFilterHolder = new GameObject("FilterHolder");
				newFilterHolder.transform.parent = highlightHolder.transform;
				MeshFilter newFilter = newFilterHolder.AddComponent<MeshFilter>();
				newFilter.sharedMesh = existingFilter.sharedMesh;
				MeshRenderer newRenderer = newFilterHolder.AddComponent<MeshRenderer>();
				
				Material[] materials = new Material[existingRenderer.sharedMaterials.Length];
				for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
				{
					materials[materialIndex] = highlightMat;
				}
				newRenderer.sharedMaterials = materials;

				highlightRenderers[filterIndex] = newRenderer;
				existingRenderers[filterIndex] = existingRenderer;
			}
		}

		protected virtual void UpdateHighlightRenderers()
		{
			if (highlightHolder == null)
				return;

			if (overrideColor)
			{
				materialPropertyBlock.SetColor("g_vOutlineColor", highlightColor);
				materialPropertyBlock.SetFloat("g_flOutlineWidth", highlightWidth);
			}

			for (int skinnedIndex = 0; skinnedIndex < existingSkinnedRenderers.Length; skinnedIndex++)
			{
				SkinnedMeshRenderer existingSkinned = existingSkinnedRenderers[skinnedIndex];
				SkinnedMeshRenderer highlightSkinned = highlightSkinnedRenderers[skinnedIndex];

				if (highlightEnabled && existingSkinned != null && highlightSkinned != null)
				{
					if (overrideColor)
					{
						highlightSkinned.SetPropertyBlock(materialPropertyBlock);
					}
					highlightSkinned.transform.position = existingSkinned.transform.position;
					highlightSkinned.transform.rotation = existingSkinned.transform.rotation;
					highlightSkinned.transform.localScale = existingSkinned.transform.lossyScale;
					highlightSkinned.localBounds = existingSkinned.localBounds;
					highlightSkinned.enabled = (existingSkinned.enabled || highlightDisabledRenderers) && existingSkinned.gameObject.activeInHierarchy;

					int blendShapeCount = existingSkinned.sharedMesh.blendShapeCount;
					for (int blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
					{
						highlightSkinned.SetBlendShapeWeight(blendShapeIndex, existingSkinned.GetBlendShapeWeight(blendShapeIndex));
					}
				}
				else if (highlightSkinned != null)
					highlightSkinned.enabled = false;

			}

			for (int rendererIndex = 0; rendererIndex < highlightRenderers.Length; rendererIndex++)
			{
				MeshRenderer existingRenderer = existingRenderers[rendererIndex];
				MeshRenderer highlightRenderer = highlightRenderers[rendererIndex];

				if (highlightEnabled && existingRenderer != null && highlightRenderer != null)
				{
					if (overrideColor)
					{
						highlightRenderer.SetPropertyBlock(materialPropertyBlock);
					}
					highlightRenderer.transform.position = existingRenderer.transform.position;
					highlightRenderer.transform.rotation = existingRenderer.transform.rotation;
					highlightRenderer.transform.localScale = existingRenderer.transform.lossyScale;
					highlightRenderer.enabled = (existingRenderer.enabled || highlightDisabledRenderers) && existingRenderer.gameObject.activeInHierarchy;
				}
				else if (highlightRenderer != null)
				{
					highlightRenderer.enabled = false;
				}
			}
		}


	}
}
