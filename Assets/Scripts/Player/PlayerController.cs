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

        [Header("Animator")]
        [SerializeField] private string vertParameterName = "Vert";
        [SerializeField] private string stateParameterName = "State";
        [SerializeField] private float animatorDampTime = 0.15f;

        [Header("Slide")]
        [SerializeField] private string slideActionName = "Crouch";
        [SerializeField] private float slideBoostMultiplier = 1.6f;
        [SerializeField] private float slideTiltAngle = 65f;
        [SerializeField] private float slideTiltSmoothTime = 0.12f;
        [SerializeField] private Transform meshRigRoot;

        private CharacterController _controller;
        private Animator _animator;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;
        private InputAction _slideAction;

        private int _vertHash;
        private int _stateHash;

        private Vector2 _moveInput;
        private Vector3 _verticalVelocity;

        private bool _isSliding;
        private float _currentTilt;
        private float _tiltVelocity;
        private Quaternion _meshRigBaseRotation;

        private bool _isAimMode;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            _controller = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();

            if (cameraTransform == null && UnityEngine.Camera.main != null)
            {
                cameraTransform = UnityEngine.Camera.main.transform;
            }

            if (meshRigRoot == null)
            {
                meshRigRoot = transform.Find("Pinguin_001_rig");
            }

            if (meshRigRoot != null)
            {
                _meshRigBaseRotation = meshRigRoot.localRotation;
            }

            _vertHash = Animator.StringToHash(vertParameterName);
            _stateHash = Animator.StringToHash(stateParameterName);

            InputActionMap map = inputActions.FindActionMap(actionMapName, throwIfNotFound: true);
            _moveAction = map.FindAction(moveActionName, throwIfNotFound: true);
            _jumpAction = map.FindAction(jumpActionName, throwIfNotFound: true);
            _sprintAction = map.FindAction(sprintActionName, throwIfNotFound: true);
            _slideAction = map.FindAction(slideActionName, throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _moveAction.Enable();
            _jumpAction.Enable();
            _sprintAction.Enable();
            _slideAction.Enable();
            _jumpAction.performed += OnJumpPerformed;
        }

        private void OnDisable()
        {
            _jumpAction.performed -= OnJumpPerformed;
            _moveAction.Disable();
            _jumpAction.Disable();
            _sprintAction.Disable();
            _slideAction.Disable();
        }

        private void Update()
        {
            _moveInput = _moveAction.ReadValue<Vector2>();

            UpdateSlideState();
            ApplyGravity();
            Move();
            UpdateAnimator();
        }

        private void LateUpdate()
        {
            if (meshRigRoot == null)
            {
                return;
            }

            float targetTilt = _isSliding ? slideTiltAngle : 0f;
            _currentTilt = Mathf.SmoothDampAngle(_currentTilt, targetTilt, ref _tiltVelocity, slideTiltSmoothTime);
            meshRigRoot.localRotation = _meshRigBaseRotation * Quaternion.Euler(_currentTilt, 0f, 0f);
        }

        private void UpdateSlideState()
        {
            _isSliding = _slideAction.IsPressed() && _controller.isGrounded && _moveInput.sqrMagnitude > 0.01f;
        }

        /// <summary>
        /// Modo puntería (piso 4 / shooter): el pingüino siempre mira hacia donde apunta
        /// la cámara, en vez de girar hacia la dirección de movimiento. Permite strafear.
        /// </summary>
        public void SetAimMode(bool enabled)
        {
            _isAimMode = enabled;
        }

        private void Move()
        {
            Vector3 moveDirection = GetCameraRelativeDirection(_moveInput);

            if (_isAimMode)
            {
                RotateTowardsCameraYaw();
            }
            else if (moveDirection.sqrMagnitude > 0.0001f)
            {
                RotateTowards(moveDirection);
            }

            float currentSpeed = _sprintAction.IsPressed() ? sprintSpeed : moveSpeed;

            if (_isSliding)
            {
                currentSpeed *= slideBoostMultiplier;
            }

            Vector3 horizontalMotion = moveDirection * currentSpeed;
            Vector3 motion = horizontalMotion + _verticalVelocity;

            _controller.Move(motion * Time.deltaTime);
        }

        private void UpdateAnimator()
        {
            if (_isSliding)
            {
                _animator.SetFloat(_vertHash, 0f, animatorDampTime, Time.deltaTime);
                _animator.SetFloat(_stateHash, 0f, animatorDampTime, Time.deltaTime);
                return;
            }

            float moveMagnitude = Mathf.Clamp01(_moveInput.magnitude);
            float stateValue = _sprintAction.IsPressed() ? 1f : 0f;

            _animator.SetFloat(_vertHash, moveMagnitude, animatorDampTime, Time.deltaTime);
            _animator.SetFloat(_stateHash, stateValue, animatorDampTime, Time.deltaTime);
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

        private void RotateTowardsCameraYaw()
        {
            if (cameraTransform == null)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
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
