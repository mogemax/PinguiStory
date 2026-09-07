using UnityEngine;
using UnityEngine.InputSystem;

namespace PinguiStory.Player
{
    /// <summary>
    /// Movimiento en tercera persona relativo a la cámara, usando CharacterController.
    /// Requiere que "Camera Transform" apunte a la cámara controlada por ThirdPersonCameraFollow.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string jumpActionName = "Jump";
        [SerializeField] private string sprintActionName = "Sprint";

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float rotationSpeed = 12f;

        [Header("Gravity & Jump")]
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundedGravity = -2f;
        [SerializeField] private float jumpHeight = 1.2f;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController _controller;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;

        private Vector2 _moveInput;
        private Vector3 _verticalVelocity;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            _controller = GetComponent<CharacterController>();

            if (cameraTransform == null && UnityEngine.Camera.main != null)
            {
                cameraTransform = UnityEngine.Camera.main.transform;
            }

            InputActionMap map = inputActions.FindActionMap(actionMapName, throwIfNotFound: true);
            _moveAction = map.FindAction(moveActionName, throwIfNotFound: true);
            _jumpAction = map.FindAction(jumpActionName, throwIfNotFound: true);
            _sprintAction = map.FindAction(sprintActionName, throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _moveAction.Enable();
            _jumpAction.Enable();
            _sprintAction.Enable();
            _jumpAction.performed += OnJumpPerformed;
        }

        private void OnDisable()
        {
            _jumpAction.performed -= OnJumpPerformed;
            _moveAction.Disable();
            _jumpAction.Disable();
            _sprintAction.Disable();
        }

        private void Update()
        {
            _moveInput = _moveAction.ReadValue<Vector2>();

            ApplyGravity();
            Move();
        }

        private void Move()
        {
            Vector3 moveDirection = GetCameraRelativeDirection(_moveInput);

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                RotateTowards(moveDirection);
            }

            float currentSpeed = _sprintAction.IsPressed() ? sprintSpeed : moveSpeed;
            Vector3 horizontalMotion = moveDirection * currentSpeed;
            Vector3 motion = horizontalMotion + _verticalVelocity;

            _controller.Move(motion * Time.deltaTime);
        }

        private Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            if (cameraTransform == null)
            {
                return new Vector3(input.x, 0f, input.y);
            }

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * input.y + right * input.x);
        }

        private void RotateTowards(Vector3 direction)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (_controller.isGrounded && _verticalVelocity.y < 0f)
            {
                _verticalVelocity.y = groundedGravity;
            }
            else
            {
                _verticalVelocity.y += gravity * Time.deltaTime;
            }
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            if (_controller.isGrounded)
            {
                _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
    }
}
