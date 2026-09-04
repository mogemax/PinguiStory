using UnityEngine;
using UnityEngine.InputSystem;

namespace PinguiStory.CameraSystem
{
    /// <summary>
    /// Cámara orbital de tercera persona: orbita alrededor de "Target" con el mouse
    /// y evita atravesar paredes del laberinto mediante SphereCast.
    /// </summary>
    public class ThirdPersonCameraFollow : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string lookActionName = "Look";

        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

        [Header("Orbit")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float initialPitch = 15f;

        [Header("Distance")]
        [SerializeField] private float distance = 4f;
        [SerializeField] private float minDistance = 0.5f;
        [SerializeField] private float collisionRadius = 0.2f;
        [SerializeField] private LayerMask collisionMask = ~0;

        [Header("Smoothing")]
        [SerializeField] private float positionSmoothTime = 0.08f;

        private InputAction _lookAction;
        private float _yaw;
        private float _pitch;
        private Vector3 _currentVelocity;

        private void Awake()
        {
            InputActionMap map = inputActions.FindActionMap(actionMapName, throwIfNotFound: true);
            _lookAction = map.FindAction(lookActionName, throwIfNotFound: true);

            _pitch = initialPitch;
            _yaw = target != null ? target.eulerAngles.y : 0f;
        }

        private void OnEnable()
        {
            _lookAction.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            _lookAction.Disable();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            UpdateOrbitAngles();

            Vector3 pivot = target.position + targetOffset;
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            float clampedDistance = ResolveCollision(pivot, rotation);
            Vector3 desiredPosition = pivot - rotation * Vector3.forward * clampedDistance;

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, positionSmoothTime);
            transform.rotation = rotation;
        }

        private void UpdateOrbitAngles()
        {
            Vector2 lookInput = _lookAction.ReadValue<Vector2>();
            _yaw += lookInput.x * mouseSensitivity;
            _pitch -= lookInput.y * mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        private float ResolveCollision(Vector3 pivot, Quaternion rotation)
        {
            Vector3 direction = -(rotation * Vector3.forward);

            if (Physics.SphereCast(pivot, collisionRadius, direction, out RaycastHit hit, distance, collisionMask))
            {
                return Mathf.Clamp(hit.distance, minDistance, distance);
            }

            return distance;
        }
    }
}
