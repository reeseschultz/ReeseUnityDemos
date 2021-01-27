using System.Collections;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reese.Demo.Stranded
{
    public class PlayerDeathSequenceController : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup backgroundGroup = default;

        [SerializeField]
        CanvasGroup textGroup = default;

        [SerializeField]
        TextMeshProUGUI text = default;

        [SerializeField]
        AudioSource deathAudioSource = default;

        float runStartSeconds = default;

        float originalTextSize = default;

        public void Run()
        {
            runStartSeconds = Time.time;

            deathAudioSource.Play();

            text.enableAutoSizing = false;

            originalTextSize = text.fontSize;

            var maxTextSize1 = originalTextSize + originalTextSize * 0.2f;
            var maxTextSize2 = maxTextSize1 + maxTextSize1 * 0.2f;

            StartCoroutine(Fade(0.5f, backgroundGroup, 0.8f, 1));
            StartCoroutine(Fade(2, textGroup, 0.8f, 1));
            StartCoroutine(Fade(5, textGroup, 0.8f, 0, false));
            StartCoroutine(Scale(2, text, 20, maxTextSize1));
            StartCoroutine(Scale(4.5f, text, 20, maxTextSize2));
            StartCoroutine(ReloadScene());
        }

        IEnumerator Fade(float delay, CanvasGroup canvasGroup, float speed, float minOrMax, bool lessThan = true)
        {
            yield return new WaitForSeconds(delay);

            while (lessThan ? canvasGroup.alpha < minOrMax : canvasGroup.alpha > minOrMax)
            {
                var change = speed * Time.deltaTime;

                if (lessThan) canvasGroup.alpha += change;
                else canvasGroup.alpha -= change;

                yield return null;
            }
        }

        IEnumerator Scale(float delay, TextMeshProUGUI text, float speed, float minOrMax, bool lessThan = true)
        {
            yield return new WaitForSeconds(delay);

            while (lessThan ? text.fontSize < minOrMax : text.fontSize > minOrMax)
            {
                var change = speed * Time.deltaTime;

                if (lessThan) text.fontSize += change;
                else text.fontSize -= change;

                yield return null;
            }
        }

        IEnumerator ReloadScene()
        {
            yield return new WaitForSeconds(9f);

            World.DisposeAllWorlds();
            DefaultWorldInitialization.Initialize("Default World", false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }
    }
}
