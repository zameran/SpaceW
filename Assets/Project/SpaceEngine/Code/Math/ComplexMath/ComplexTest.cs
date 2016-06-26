using UnityEngine;

using ZFramework.Unity.Common.Messenger;

[RequireComponent(typeof(ComplexTranform))]
public class ComplexTest : MonoBehaviour
{
    #region OwnComplexTransform
    private ComplexTranform ownComplexTransform;
    public ComplexTranform OwnComplexTransform
    {
        get
        {
            if (ownComplexTransform != null)
                return ownComplexTransform;
            else
                ownComplexTransform = this.GetComponent<ComplexTranform>();

            return ownComplexTransform;
        }

        private set
        {
            ownComplexTransform = value;
        }
    }
    #endregion

    public ComplexTranform[] Transforms;
    public ComplexVector Offset;

    public Vector3d RelativeOffset = Vector3d.zero;

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
        Vector3d CurrentRelativeOffset = OwnComplexTransform.RelativePosition;

        for (int i = 0; i < Transforms.Length; i++)
        {
            Transforms[i].RelativePosition -= CurrentRelativeOffset;
            Transforms[i].SetComplexPostion(Transforms[i].RelativePosition);
            Transforms[i].CalculateComplexPosition(Rim);
        }

        OwnComplexTransform.RelativePosition -= CurrentRelativeOffset;

        RelativeOffset += CurrentRelativeOffset;

        SetComplexOffset(RelativeOffset);
    }

    private void SetComplexOffset(Vector3d offset)
    {
        Offset = new ComplexVector(new Complex(offset.x, Offset.x.Imaginary),
                                   new Complex(offset.y, Offset.y.Imaginary),
                                   new Complex(offset.z, Offset.z.Imaginary));
    }
}