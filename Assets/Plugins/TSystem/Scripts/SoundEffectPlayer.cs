using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundEffectPlayer : MonoBehaviour
    {
        public AudioClip goodHit, badHit, flickHit;

        public void PlayHitSound(bool good, bool flick)
        {
            var source = GetComponent<AudioSource>();
            if (flick)
                source.PlayOneShot(flickHit, TSystemConfig.Now.effectVolume);
            else if (good)
                source.PlayOneShot(goodHit, TSystemConfig.Now.effectVolume);
            else
                source.PlayOneShot(badHit, TSystemConfig.Now.effectVolume);
        }
    }
}