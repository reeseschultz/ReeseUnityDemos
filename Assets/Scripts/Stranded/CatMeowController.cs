using UnityEngine;

namespace Reese.Demo
{
    [RequireComponent(typeof(AudioSource))]
    public class CatMeowController : MonoBehaviour
    {
        [SerializeField]
        AudioClip[] clips = default;

        AudioSource source = default;

        void Start()
            => source = GetComponent<AudioSource>();

        public void Meow()
        {
            if (clips.Length <= 0) return;

            source.clip = clips[UnityEngine.Random.Range(0, clips.Length)];
            source.Play();
        }
    }
}
