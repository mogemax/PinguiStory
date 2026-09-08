using UnityEngine;

namespace PinguiStory.Combat
{
    /// <summary>
    /// Proyectil simple (bola de nieve): vuela en línea recta, aplica daño al primer
    /// IDamageable que golpea y se destruye al impactar con cualquier cosa o al expirar.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 4f;

        private Rigidbody _rigidbody;
        private int _damage;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Launch(Vector3 direction, float speed, int damage)
        {
            _damage = damage;
            _rigidbody.linearVelocity = direction.normalized * speed;
            Destroy(gameObject, lifeTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            IDamageable damageable = collision.collider.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(_damage);

            Destroy(gameObject);
        }
    }
}
