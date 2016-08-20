using UnityEngine;

[ExecuteInEditMode]
public class RingShadow : Shadow
{
	public Texture Texture;
	
	public Ring Ring;
	
	public float InnerRadius = 1.0f;
	
	public float OuterRadius = 2.0f;
	
	public override Texture GetTexture()
	{
		return Texture;
	}
	
	public override bool CalculateShadow()
	{
		if (base.CalculateShadow() == true)
		{
			if (Texture != null)
			{
				if (Helper.Enabled(Ring) == true)
				{
					InnerRadius = Ring.InnerRadius;
					OuterRadius = Ring.OuterRadius;
				}
				
				var direction = default(Vector3);
				var position  = default(Vector3);
				var color     = default(Color);
				
				Helper.CalculateLight(Light, transform.position, null, null, ref position, ref direction, ref color);
				
				var rotation = Quaternion.FromToRotation(direction, Vector3.back);
				var squash   = Vector3.Dot(direction, transform.up); // Find how squashed the ellipse is based on light direction
				var width    = transform.lossyScale.x * OuterRadius;
				var length   = transform.lossyScale.z * OuterRadius;
				var axis     = rotation * transform.up; // Find the transformed up axis
				var spin     = Quaternion.LookRotation(Vector3.forward, new Vector2(-axis.x, axis.y)); // Orient the shadow ellipse
				var scale    = Helper.Reciprocal3(new Vector3(width, length * Mathf.Abs(squash), 1.0f));
				var skew     = Mathf.Tan(Helper.Acos(-squash));
				
				var shadowT = Helper.Translation(-transform.position);
				var shadowR = Helper.Rotation(spin * rotation); // Spin the shadow so lines up with its tilt
				var shadowS = Helper.Scaling(scale); // Scale the ring into an oval
				var shadowK = Helper.ShearingZ(new Vector2(0.0f, skew)); // Skew the shadow so it aligns with the ring plane
				
				Matrix = shadowS * shadowK * shadowR * shadowT;
				Ratio  = Helper.Divide(OuterRadius, OuterRadius - InnerRadius);
				
				return true;
			}
		}
		
		return false;
	}
	
#if UNITY_EDITOR
	protected virtual void OnDrawGizmosSelected()
	{
		if (Helper.Enabled(this) == true)
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			
			Helper.DrawCircle(Vector3.zero, Vector3.right * InnerRadius, Vector3.forward * InnerRadius);
			Helper.DrawCircle(Vector3.zero, Vector3.right * OuterRadius, Vector3.forward * OuterRadius);
			
			if (CalculateShadow() == true)
			{
				Gizmos.matrix = Matrix.inverse;
				
				Gizmos.DrawWireCube(new Vector3(0,0,5), new Vector3(2,2,10));
			}
		}
	}
#endif
}