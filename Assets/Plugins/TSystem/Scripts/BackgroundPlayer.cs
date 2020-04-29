using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

namespace TSystem
{
    public class BackgroundPlayer : MonoBehaviour
    {
        public IngameBasis Game { get { return IngameBasis.Now; } }

        public bool loaded;
        public bool hasBgaError;
        public VideoPlayer videoPlayer;
        public GameObject videoScreen;
        public RawImage imageFrame;
        public GameObject defaultBackground;

        private bool isBGA = false;
        
        public void LoadBGA(string path)
        {
            defaultBackground.SetActive(false);
            videoScreen.SetActive(true);
            loaded = false;
            hasBgaError = false;

            if (path == "")
                return;

            videoPlayer.audioOutputMode = TSystemConfig.Now.enableBgaMusic ? VideoAudioOutputMode.Direct : VideoAudioOutputMode.None;

            if(Game.Packet.loadFromResources)
            {
                videoPlayer.source = VideoSource.VideoClip;
                videoPlayer.clip = Resources.Load<VideoClip>(path);
                videoPlayer.prepareCompleted += (p) => { loaded = true; isBGA = true; };
                videoPlayer.Prepare();
            }
            else
            {
                try
                {
                    videoPlayer.source = VideoSource.Url;
                    videoPlayer.url = path;
                    videoPlayer.prepareCompleted += (p) => { loaded = true; isBGA = true; };
                    videoPlayer.Prepare();
                }
                catch (System.Exception e)
                {
                    TSystemStatic.LogWithException("Error occured while loading BGA.", e);
                    hasBgaError = true;
                }
            }
        }

        public IEnumerator LoadBackImage(string path)
        {
            defaultBackground.SetActive(false);
            
            if(Game.Packet.loadFromResources)
            {
                imageFrame.texture = Resources.Load<Texture2D>(path);
                imageFrame.gameObject.SetActive(true);
                loaded = true;
            }
            else
            {
                var request = UnityWebRequestTexture.GetTexture(path);
                yield return request.SendWebRequest();
                if(request.isNetworkError || request.isHttpError)
                    TSystemStatic.LogWarning("Error occured while loading background image.");
                else
                {
                    imageFrame.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    imageFrame.gameObject.SetActive(true);
                    loaded = true;
                }
            }
        }

        public void Pause()
        {
            if (isBGA)
                videoPlayer.Pause();
        }

        public void Resume()
        {
            if (isBGA)
                videoPlayer.Play();
        }

        public void SetStartPlaytime(float time)
        {
            if (isBGA)
                videoPlayer.frame = Mathf.RoundToInt(Mathf.Max(time, 0) * videoPlayer.frameRate);
        }
    }
}