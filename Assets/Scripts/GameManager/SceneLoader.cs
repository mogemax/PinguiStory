using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PinguiStory.Core
{
    /// <summary>
    /// Encapsula la carga asíncrona de escenas. Persiste entre escenas (DontDestroyOnLoad)
    /// para poder disparar cargas desde cualquier piso sin buscar referencias en la escena.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader _instance;
        public static SceneLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<SceneLoader>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SceneLoader");
                        _instance = go.AddComponent<SceneLoader>();
                    }
                }
                return _instance;
            }
        }

        public event Action LoadStarted;
        public event Action<float> LoadProgressChanged;
        public event Action LoadCompleted;

        public bool IsLoading { get; private set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadScene(string sceneName)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"[SceneLoader] Ya hay una carga en progreso. Se ignoró la solicitud de: {sceneName}");
                return;
            }

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneLoader] No se proporcionó un nombre de escena válido.");
                return;
            }

            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            IsLoading = true;
            LoadStarted?.Invoke();

            AsyncOperation operation = null;

            try
            {
                operation = SceneManager.LoadSceneAsync(sceneName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneLoader] Error al intentar cargar la escena '{sceneName}': {ex.Message}");
            }

            if (operation == null)
            {
                Debug.LogError($"[SceneLoader] No se pudo iniciar la carga de '{sceneName}'. Revisa si la escena existe y está agregada en Build Settings.");
                IsLoading = false;
                yield break;
            }

            while (!operation.isDone)
            {
                LoadProgressChanged?.Invoke(operation.progress);
                yield return null;
            }

            IsLoading = false;
            LoadCompleted?.Invoke();
        }
    }
}
