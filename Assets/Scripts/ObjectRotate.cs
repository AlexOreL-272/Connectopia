using UnityEngine;
using System.Collections;

public class ObjectRotate : MonoBehaviour {

    public Transform target;
    public float rotationSpeed = 3.0f;
    public float minY = 0.0f;
    public float zoomSpeed = 2.0f;
    public float minZoom = 2.0f;
    public float maxZoom = 20.0f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float initialOrthographicSize;
    private Camera cam;

    void Start() {
        cam = GetComponent<Camera>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialOrthographicSize = cam.orthographicSize;
    }

    void Update() {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(horizontalInput) > 0 || Mathf.Abs(verticalInput) > 0) {
            float horizontal = rotationSpeed * Input.GetAxis("Horizontal");
            float vertical = rotationSpeed * Input.GetAxis("Vertical");
            
            transform.RotateAround(target.position, Vector3.up, -horizontal);
            transform.RotateAround(target.position, transform.right, vertical);

            if (transform.position.y < minY)
            {
                transform.position = new Vector3(transform.position.x, minY, transform.position.z);
            }
            

            transform.LookAt(target);
        }
        
        float newSize = cam.orthographicSize - zoomInput * zoomSpeed;
        newSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        cam.orthographicSize = newSize;
        
        if (Input.GetKeyDown(KeyCode.R)) {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            cam.orthographicSize = initialOrthographicSize;
        }
        
    }
}
