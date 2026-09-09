using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PinguiStory.Collectibles;

namespace PinguiStory.UI
{
    /// <summary>
    /// HUD de piedras recolectadas en el piso actual.
    /// El total por piso es fijo (no se auto-cuenta desde la escena).
    /// </summary>
    public class StoneCounterUI : MonoBehaviour
    {
        private const int TotalStonesPerFloor = 5;

        [SerializeField] private Image[] circleSlots;
        [SerializeField] private Sprite[] stoneSprites;
        [SerializeField] private Color collectedColor = Color.white;
        [SerializeField] private Color pendingColor = new Color(1f, 1f, 1f, 0.28f);

        private Coroutine _waitForManagerRoutine;
        private bool _subscribed;

        private void OnEnable()
        {
            _waitForManagerRoutine = StartCoroutine(WaitForStoneManagerAndSubscribe());
        }

        private void OnDisable()
        {
            if (_waitForManagerRoutine != null)
            {
                StopCoroutine(_waitForManagerRoutine);
                _waitForManagerRoutine = null;
            }

            Unsubscribe();
        }

        /// <summary>
        /// StoneManager persiste entre escenas vía DontDestroyOnLoad, por lo que su Awake()
        /// puede correr antes o después que este OnEnable() según el orden de la jerarquía.
        /// Se espera activamente a que Instance exista para no depender de ese orden.
        /// </summary>
        private IEnumerator WaitForStoneManagerAndSubscribe()
        {
            while (StoneManager.Instance == null)
            {
                yield return null;
            }

            StoneManager.Instance.StoneCollected += HandleStoneCollected;
            StoneManager.Instance.FloorCountReset += HandleFloorCountReset;
            _subscribed = true;

            Refresh(StoneManager.Instance.FloorCount);
        }

        private void Unsubscribe()
        {
            if (!_subscribed || StoneManager.Instance == null)
            {
                return;
            }

            StoneManager.Instance.StoneCollected -= HandleStoneCollected;
            StoneManager.Instance.FloorCountReset -= HandleFloorCountReset;
            _subscribed = false;
        }

        private void HandleStoneCollected(int floorCount, int totalCount)
        {
            Refresh(floorCount);
        }

        private void HandleFloorCountReset()
        {
            Refresh(0);
        }

        private void Refresh(int floorCount)
        {
            for (int i = 0; i < circleSlots.Length; i++)
            {
                Image slot = circleSlots[i];
                if (slot == null)
                {
                    continue;
                }

                bool isFilled = i < floorCount && i < TotalStonesPerFloor;
                slot.sprite = GetStoneSprite(i);
                slot.color = isFilled ? collectedColor : pendingColor;
            }
        }

        private Sprite GetStoneSprite(int slotIndex)
        {
            if (stoneSprites == null || stoneSprites.Length == 0)
            {
                return null;
            }

            return stoneSprites[slotIndex % stoneSprites.Length];
        }
    }
}
