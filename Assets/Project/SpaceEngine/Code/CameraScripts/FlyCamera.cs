using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    public Camera cameraComponent = null;

    public float speed = 0.1f;
    public float rotationSpeed = 3.0f;
    public float alignDistance = 1024.0f;

    public Planetoid planetoid;

    private float currentSpeed;
    private float zRotation = 0;

    private Vector3 velocity = Vector3.zero;
    private Quaternion targetRotation = Quaternion.identity;

    public float distanceToPlanetCore;

    private float nearClipPlaneCache;
    private float farClipPlaneCache;

    public bool dynamicClipPlanes = false;
    public bool aligned = false;

    private float x;
    private float y;
    private float z;

    void Start()
    {
        Init();
    }

    private void Init()
    {
        if (cameraComponent == null)
            cameraComponent = this.GetComponent<Camera>();

        nearClipPlaneCache = cameraComponent.nearClipPlane;
        farClipPlaneCache = cameraComponent.farClipPlane;

        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void FixedUpdate()
    {
        if (dynamicClipPlanes)
        {
            if (planetoid != null)
            {
                float h = (distanceToPlanetCore - planetoid.PlanetRadius);

                cameraComponent.nearClipPlane = Mathf.Clamp(0.1f * h, 0.01f, 1000.0f);
                cameraComponent.farClipPlane = Mathf.Clamp(1e6f * h, 1000.0f, 1e10f);
            }
            else
            {
                cameraComponent.nearClipPlane = nearClipPlaneCache;
                cameraComponent.farClipPlane = farClipPlaneCache;
            }
        }
        else
        {
            cameraComponent.nearClipPlane = nearClipPlaneCache;
            cameraComponent.farClipPlane = farClipPlaneCache;
        }

        if (Input.GetKey(KeyCode.E))
        {
            zRotation -= 1 * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            zRotation += 1 * Time.deltaTime;
        }
        else
        {
            zRotation = Mathf.Lerp(zRotation, 0, Time.deltaTime * 2);
        }

        zRotation = Mathf.Clamp(zRotation, -100, 100);

        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            targetRotation = Quaternion.LookRotation((ray.origin + ray.direction * 10f) - transform.position, transform.up);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);

            if (!aligned)
                transform.Rotate(new Vector3(0, 0, zRotation));
        }
        else if (Input.GetMouseButton(1))
        {
            x += (Input.GetAxis("Mouse X") * 60.0f * 0.02f);
            y -= (Input.GetAxis("Mouse Y") * 30.0f * 0.02f);
            z = zRotation;

            if (!aligned)
                RotateAround(x, y, z, new Vector3(0, 0, -distanceToPlanetCore));
        }
        else
        {
            if (!aligned)
                transform.Rotate(new Vector3(0, 0, zRotation));
        }

        if (planetoid != null)
        {
            distanceToPlanetCore = Vector3.Distance(transform.position, planetoid.transform.position);

            if (distanceToPlanetCore < alignDistance)
            {
                aligned = true;

                Vector3 gravityVector = planetoid.transform.position - transform.position;

                targetRotation = Quaternion.LookRotation(transform.forward, -gravityVector);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed * 3f);
            }
            else
            {
                aligned = false;
            }
        }

        velocity.z = Input.GetAxis("Vertical");
        velocity.x = Input.GetAxis("Horizontal");

        currentSpeed = speed;

        if (Input.GetKey(KeyCode.LeftShift))
            currentSpeed = speed * 10f;
        if (Input.GetKey(KeyCode.LeftAlt))
            currentSpeed = speed * 100f;
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftAlt))
            currentSpeed = speed * 1000f;
        if (Input.GetKey(KeyCode.LeftControl))
            currentSpeed = speed / 10f;

        speed += Mathf.RoundToInt(Input.GetAxis("Mouse ScrollWheel") * 10.0f);
        speed = Mathf.Clamp(speed, 1, 100);

        transform.Translate(velocity * currentSpeed);
    }

    public float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }

    void RotateAround(float x, float y, float z, Vector3 distanceVector)
    {
        Quaternion rotation = Quaternion.Euler(y + targetRotation.x, x + targetRotation.y, z + targetRotation.z);

        Vector3 position = rotation * distanceVector + planetoid.transform.position;

        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * rotationSpeed);
        transform.position = position;
    }
}