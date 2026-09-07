using UnityEngine;

namespace PinguiStory.Core
{
    /// <summary>
    /// Trigger que arranca el juego (carga Piso1) cuando el jugador entra a la torre.
    /// Colocar en un Collider con "Is Trigger" activado, en la entrada de la torre.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TowerEntrance : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";

        private bool _triggered;

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered || !other.CompareTag(playerTag))
            {
                return;
            }

            _triggered = true;
            GameManager.Instance.StartGame();
        }
    }
}
