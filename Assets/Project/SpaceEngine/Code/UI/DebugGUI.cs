using UnityEngine;

public abstract class DebugGUI : MonoBehaviour
{
    public Rect debugInfoBounds = new Rect(10, 10, 500, 500);

    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {

    }

    protected virtual void OnGUI()
    {
        GUI.depth = -100;
    }
}