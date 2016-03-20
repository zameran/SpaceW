using UnityEngine;

public static class QuadCacheUtility
{
    public static void BeginTransfer()
    {
        GUILayer[] gui = GameObject.FindObjectsOfType<GUILayer>();

        for (int i = 0; i < gui.Length; i++)
        {
            gui[i].enabled = false;
        }
    }

    public static void EndTransfer()
    {
        GUILayer[] gui = GameObject.FindObjectsOfType<GUILayer>();

        for (int i = 0; i < gui.Length; i++)
        {
            gui[i].enabled = true;
        }
    }
}