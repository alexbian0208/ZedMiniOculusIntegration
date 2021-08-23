using UnityEngine;

public static class GameObjectExtensions
{
	// Sets the layer for this gameobject and all of its children
	public static void SetLayerRecursive(this GameObject gameObject, int layer)
	{
		gameObject.layer = layer;
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			gameObject.transform.GetChild(i).gameObject.SetLayerRecursive(layer);
		}
	}
}
