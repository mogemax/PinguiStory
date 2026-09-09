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
        [SerializeField] private Vector3 fallbackMuzzleLocalPosition = new Vector3(0.25f, 0.9f, 0.8f);
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float fireCooldown = 0.25f;

        [Header("Visual")]
        [SerializeField] private GameObject weaponVisualPrefab;
        [SerializeField] private Vector3 weaponVisualLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 weaponVisualLocalEulerAngles = Vector3.zero;

        private InputAction _attackAction;
        private float _nextFireTime;
        private GameObject _weaponVisualInstance;

        private void Awake()
        {
            if (muzzlePoint == null)
            {
                GameObject fallbackMuzzle = new GameObject("Runtime Muzzle Point");
                muzzlePoint = fallbackMuzzle.transform;
                muzzlePoint.SetParent(transform, false);
                muzzlePoint.localPosition = fallbackMuzzleLocalPosition;
            }

            SpawnWeaponVisual();

            InputActionMap map = inputActions.FindActionMap(actionMapName, throwIfNotFound: true);
            _attackAction = map.FindAction(attackActionName, throwIfNotFound: true);
        }

        /// <summary>
        /// Instancia el modelo visual del arma (ej. AK74M) como hijo de "Muzzle Point".
        /// Puramente estético: no afecta la dirección ni el punto de disparo del proyectil.
        /// </summary>
        private void SpawnWeaponVisual()
        {
            if (weaponVisualPrefab == null)
            {
                return;
            }

            _weaponVisualInstance = Instantiate(weaponVisualPrefab, muzzlePoint);
            _weaponVisualInstance.transform.localPosition = weaponVisualLocalPosition;
            _weaponVisualInstance.transform.localRotation = Quaternion.Euler(weaponVisualLocalEulerAngles);
            _weaponVisualInstance.SetActive(false);
        }

        private void OnEnable()
        {
            _attackAction.Enable();
            _attackAction.performed += OnAttackPerformed;

            if (_weaponVisualInstance != null)
            {
                _weaponVisualInstance.SetActive(true);
            }
        }

        private void OnDisable()
        {
            _attackAction.performed -= OnAttackPerformed;
            _attackAction.Disable();

            if (_weaponVisualInstance != null)
            {
                _weaponVisualInstance.SetActive(false);
            }
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
