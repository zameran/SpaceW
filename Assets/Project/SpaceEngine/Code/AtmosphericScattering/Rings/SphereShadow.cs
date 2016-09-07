using UnityEngine;

public class SphereShadow : Shadow
{
    public float InnerRadius = 1.0f;
    public float OuterRadius = 2.0f;

    public Gradient PenumbraBrightness = new Gradient();
    public Gradient PenumbraColor = new Gradient();

    private Texture2D penumbraLut;

    private static GradientColorKey[] defaultLightingBrightness = new GradientColorKey[] {new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.white, 1.0f)};
    private static GradientColorKey[] defaultLightingColor = new GradientColorKey[] {new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.white, 1.0f)};

    private static Vector3[] vectors = new Vector3[3];

    private static float[] magnitudes = new float[3];

    public override Texture GetTexture()
    {
        RegenerateLightingLut();

        return penumbraLut;
    }

    public override bool CalculateShadow()
    {
        if (base.CalculateShadow() == true)
        {
            var direction = default(Vector3);
            var position = default(Vector3);
            var color = default(Color);

            Helper.CalculateLight(Light, transform.position, null, null, ref position, ref direction, ref color);

            var rotation = Quaternion.FromToRotation(direction, Vector3.back);

            SetVector(0, rotation * transform.right * transform.lossyScale.x * OuterRadius);
            SetVector(1, rotation * transform.up * transform.lossyScale.y * OuterRadius);
            SetVector(2, rotation * transform.forward * transform.lossyScale.z * OuterRadius);

            SortVectors();

            var spin = Quaternion.LookRotation(Vector3.forward, new Vector2(-vectors[1].x, vectors[1].y)); // Orient the shadow ellipse
            var scale = Helper.Reciprocal3(new Vector3(magnitudes[0], magnitudes[1], 1.0f));

            var shadowT = Helper.Translation(-transform.position);
            var shadowR = Helper.Rotation(spin * rotation);
            var shadowS = Helper.Scaling(scale);

            Matrix = shadowS * shadowR * shadowT;
            Ratio = Helper.Divide(OuterRadius, OuterRadius - InnerRadius);

            return true;
        }

        return false;
    }

    protected virtual void Awake()
    {
        PenumbraBrightness.colorKeys = defaultLightingBrightness;
        PenumbraColor.colorKeys = defaultLightingColor;

        RegenerateLightingLut();
    }

    protected virtual void OnDestroy()
    {
        Helper.Destroy(penumbraLut);
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        if (Helper.Enabled(this) == true)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.DrawWireSphere(Vector3.zero, InnerRadius);
            Gizmos.DrawWireSphere(Vector3.zero, OuterRadius);

            if (CalculateShadow() == true)
            {
                Gizmos.matrix = Matrix.inverse;

                Gizmos.DrawWireCube(new Vector3(0, 0, 5), new Vector3(2, 2, 10));
            }
        }
    }
#endif

    private void RegenerateLightingLut()
    {
        if (penumbraLut == null || penumbraLut.width != 1 || penumbraLut.height != 64)
        {
            Helper.Destroy(penumbraLut);

            penumbraLut = Helper.CreateTempTeture2D(1, 64);
        }

        for (var y = 0; y < penumbraLut.height; y++)
        {
            var t = y / (float)penumbraLut.height;
            var a = PenumbraBrightness.Evaluate(t);
            var b = PenumbraColor.Evaluate(t);
            var c = a * b;

            c.a = c.grayscale;

            penumbraLut.SetPixel(0, y, c);
        }

        // Make sure the last pixel is white
        penumbraLut.SetPixel(0, penumbraLut.height - 1, Color.white);

        penumbraLut.wrapMode = TextureWrapMode.Clamp;
        penumbraLut.Apply();
    }

    private void SetVector(int index, Vector3 vector)
    {
        vectors[index] = vector;

        magnitudes[index] = new Vector2(vector.x, vector.y).magnitude;
    }

    // Put the highest magnitude vectors in indices 0 & 1
    private void SortVectors()
    {
        // Lowest is 0 or 2
        if (magnitudes[0] < magnitudes[1])
        {
            // Lowest is 0
            if (magnitudes[0] < magnitudes[2])
            {
                vectors[0] = vectors[2];
                magnitudes[0] = magnitudes[2];
            }
        }

        // Lowest is 1 or 2
        else
        {
            // Lowest is 1
            if (magnitudes[1] < magnitudes[2])
            {
                vectors[1] = vectors[2];
                magnitudes[1] = magnitudes[2];
            }
        }
    }
}