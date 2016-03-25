using UnityEngine;

public abstract class GameCamera : MonoBehaviour, ICamera
{
    protected virtual void Start()
    {
        Init();
    }

    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {

    }

    protected abstract void Init();
}