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
        public static GameManager Instance { get; private set; }

        [Header("Pisos de la torre, en orden de progresión")]
        [SerializeField] private string[] floorSceneNames = { "Piso1", "Piso2", "Piso3" };

        public event Action<GameState> StateChanged;
        public event Action<int> FloorChanged;

        public GameState State { get; private set; } = GameState.MainMenu;
        public int CurrentFloorIndex { get; private set; } = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
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
            SceneLoader.Instance.LoadScene(floorSceneNames[CurrentFloorIndex]);
        }
    }
}
