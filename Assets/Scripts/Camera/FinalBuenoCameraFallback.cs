using UnityEngine;
using PinguiStory.CameraSystem;

namespace PinguiStory.UI
{
    /// <summary>
    /// Permite abrir FinalBueno directamente en el editor sin depender de la
    /// cámara persistente del jugador. En una partida normal, la cámara de
    /// exploración ya existe y este respaldo se apaga para evitar duplicados.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(AudioListener))]
    public class FinalBuenoCameraFallback : MonoBehaviour
    {
        private void Awake()
        {
            if (ThirdPersonCameraFollow.Instance == null)
            {
                return;
            }

            Camera fallbackCamera = GetComponent<Camera>();
            AudioListener fallbackAudioListener = GetComponent<AudioListener>();

            fallbackCamera.enabled = false;
            fallbackAudioListener.enabled = false;
        }
    }
}
