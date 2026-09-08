using System;
using UnityEngine;
using PinguiStory.Collectibles;

namespace PinguiStory.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Victory
    }

    /// <summary>
    /// Fuente única de verdad del progreso global: piso actual y estado del juego.
    /// Persiste entre escenas (DontDestroyOnLoad) y coordina a SceneLoader/StoneManager.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Pisos de la torre, en orden de progresión")]
        [SerializeField] private string[] floorSceneNames = { "Piso1", "Piso2", "Piso3" };

        public event Action<GameState> StateChanged;
        public event Action<int> FloorChanged;

        public GameState State { get; private set; } = GameState.MainMenu;
        public int CurrentFloorIndex { get; private set; } = -1;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadCompleted -= RepositionPlayerAtSpawn;
                SceneLoader.Instance.LoadCompleted += RepositionPlayerAtSpawn;
            }
        }

        private void OnDisable()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadCompleted -= RepositionPlayerAtSpawn;
            }
        }

        private void RepositionPlayerAtSpawn()
        {
            if (FloorSpawnPoint.Current == null)
            {
                return;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }

            Transform spawn = FloorSpawnPoint.Current.transform;
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

        public void StartGame()
        {
            CurrentFloorIndex = 0;
            SetState(GameState.Playing);
            LoadCurrentFloor();
        }

        public void AdvanceToNextFloor()
        {
            int nextIndex = CurrentFloorIndex + 1;

            if (nextIndex >= floorSceneNames.Length)
            {
                SetState(GameState.Victory);
                return;
            }

            CurrentFloorIndex = nextIndex;
            FloorChanged?.Invoke(CurrentFloorIndex);
            LoadCurrentFloor();

            if (StoneManager.Instance != null)
            {
                StoneManager.Instance.ResetFloorCount();
            }
        }

        public void SetState(GameState newState)
        {
            State = newState;
            StateChanged?.Invoke(State);
        }

        private void LoadCurrentFloor()
        {
            if (floorSceneNames == null || floorSceneNames.Length == 0)
            {
                Debug.LogError("[GameManager] No se han configurado los nombres de escena en 'floorSceneNames'.");
                return;
            }

            if (CurrentFloorIndex < 0 || CurrentFloorIndex >= floorSceneNames.Length)
            {
                Debug.LogError($"[GameManager] Índice de piso fuera de rango: {CurrentFloorIndex}");
                return;
            }

            string sceneToLoad = floorSceneNames[CurrentFloorIndex];

            if (string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogError($"[GameManager] El nombre de la escena en el índice {CurrentFloorIndex} está vacío.");
                return;
            }

            SceneLoader.Instance.LoadScene(sceneToLoad);
        }
    }
}