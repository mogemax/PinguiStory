using UnityEngine;
using PinguiStory.Collectibles;

namespace PinguiStory.Core
{
    /// <summary>
    /// Trigger de salida de piso (escalera/puerta). Opcionalmente exige un mínimo
    /// de piedras recolectadas antes de dejar avanzar al siguiente piso.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FloorExit : MonoBehaviour
    {
        [SerializeField] private int minimumStonesRequired = 0;
        [SerializeField] private bool useFloorStoneCount = true;
        [SerializeField] private bool requiresBossDefeat;

        private bool _triggered;
        private bool _bossDefeated;

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

            if (!HasEnoughStones())
            {
                return;
            }

            _triggered = true;
            GameManager.Instance.AdvanceToNextFloor();
        }

        private bool HasEnoughStones()
        {
            if (requiresBossDefeat && !_bossDefeated)
            {
                return false;
            }

            if (minimumStonesRequired <= 0 || StoneManager.Instance == null)
            {
                return true;
            }

            int collected = useFloorStoneCount ? StoneManager.Instance.FloorCount : StoneManager.Instance.TotalCount;
            return collected >= minimumStonesRequired;
        }

        /// <summary>Permite usar esta salida cuando el encuentro local ya fue superado.</summary>
        public void UnlockAfterBossDefeat()
        {
            _bossDefeated = true;
        }
    }
}
