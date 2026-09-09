using System.Collections;
using UnityEngine;

namespace PinguiStory.Core
{
    /// <summary>
    /// Controla el cierre de partida en una escena de final: tras un tiempo fijo
    /// mostrando la cinemática, vuelve al menú principal.
    /// </summary>
    public class EndingSceneController : MonoBehaviour
    {
        [SerializeField] private float delayBeforeReturningToMenu = 5f;
        [SerializeField] private string mainMenuSceneName = "Menu&Inicio";

        private void Start()
        {
            StartCoroutine(ReturnToMenuAfterDelay());
        }

        private IEnumerator ReturnToMenuAfterDelay()
        {
            yield return new WaitForSeconds(delayBeforeReturningToMenu);
            SceneLoader.Instance.LoadScene(mainMenuSceneName);
        }
    }
}
