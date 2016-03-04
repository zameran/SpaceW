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
            zRotation -= 10;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            zRotation += 10;
        }
        else
        {
            zRotation = 0;
        }

        zRotation = Mathf.Clamp(zRotation, -100, 100);

        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            targetRotation = Quaternion.LookRotation((ray.origin + ray.direction * 10f) - transform.position, transform.up);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);

            if (!aligned)
                transform.Rotate(new Vector3(0, 0, zRotation) * Time.fixedDeltaTime * rotationSpeed);
        }
        else
        {
            if (!aligned)
                transform.Rotate(new Vector3(0, 0, zRotation) * Time.fixedDeltaTime * rotationSpeed);
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

        transform.Translate(velocity * currentSpeed);
    }
}