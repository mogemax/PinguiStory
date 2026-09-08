using UnityEngine;
using UnityEngine.InputSystem;

namespace PinguiStory.Combat
{
    /// <summary>
    /// Dispara proyectiles simples (Projectile) desde un punto del arma equipada,
    /// usando la acción "Attack" del Input System. Sin lógica de puntería: dispara
    /// hacia adelante desde "Muzzle Point".
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string attackActionName = "Attack";

        [Header("Weapon")]
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float fireCooldown = 0.25f;

        private InputAction _attackAction;
        private float _nextFireTime;

        private void Awake()
        {
            if (muzzlePoint == null)
            {
                muzzlePoint = transform;
            }

            InputActionMap map = inputActions.FindActionMap(actionMapName, throwIfNotFound: true);
            _attackAction = map.FindAction(attackActionName, throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _attackAction.Enable();
            _attackAction.performed += OnAttackPerformed;
        }

        private void OnDisable()
        {
            _attackAction.performed -= OnAttackPerformed;
            _attackAction.Disable();
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (Time.time < _nextFireTime)
            {
                return;
            }

            _nextFireTime = Time.time + fireCooldown;
            Fire();
        }

        private void Fire()
        {
            Projectile projectile = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
            projectile.Launch(muzzlePoint.forward, projectileSpeed, damage);
        }
    }
}
