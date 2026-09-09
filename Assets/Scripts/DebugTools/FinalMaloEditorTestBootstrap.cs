using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using PinguiStory.CameraSystem;
using PinguiStory.Core;
using PinguiStory.Player;

namespace PinguiStory.DebugTools
{
    /// <summary>
    /// Prepara el rig mínimo para ejecutar FinalMalo directamente desde el editor,
    /// probando el encuentro de jefe y el modo shooter sin pasar por el flujo normal.
    /// </summary>
    public class FinalMaloEditorTestBootstrap : MonoBehaviour
    {
        [SerializeField] private PlayerController playerPrefab;
        [SerializeField] private ThirdPersonCameraFollow cameraPrefab;
        [SerializeField] private InputActionAsset inputActions;

        private void Awake()
        {
#if UNITY_EDITOR
            // Debe correr en Awake, no en Start: BossEncounterController activa el
            // encuentro en su propio Start() y necesita que PlayerController.Instance
            // y la cámara ya existan. Unity ejecuta todos los Awake antes que
            // cualquier Start, así que esto garantiza el orden sin tocar
            // Script Execution Order del proyecto.
            if (PlayerController.Instance == null)
            {
                CreateDirectTestRig();
            }

            DisableSceneCamerasExcept(ThirdPersonCameraFollow.Instance);
#else
            enabled = false;
#endif
        }

#if UNITY_EDITOR
        private void CreateDirectTestRig()
        {
            Transform spawn = FloorSpawnPoint.Current != null ? FloorSpawnPoint.Current.transform : transform;
            PlayerController player = Instantiate(playerPrefab, spawn.position, spawn.rotation);
            player.enabled = true;

            ThirdPersonCameraFollow explorationCamera = Instantiate(cameraPrefab);
            GameObject cameraObject = explorationCamera.gameObject;
            explorationCamera.SetTarget(player.transform);
            player.SetCameraTransform(cameraObject.transform);

            ShooterCameraFollow shooterCamera = cameraObject.AddComponent<ShooterCameraFollow>();
            shooterCamera.enabled = false;
            shooterCamera.Initialize(inputActions, player.transform);
        }

        private static void DisableSceneCamerasExcept(ThirdPersonCameraFollow activeFollowCamera)
        {
            if (activeFollowCamera == null)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            foreach (Camera sceneCamera in FindObjectsOfType<Camera>())
            {
                if (sceneCamera.gameObject == activeFollowCamera.gameObject || sceneCamera.gameObject.scene != activeScene)
                {
                    continue;
                }

                sceneCamera.enabled = false;
                AudioListener listener = sceneCamera.GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = false;
                }
            }
        }
#endif
    }
}
