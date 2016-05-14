using UnityEngine;

public class MouseOrbitImproved : GameCamera
{
    public Transform target;

    public float xSpeed = 2.0f;
    public float ySpeed = 60.0f;

    public float yMinLimit = -90f;
    public float yMaxLimit = 90f;

    public float distanceMin = 2.0f;
    public float distanceMax = 256.0f;

    private Rigidbody rb;

    float x = 0.0f;
    float y = 0.0f;
    float distance = 5.0f;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (target)
        {
            x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

            RaycastHit hit;

            if (Physics.Linecast(target.position, transform.position, out hit)) distance -= hit.distance;

            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1.0f * Time.deltaTime);
            transform.position = position; //Vector3.Slerp(transform.position, position, 1.0f * Time.deltaTime);
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    protected override void Init()
    {
        Vector3 angles = transform.eulerAngles;

        x = angles.y;
        y = angles.x;

        rb = GetComponent<Rigidbody>();

        if (target != null) distance = Vector3.Distance(target.position, transform.position);
        if (rb != null) rb.freezeRotation = true;
    }
}