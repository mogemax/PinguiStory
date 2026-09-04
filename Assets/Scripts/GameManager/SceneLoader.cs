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
        public static SceneLoader Instance { get; private set; }

        public event Action LoadStarted;
        public event Action<float> LoadProgressChanged;
        public event Action LoadCompleted;

        public bool IsLoading { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadScene(string sceneName)
        {
            if (IsLoading)
            {
                return;
            }

            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            IsLoading = true;
            LoadStarted?.Invoke();

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

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
