using UnityEngine;

public static class ScaledSpaceExtensions
{
    public static Vector3d LocalToScaledSpace(this Vector3d localSpacePoint)
    {
        return localSpacePoint * ScaledSpace.InverseScaleFactor - ScaledSpace.TotalOffset;
    }

    public static Vector3d ScaledToLocalSpace(this Vector3d scaledSpacePoint)
    {
        return (scaledSpacePoint + ScaledSpace.TotalOffset) * ScaledSpace.ScaleFactor;
    }
}

public class ScaledSpace : MonoBehaviour
{
    public float scaleFactor = 1.0f;

    public Transform originTarget;

    private static Vector3d totalOffset;

    public static Vector3d TotalOffset
    {
        get
        {
            return totalOffset;
        }
    }

    public static ScaledSpace Instance
    {
        get;
        private set;
    }

    public static float ScaleFactor
    {
        get
        {
            return (!Instance) ? 1f : Instance.scaleFactor;
        }
    }

    public static float InverseScaleFactor
    {
        get
        {
            return (!Instance) ? 1f : (1f / Instance.scaleFactor);
        }
    }

    public static Transform SceneTransform
    {
        get
        {
            return Instance.transform;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (!originTarget)
        {
            return;
        }
    }

    private void LateUpdate()
    {
        if (!originTarget) return;

        Vector3d b = originTarget.position;

        totalOffset += b;

        /*
        for (int i = scaledSpaceObjects.Count - 1; i >= 0; i--)
        {
            if (scaledSpaceObjects[i] == null)
            {
                scaledSpaceObjects.RemoveAt(i);
            }
            else
            {
                scaledSpaceObjects[i].transform.position -= b;
            }
        }
        */
    }

    /*
    public static void AddScaledSpaceObject(MapObject t)
    {
        if (ScaledSpace.Instance.scaledSpaceObjects.Contains(t))
        {
            Debug.LogWarning("Warning, MapObject " + t.name + " already exists in scaled space", t);
            return;
        }
        ScaledSpace.Instance.scaledSpaceObjects.Add(t);
    }

    public static void RemoveScaledSpaceObject(MapObject t)
    {
        Instance.scaledSpaceObjects.Remove(t);
    }
    */

    public static Vector3d LocalToScaledSpace(Vector3d localSpacePoint)
    {
        return localSpacePoint * InverseScaleFactor - totalOffset;
    }

    public static Vector3d ScaledToLocalSpace(Vector3d scaledSpacePoint)
    {
        return (scaledSpacePoint + totalOffset) * ScaleFactor;
    }
}