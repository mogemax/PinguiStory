using UnityEngine;

namespace PinguiStory.Core
{
    /// <summary>
    /// Trigger en la puerta de la torre del menú principal. Al entrar el jugador
    /// (tag "Player"), arranca la progresión de niveles llamando a GameManager.StartGame(),
    /// lo que carga el Piso1.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TowerEntrance : MonoBehaviour
    {
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

            if (GameManager.Instance == null)
            {
                Debug.LogWarning("TowerEntrance: no hay un GameManager en la escena.");
                return;
            }

            _triggered = true;
            GameManager.Instance.StartGame();
        }
    }
}