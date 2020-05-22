using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    public class SoundEffectPlayer : MonoBehaviour
    {
        public AudioSource goodHit, badHit, flickHit;

        private void Start()
        {
            goodHit.volume = TSystemConfig.Now.effectVolume;
            badHit.volume = TSystemConfig.Now.effectVolume;
            flickHit.volume = TSystemConfig.Now.effectVolume;
        }

        public void PlayHitSound(bool good, bool flick)
        {
            if (!good)
                badHit.Play();
            else if (flick)
                flickHit.Play();
            else
                goodHit.Play();
        }
    }
}