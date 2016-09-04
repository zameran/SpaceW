using UnityEngine;

[ExecuteInEditMode]
public class RingSegment : MonoBehaviour
{
    public Ring Ring;

    public MeshFilter MeshFilter;

    public MeshRenderer MeshRenderer;

    public void ManualUpdate(Mesh mesh, Material material, Quaternion rotation)
    {
        if (Ring != null)
        {
            if (MeshFilter == null) MeshFilter = gameObject.AddComponent<MeshFilter>();

            if (MeshRenderer == null) MeshRenderer = gameObject.AddComponent<MeshRenderer>();

            if (MeshFilter.sharedMesh != mesh)
            {
                Helper.BeginStealthSet(MeshFilter);
                {
                    MeshFilter.sharedMesh = mesh;
                }
                Helper.EndStealthSet();
            }

            if (MeshRenderer.sharedMaterial != material)
            {
                Helper.BeginStealthSet(MeshRenderer);
                {
                    MeshRenderer.sharedMaterial = material;
                }
                Helper.EndStealthSet();
            }

            Helper.SetLocalRotation(transform, rotation);
        }
    }

    public static RingSegment Create(Ring ring)
    {
        var segment = ComponentPool<RingSegment>.Pop("Segment", ring.transform);

        segment.Ring = ring;

        return segment;
    }

    public static void Pool(RingSegment segment)
    {
        if (segment != null)
        {
            segment.Ring = null;

            ComponentPool<RingSegment>.Add(segment);
        }
    }

    public static void MarkForDestruction(RingSegment segment)
    {
        if (segment != null)
        {
            segment.Ring = null;

            segment.gameObject.SetActive(true);
        }
    }

    protected virtual void Update()
    {
        if (Ring == null)
        {
            Pool(this);
        }
    }
}