using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

//This script is only used to start the Suntail demo scene
namespace Suntail
{
    public class SuntailStartDemo : MonoBehaviour
    {
        [SerializeField]
        private AudioMixer _audioMixer;

        [SerializeField]
        private Image blackScreenImage;

        [SerializeField]
        private Text blackScreenText1;

        [SerializeField]
        private Text blackScreenText2;

        [SerializeField]
        private float blackScreenDuration = 3f;

        [SerializeField]
        private float fadingDuration = 2f;

        private void Start()
        {
            blackScreenImage.gameObject.SetActive(true);
            blackScreenText1.gameObject.SetActive(true);
            blackScreenText2.gameObject.SetActive(true);
            _audioMixer.SetFloat("soundsVolume", -80f);

            StartCoroutine(ShowBlackScreen());
        }

        private IEnumerator ShowBlackScreen()
        {
            yield return new WaitForSeconds(blackScreenDuration);
            blackScreenImage.raycastTarget = false;
            blackScreenImage.canvasRenderer.cullTransparentMesh = true;
            blackScreenImage.CrossFadeAlpha(0, fadingDuration, false);

            blackScreenText1.CrossFadeAlpha(0, fadingDuration, false);
            blackScreenText2.CrossFadeAlpha(0, fadingDuration, false);
            StartCoroutine(StartAudioFade(_audioMixer, "soundsVolume", fadingDuration, 1f));
        }

        //Sound fading
        public static IEnumerator StartAudioFade(AudioMixer audioMixer, string exposedParam, float duration, float targetVolume)
        {
            audioMixer.GetFloat(exposedParam, out float currentVol);
            currentVol = Mathf.Pow(10, currentVol / 20);
            float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);

            float currentTime = 0;
            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
                audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
                yield return null;
            }
        }
    }
}