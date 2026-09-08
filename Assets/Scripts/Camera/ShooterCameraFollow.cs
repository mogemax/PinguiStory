using UnityEngine;
using UnityEngine.InputSystem;

namespace PinguiStory.CameraSystem
{
    /// <summary>
    /// Cámara fija en tercera persona sobre el hombro (estilo Fortnite), usada solo
    /// durante el enfrentamiento del piso 4. A diferencia de ThirdPersonCameraFollow,
    /// no orbita libremente: mantiene un offset lateral fijo respecto al pivote y
    /// el yaw/pitch de la cámara son la referencia que el jugador debe mirar
    /// (ver PlayerController.SetAimMode).
    /// </summary>
    public class ShooterCameraFollow : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string lookActionName = "Look";

        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

        [Header("Orbit")]
        [SerializeField] private float mouseSensitivity = 0.15f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float initialPitch = 10f;

        [Header("Shoulder")]
        [SerializeField] private float sideOffset = 0.6f;
        [SerializeField] private float distanceBehind = 3f;

        [Header("Collision")]
        [SerializeField] private float minDistance = 0.5f;
        [SerializeField] private float collisionRadius = 0.2f;
        [SerializeField] private LayerMask collisionMask = ~0;

        [Header("Smoothing")]
        [SerializeField] private float positionSmoothTime = 0.05f;

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

            Vector3 desiredLocalOffset = new Vector3(sideOffset, 0f, -distanceBehind);
            Vector3 desiredPosition = pivot + rotation * desiredLocalOffset;

            desiredPosition = ResolveCollision(pivot, desiredPosition);

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

        private Vector3 ResolveCollision(Vector3 pivot, Vector3 desiredPosition)
        {
            Vector3 offset = desiredPosition - pivot;
            float desiredDistance = offset.magnitude;

            if (desiredDistance < 0.0001f)
            {
                return desiredPosition;
            }

            Vector3 direction = offset / desiredDistance;

            if (Physics.SphereCast(pivot, collisionRadius, direction, out RaycastHit hit, desiredDistance, collisionMask))
            {
                float clampedDistance = Mathf.Clamp(hit.distance, minDistance, desiredDistance);
                return pivot + direction * clampedDistance;
            }

            return desiredPosition;
        }
    }
}
