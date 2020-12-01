using UnityEngine;

namespace Reese.Demo.Stranded
{
    public class PlayerSoundController : MonoBehaviour
    {
        [SerializeField]
        AudioClip[] grunts = default;

        [SerializeField]
        AudioSource walkSoundSource = default;

        [SerializeField]
        AudioSource jumpSoundSource = default;

        public void Walk()
        {
            walkSoundSource.time = UnityEngine.Random.Range(0, walkSoundSource.clip.length);
            walkSoundSource.Play();
        }

        public void StopWalking()
            => walkSoundSource.Stop();

        public bool IsWalking()
            => walkSoundSource.isPlaying;

        public void Jump()
        {
            if (grunts.Length <= 0) return;

            jumpSoundSource.clip = grunts[UnityEngine.Random.Range(0, grunts.Length)];
            jumpSoundSource.Play();
        }
    }
}
