using UnityEngine;

namespace PinguiStory.Core
{
    /// <summary>
    /// Salida final para una puerta o zona de llegada. Al entrar el jugador,
    /// termina la partida y vuelve al menú principal con una sesión limpia.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MainMenuExit : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";

        private bool _triggered;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered || !other.CompareTag(playerTag))
            {
                return;
            }

            _triggered = true;
            GameManager.Instance.ReturnToMainMenu();
        }
    }
}
