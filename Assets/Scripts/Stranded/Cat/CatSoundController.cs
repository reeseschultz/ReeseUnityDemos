using UnityEngine;

namespace Reese.Demo.Stranded
{
    [RequireComponent(typeof(AudioSource))]
    public class CatSoundController : MonoBehaviour
    {
        [SerializeField]
        AudioClip[] meows = default;

        AudioSource source = default;

        void Start()
            => source = GetComponent<AudioSource>();

        public void Meow()
        {
            if (meows.Length <= 0) return;

            source.clip = meows[UnityEngine.Random.Range(0, meows.Length)];
            source.Play();
        }
    }
}
