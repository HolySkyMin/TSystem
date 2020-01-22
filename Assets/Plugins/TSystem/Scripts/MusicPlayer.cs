using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TSystem
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        public IngameBasis Game { get { return IngameBasis.Now; } }

        public bool loaded;

        public IEnumerator LoadMusic(string file)
        {
            loaded = false;
            if(Game.Packet.loadFromResources)
            {
                GetComponent<AudioSource>().clip = Resources.Load<AudioClip>(file);
                GetComponent<AudioSource>().volume = TSystemConfig.Now.musicVolume;

                loaded = true;
            }
            else
            {
                WWW www = null;

                if (File.Exists(file))
                {
                    www = new WWW(file);
                    yield return www;

                    try
                    {
                        GetComponent<AudioSource>().clip = www.GetAudioClip(false, false);
                        GetComponent<AudioSource>().volume = TSystemConfig.Now.musicVolume;

                        loaded = true;
                    }
                    catch (Exception e)
                    {
                        TSystemStatic.LogWithException("Error occured while loading music.", e);
                    }
                }
            }
        }

        public void Play()
        {
            GetComponent<AudioSource>().Play();
        }

        public void Pause()
        {
            GetComponent<AudioSource>().Pause();
        }

        public void Resume()
        {
            GetComponent<AudioSource>().UnPause();
        }
    }
}