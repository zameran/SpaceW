using UnityEngine;

using ZFramework.Unity.Common;
using ZFramework.Unity.Common.Messenger;

public sealed class GodManager : MonoBehaviour
{
    private void Awake()
    {
        Messenger.Setup(true);
    }
}