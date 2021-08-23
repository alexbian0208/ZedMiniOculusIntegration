using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{
	public class MaterialOverrider : MonoBehaviour
	{
		public Dictionary<Renderer, Material[]> originalMaterials { get; private set; }
		public Renderer[] renderers { get; private set; }
		public Material overridingMaterial;
		public List<Renderer> excludeRenderers;

		void Awake()
		{
			FetchRenderers();
			
		}

		void Start()
		{
			if (overridingMaterial != null)
			{
				doOverrideMaterials(overridingMaterial);
			}
		}

		public void FetchRenderers()
		{
			Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
			List<Renderer> overridingRenderers = new List<Renderer>();
			foreach (Renderer childRenderer in childRenderers)
			{
				if (!excludeRenderers.Contains(childRenderer))
				{
					overridingRenderers.Add(childRenderer);
				}
			}
			renderers = overridingRenderers.ToArray();
			
			originalMaterials = MapRenderersToMaterials(renderers);
		}
		
		public void RestoreMaterials()
		{
			if (overridingMaterial != null)
			{
				overridingMaterial = null;
				for (int i = 0; i < renderers.Length; i++)
				{
					Renderer renderer = renderers[i];
					renderer.materials = originalMaterials[renderer];
				}
			}
		}

		public void OverrideMaterials(Material material)
		{
			if (overridingMaterial != material)
			{
				doOverrideMaterials(material);
			}
		}
		private void doOverrideMaterials(Material material)
		{
			overridingMaterial = material;
			if (renderers == null)
			{
				Debug.LogWarning("[MaterialOverrider] renderers were null in override on gameobject: " + gameObject.name);
				FetchRenderers();
			}
			for (int i = 0; i < renderers.Length; i++)
			{
				Renderer renderer = renderers[i];

				// TODO
				// Dont create arrays at runtime like this
				Material[] overridingMaterialArray = new Material[renderer.materials.Length];
				for (int m = 0; m < renderer.materials.Length; m++)
				{
					overridingMaterialArray[m] = overridingMaterial;
				}

				renderer.materials = overridingMaterialArray;
			}
		}
		public void RemapBaseMaterials(Material material)
		{
			OverrideMaterials(material);
			originalMaterials = MapRenderersToMaterials(renderers);
		}

		public static Dictionary<Renderer, Material[]> MapRenderersToMaterials(Renderer[] renderers)
		{
			Dictionary<Renderer, Material[]> rendererMaterialsMap = new Dictionary<Renderer, Material[]>();
			for (int i = 0; i < renderers.Length; i++)
			{
				Renderer renderer = renderers[i];
				Material[] rendererMaterials = new Material[renderer.materials.Length];
				for (int m = 0; m < renderer.materials.Length; m++)
				{
					rendererMaterials[m] = renderer.materials[m];
				}
				rendererMaterialsMap.Add(renderer, rendererMaterials);
			}
			return rendererMaterialsMap;
		}
	}
}
