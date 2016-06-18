using UnityEngine;

public static class MaterialHelper
{
    public static Material CreateTemp(Shader shader, string name)
    {
        Material material = new Material(shader);
        material.name = string.Format("{0}(Instance){1}", name, Random.Range(float.MinValue, float.MaxValue));
        material.hideFlags = HideFlags.HideAndDontSave;

        return material;
    }

    public static Material CreateTemp(Shader shader, string name, int renderingQueue)
    {
        Material material = CreateTemp(shader, name);
        material.renderQueue = renderingQueue;

        return material;
    }
}