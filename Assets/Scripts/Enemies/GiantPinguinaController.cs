using System;
using UnityEngine;
using PinguiStory.Combat;

namespace PinguiStory.Enemies
{
    /// <summary>
    /// Pinguina gigante: persigue al jugador en línea recta y puede recibir daño
    /// de proyectiles (IDamageable). No conoce GameManager ni el flujo de piso 4:
    /// solo expone eventos para que un controlador de encuentro reaccione.
    /// </summary>
    public class GiantPinguinaController : MonoBehaviour, IDamageable
    {
        [Header("Target")]
        [SerializeField] private string playerTag = "Player";

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float rotationSpeed = 6f;

        [Header("Health")]
        [SerializeField] private int maxHealth = 100;

        [Header("Catch")]
        [SerializeField] private float catchDistance = 1.2f;

        /// <summary>Se dispara una vez cuando la vida llega a 0.</summary>
        public event Action Defeated;

        /// <summary>Se dispara una vez cuando alcanza al jugador.</summary>
        public event Action PlayerCaught;

        private Transform _player;
        private int _currentHealth;
        private bool _isDefeated;
        private bool _hasCaughtPlayer;

        private void Awake()
        {
            _currentHealth = maxHealth;

            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                _player = playerObject.transform;
            }
        }

        private void Update()
        {
            if (_isDefeated || _hasCaughtPlayer || _player == null)
            {
                return;
            }

            ChasePlayer();
            CheckCatch();
        }

        private void ChasePlayer()
        {
            Vector3 toPlayer = _player.position - transform.position;
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Vector3 direction = toPlayer.normalized;

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        private void CheckCatch()
        {
            float distance = Vector3.Distance(transform.position, _player.position);
            if (distance > catchDistance)
            {
                return;
            }

            _hasCaughtPlayer = true;
            PlayerCaught?.Invoke();
        }

        public void TakeDamage(int amount)
        {
            if (_isDefeated)
            {
                return;
            }

            _currentHealth -= amount;
            if (_currentHealth > 0)
            {
                return;
            }

            _isDefeated = true;
            Defeated?.Invoke();
        }
    }
}
