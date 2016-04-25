using UnityEngine;

public class HideFlagSetter : MonoBehaviour
{
    public HideFlags flag;

    public Object obj;

    [ContextMenu("Set Flag")]
    public void SetFlag()
    {
        obj.hideFlags = flag;
    }
}