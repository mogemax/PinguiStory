using UnityEngine;
using PinguiStory.Combat;
using PinguiStory.CameraSystem;
using PinguiStory.Enemies;
using PinguiStory.Player;

namespace PinguiStory.Core
{
    /// <summary>
    /// Orquesta el encuentro de la jefa en la escena SalaDelJefe: al cargar la
    /// escena activa a la jefa y cambia de cámara de exploración a cámara de
    /// shooter. Si la jefa atrapa al jugador, reinicia el intento; si la
    /// derrota, cierra la partida volviendo al menú principal.
    /// </summary>
    public class BossEncounterController : MonoBehaviour
    {
        [Header("Jefe")]
        [SerializeField] private GiantPinguinaController boss;

        // Jugador y cámara persisten entre escenas (DontDestroyOnLoad) y viven en
        // Menu&Inicio.unity, no en esta escena: no se pueden arrastrar en el Inspector
        // porque Unity no permite referencias serializadas entre archivos de escena
        // distintos. Se resuelven en runtime, una vez que ya fueron cargados.
        private ThirdPersonCameraFollow _exploreCamera;
        private ShooterCameraFollow _shooterCamera;
        private PlayerController _playerController;
        private WeaponController _weaponController;

        private Vector3 _bossStartPosition;
        private Quaternion _bossStartRotation;

        private void Awake()
        {
            ResolveRuntimeDependencies();

            if (boss != null)
            {
                _bossStartPosition = boss.transform.position;
                _bossStartRotation = boss.transform.rotation;
            }
        }

        private void ResolveRuntimeDependencies()
        {
            _playerController = PlayerController.Instance;
            if (_playerController != null)
            {
                _weaponController = _playerController.GetComponent<WeaponController>();
            }

            _exploreCamera = ThirdPersonCameraFollow.Instance;
            if (_exploreCamera != null)
            {
                _shooterCamera = _exploreCamera.GetComponent<ShooterCameraFollow>();
            }
        }

        private void OnEnable()
        {
            if (boss != null)
            {
                boss.Defeated += HandleBossDefeated;
                boss.PlayerCaught += HandlePlayerCaught;
            }
        }

        private void OnDisable()
        {
            if (boss != null)
            {
                boss.Defeated -= HandleBossDefeated;
                boss.PlayerCaught -= HandlePlayerCaught;
            }
        }

        private void Start()
        {
            StartEncounter();
        }

        private void StartEncounter()
        {
            ResolveRuntimeDependencies();

            if (boss == null)
            {
                return;
            }

            boss.Activate();
            SetShooterModeEnabled(true);
        }

        private void HandleBossDefeated()
        {
            SetShooterModeEnabled(false);
            GameManager.Instance.ReturnToMainMenu();
        }

        /// <summary>
        /// La jefa atrapó al jugador: lo devuelve al FloorSpawnPoint de esta escena
        /// y reinicia el intento desde la posición inicial de la jefa.
        /// </summary>
        private void HandlePlayerCaught()
        {
            SetShooterModeEnabled(false);
            Transform retryPoint = FloorSpawnPoint.Current != null
                ? FloorSpawnPoint.Current.transform
                : transform;
            PlaceCharacterAt(retryPoint);

            boss.ResetEncounter();
            boss.transform.SetPositionAndRotation(_bossStartPosition, _bossStartRotation);

            StartEncounter();
        }

        /// <summary>
        /// ThirdPersonCameraFollow y ShooterCameraFollow comparten la misma InputAction
        /// "Look" del mismo InputActionAsset: cada una la habilita/deshabilita en su
        /// propio OnEnable/OnDisable. Por eso hay que apagar primero la cámara saliente
        /// y recién después prender la entrante — si se hiciera al revés, el Disable()
        /// de la que se apaga se ejecutaría último y dejaría el look muerto para ambas.
        /// </summary>
        private void SetShooterModeEnabled(bool isShooterMode)
        {
            if (isShooterMode)
            {
                if (_exploreCamera != null)
                {
                    _exploreCamera.enabled = false;
                }

                if (_shooterCamera != null)
                {
                    _shooterCamera.enabled = true;
                }
            }
            else
            {
                if (_shooterCamera != null)
                {
                    _shooterCamera.enabled = false;
                }

                if (_exploreCamera != null)
                {
                    _exploreCamera.enabled = true;
                }
            }

            if (_playerController != null)
            {
                _playerController.SetAimMode(isShooterMode);
            }

            if (_weaponController != null)
            {
                _weaponController.enabled = isShooterMode;
            }
        }

        private static void PlaceCharacterAt(Transform spawn)
        {
            PlayerController playerController = PlayerController.Instance;
            if (playerController == null)
            {
                return;
            }

            GameObject player = playerController.gameObject;

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            player.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

            if (controller != null)
            {
                controller.enabled = true;
            }
        }
    }
}
