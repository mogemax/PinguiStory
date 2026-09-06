using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PinguiStory.Player;
using PinguiStory.CameraSystem;

namespace PinguiStory.UI
{
    /// <summary>
    /// Controla la pantalla de menú principal: mientras el jugador no ha dado Start,
    /// la cámara muestra una toma fija de la torre y las montañas. Al presionar Start,
    /// la cámara baja en una sola toma continua hasta la posición de juego, y ahí se
    /// entrega el control al jugador (PlayerController + ThirdPersonCameraFollow).
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Cámara")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Transform menuViewPoint;
        [SerializeField] private Transform gameplayViewPoint;
        [SerializeField] private float transitionDuration = 2.5f;
        [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("UI")]
        [SerializeField] private Button startButton;
        [SerializeField] private CanvasGroup menuCanvasGroup;
        [SerializeField] private float uiFadeDuration = 0.5f;

        [Header("Jugador")]
        [SerializeField] private GameObject playerObject;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private ThirdPersonCameraFollow cameraFollow;

        private bool _transitioning;

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            SetupInitialState();
        }

        private void OnEnable()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartPressed);
            }
        }

        private void OnDisable()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(OnStartPressed);
            }
        }

        private void SetupInitialState()
        {
            if (menuViewPoint != null && mainCamera != null)
            {
                mainCamera.transform.SetPositionAndRotation(menuViewPoint.position, menuViewPoint.rotation);
            }

            if (playerController != null)
            {
                playerController.enabled = false;
            }

            if (cameraFollow != null)
            {
                cameraFollow.enabled = false;
            }

            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = 1f;
                menuCanvasGroup.interactable = true;
                menuCanvasGroup.blocksRaycasts = true;
            }
        }

        private void OnStartPressed()
        {
            if (_transitioning)
            {
                return;
            }

            _transitioning = true;

            if (startButton != null)
            {
                startButton.interactable = false;
            }

            StartCoroutine(TransitionToGameplay());
        }

        private IEnumerator TransitionToGameplay()
        {
            yield return StartCoroutine(FadeCanvasGroup(1f, 0f, uiFadeDuration));

            if (playerObject != null && !playerObject.activeSelf)
            {
                playerObject.SetActive(true);
            }

            Vector3 startPos = mainCamera.transform.position;
            Quaternion startRot = mainCamera.transform.rotation;

            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = easeCurve.Evaluate(Mathf.Clamp01(elapsed / transitionDuration));

                mainCamera.transform.position = Vector3.Lerp(startPos, gameplayViewPoint.position, t);
                mainCamera.transform.rotation = Quaternion.Slerp(startRot, gameplayViewPoint.rotation, t);

                yield return null;
            }

            mainCamera.transform.SetPositionAndRotation(gameplayViewPoint.position, gameplayViewPoint.rotation);

            EnableGameplayControl();

            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.gameObject.SetActive(false);
            }

            _transitioning = false;
        }

        private void EnableGameplayControl()
        {
            if (playerController != null)
            {
                playerController.enabled = true;
            }

            if (cameraFollow != null)
            {
                cameraFollow.enabled = true;
            }
        }

        private IEnumerator FadeCanvasGroup(float from, float to, float duration)
        {
            if (menuCanvasGroup == null)
            {
                yield break;
            }

            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                menuCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            menuCanvasGroup.alpha = to;
        }
    }
}