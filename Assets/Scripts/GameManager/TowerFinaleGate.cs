using UnityEngine;
using PinguiStory.Collectibles;

namespace PinguiStory.Core
{
    /// <summary>
    /// Salida final de Piso4: en vez de avanzar al siguiente piso, decide entre
    /// el final bueno o la sala del jefe según el total de piedras recolectadas
    /// en toda la torre.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TowerFinaleGate : MonoBehaviour
    {
        [Header("Condición del final bueno")]
        [SerializeField] private int totalStonesRequiredForGoodEnding = 20;

        [Header("Escenas de destino")]
        [SerializeField] private string goodEndingSceneName = "FinalBueno";
        [SerializeField] private string badEndingSceneName = "FinalMalo";

        private bool _triggered;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered || !other.CompareTag("Player"))
            {
                return;
            }

            _triggered = true;

            int totalStones = StoneManager.Instance != null ? StoneManager.Instance.TotalCount : 0;
            string destinationScene = totalStones >= totalStonesRequiredForGoodEnding
                ? goodEndingSceneName
                : badEndingSceneName;

            SceneLoader.Instance.LoadScene(destinationScene);
        }
    }
}
