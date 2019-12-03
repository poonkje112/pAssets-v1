namespace com.poonkje.Player
{
    using UnityEngine;

    [System.Serializable]
    class ClampAxis
    {
        public float negative, positive;
    }

    [RequireComponent(typeof(Rigidbody))]
    public class FirstPersonPlayerController : MonoBehaviour
    {
        [Header("Player Settings")] [SerializeField]
        float movementSpeed = 1f;

        [SerializeField] float sensitivity = 3f;
        [SerializeField] ClampAxis clampAxis = new ClampAxis() {negative = -45, positive = 45};
        [SerializeField] Camera playerCamera;
        [SerializeField] bool overrideRigidbodySettings = true;
        Collider _Collider;
        Rigidbody _Rig;

        void Start()
        {
            _Rig = GetComponent<Rigidbody>();

            if (overrideRigidbodySettings)
            {
                _Rig.constraints = RigidbodyConstraints.FreezeRotation;
                _Rig.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }

            if (playerCamera == null && !TryFindMainCamera(out playerCamera))
            {
                GameObject cameraObject = new GameObject("PlayerCamera") {tag = "MainCamera"};
                Camera cam = cameraObject.AddComponent<Camera>();
                cam.orthographic = false;
                playerCamera = cam;
            }

            if (!TryGetComponent(out _Collider))
            {
                Debug.LogWarning("Failed to get 3D Collider, adding one...", gameObject);
                _Collider = gameObject.AddComponent<BoxCollider>(); // Checking if the object has an 3D COllider
            }

            if (GetComponentInChildren<Camera>() == null || playerCamera.transform.parent == transform)
            {
                GameObject camRoot = new GameObject("Camera Root");
                camRoot.transform.parent = transform;
                camRoot.transform.localPosition = Vector3.zero;
                playerCamera.transform.parent = camRoot.transform;
                playerCamera.transform.localPosition = Vector3.zero;
                playerCamera.transform.localRotation = Quaternion.identity;
            }
        }

        void FixedUpdate()
        {
            MovementHandle();
        }

        void Update()
        {
            LookHandle();
        }

        void MovementHandle()
        {
            Vector3 axis = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            if (axis != Vector3.zero)
            {
                //Getting our direction from the camera
                Vector3 moveDir = playerCamera.transform.TransformDirection(axis);
                
                //Setting our movedir y to 0 to prevent the player from going up
                moveDir.y = 0;
                
                //Moving the player
                _Rig.MovePosition(transform.position + moveDir * movementSpeed);
            }
        }

        void LookHandle()
        {
            //Motor Initialization
            Quaternion horizontalMotor = playerCamera.transform.parent.localRotation;
            Quaternion verticalMotor = playerCamera.transform.localRotation;
            Vector2 axis = new Vector2(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X")) * sensitivity;
            
            //Horizontal Rotation
            Quaternion xAxis = Quaternion.AngleAxis(axis.y, Vector3.up);
            playerCamera.transform.parent.localRotation = horizontalMotor * xAxis;
            
            //Vertical Rotation
            Quaternion yAxis = Quaternion.AngleAxis(axis.x, Vector3.left);
            playerCamera.transform.localRotation = verticalMotor * yAxis;

            //Clamping our rotation
            Vector3 tempRot = playerCamera.transform.localEulerAngles;
            tempRot.x = ClampAngle(tempRot.x, clampAxis.negative, clampAxis.positive);
            playerCamera.transform.localEulerAngles = tempRot;
        }
        
        float ClampAngle(float angle, float min, float max)
        {
            if (angle < 0f) angle = 360 + angle;
            return angle > 180f ? Mathf.Max(angle, 360+min) : Mathf.Min(angle, max);
        }
        
        bool TryFindMainCamera(out Camera cam)
        {
            cam = Camera.main;
            return cam != null;
        }
    }
}