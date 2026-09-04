using System;
using UnityEngine;

namespace PinguiStory.Collectibles
{
    /// <summary>
    /// Fuente única de verdad para el conteo de piedras. Persiste entre escenas (pisos)
    /// para que el total se mantenga a lo largo de toda la torre.
    /// </summary>
    public class StoneManager : MonoBehaviour
    {
        public static StoneManager Instance { get; private set; }

        /// <summary>Se dispara al recolectar una piedra: (piedras del piso actual, piedras totales).</summary>
        public event Action<int, int> StoneCollected;

        /// <summary>Se dispara cuando se reinicia el conteo del piso (al cambiar de escena).</summary>
        public event Action FloorCountReset;

        public int FloorCount { get; private set; }
        public int TotalCount { get; private set; }

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

        public void RegisterStone(int amount)
        {
            FloorCount += amount;
            TotalCount += amount;
            StoneCollected?.Invoke(FloorCount, TotalCount);
        }

        public void ResetFloorCount()
        {
            FloorCount = 0;
            FloorCountReset?.Invoke();
        }
    }
}
