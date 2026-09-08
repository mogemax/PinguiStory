using UnityEngine;
using UnityEngine.InputSystem;
using PinguiStory.Player;
using PinguiStory.CameraSystem;

namespace PinguiStory.DebugTools
{
    /// <summary>
    /// TEMPORAL: permite alternar entre cámara de exploración y cámara de shooter
    /// con la tecla P, para probar la Fase 3 antes de que exista el piso 4 real
    /// que active este cambio automáticamente. Borrar cuando el piso 4 esté listo.
    /// </summary>
    public class ShooterModeDebugToggle : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private ThirdPersonCameraFollow exploreCamera;
        [SerializeField] private ShooterCameraFollow shooterCamera;

        private bool _isShooterMode;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            {
                Toggle();
            }
        }

        private void Toggle()
        {
            _isShooterMode = !_isShooterMode;

            if (exploreCamera != null)
            {
                exploreCamera.enabled = !_isShooterMode;
            }

            if (shooterCamera != null)
            {
                shooterCamera.enabled = _isShooterMode;
            }

            if (playerController != null)
            {
                playerController.SetAimMode(_isShooterMode);
            }
        }
    }
}
