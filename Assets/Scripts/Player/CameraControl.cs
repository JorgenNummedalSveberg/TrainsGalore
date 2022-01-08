using UnityEngine;

public class CameraControl : MonoBehaviour
{

    public float mouseSensitivity = 100;

    public Transform playerBody;

    private float _xRotation;

    public Camera cam;

    [Range(0, 1000)]public float treeClippingPlane;

    
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float[] distances = new float[32];
        distances[6] = treeClippingPlane;
        cam.layerCullDistances = distances;
        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        playerBody.Rotate(Vector3.up * mouseX);
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }
}
