using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace TSystem
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        public IngameBasis Game { get { return IngameBasis.Now; } }

        public bool loaded;

        public IEnumerator LoadMusic(string file, AudioType type)
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
                var request = UnityWebRequestMultimedia.GetAudioClip(file, type);
                yield return request.SendWebRequest();
                if(request.isNetworkError || request.isHttpError)
                    TSystemStatic.LogWarning("Error occured while loading music.");
                else
                {
                    GetComponent<AudioSource>().clip = ((DownloadHandlerAudioClip)request.downloadHandler).audioClip;
                    GetComponent<AudioSource>().volume = TSystemConfig.Now.musicVolume;

                    loaded = true;
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