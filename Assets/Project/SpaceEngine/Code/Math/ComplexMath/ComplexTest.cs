using UnityEngine;

using ZFramework.Unity.Common.Messenger;

public class ComplexTest : MonoBehaviour
{
    public ComplexTranform[] Transforms;
    public ComplexVector Offset;

    public Vector3d offset = Vector3d.zero;

    public float Rim = 20000.0f;

    private void Start()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        for (int i = 0; i < Transforms.Length; i++)
        {
            Gizmos.DrawWireSphere(Transforms[i].RelativePosition, Rim);
        }
    }

    private void LateUpdate()
    {
        Shift();
    }

    private void Shift()
    {
        Vector3d currentOffset = this.transform.position;

        for (int i = 0; i < Transforms.Length; i++)
        {
            Transforms[i].RelativePosition -= currentOffset.ToVector3();
            Transforms[i].SetComplexPostion(Transforms[i].RelativePosition);
            Transforms[i].IncrementComplexPosition(Rim);
        }

        this.transform.position -= currentOffset.ToVector3();

        offset += currentOffset;
    }
}