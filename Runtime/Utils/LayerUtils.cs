using UnityEngine;

public static class LayerUtils
{
    public static void SetLayerRecursive(GameObject obj, int layer)
    {
        if (obj == null) { return; }

        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
}