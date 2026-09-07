using UnityEngine;

namespace PinguiStory.Core
{
    /// <summary>
    /// Marca el punto de aparición del jugador en la escena de un piso.
    /// GameManager reposiciona ahí al jugador persistente después de cada carga de escena.
    /// </summary>
    public class FloorSpawnPoint : MonoBehaviour
    {
        public static FloorSpawnPoint Current { get; private set; }

        private void Awake()
        {
            Current = this;
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }
    }
}
