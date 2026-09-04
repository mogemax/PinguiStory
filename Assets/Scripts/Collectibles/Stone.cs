using UnityEngine;

namespace PinguiStory.Collectibles
{
    /// <summary>
    /// Piedra recolectable individual. Requiere un Collider en modo Trigger
    /// y que el jugador tenga el tag "Player".
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Stone : MonoBehaviour
    {
        [SerializeField] private int value = 1;
        [SerializeField] private GameObject collectVfx;
        [SerializeField] private AudioClip collectSfx;

        private bool _collected;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_collected || !other.CompareTag("Player"))
            {
                return;
            }

            _collected = true;
            Collect();
        }

        private void Collect()
        {
            StoneManager.Instance.RegisterStone(value);

            if (collectVfx != null)
            {
                Instantiate(collectVfx, transform.position, Quaternion.identity);
            }

            if (collectSfx != null)
            {
                AudioSource.PlayClipAtPoint(collectSfx, transform.position);
            }

            gameObject.SetActive(false);
        }
    }
}
