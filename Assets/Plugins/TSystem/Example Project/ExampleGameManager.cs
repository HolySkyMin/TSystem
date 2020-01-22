using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace TSystem.Example
{
    public class ExampleGameManager : MonoBehaviour
    {
        [Header("Beatmap info and environments")]
        public string resourceBeatmapPath;
        public string resourceMusicPath;
        public string resourceBGAPath;
        public string resourceBackImagePath;
        public bool allowAutoPlay;
        public bool disableBGA;
        public bool disableMusic;
        public bool enableMirror;
        public bool colorMyNote;
        public TextAsset modeFile;

        public GameObject tSystem;

        private void Start()
        {
            StartTSystem();
        }

        public void StartTSystem()
        {
            TSystemConfig.Now = new TSystemConfig()
            {
                noteSpeed = 1,
                gameSync = 0,
                allowSoundEffect = true,
                colorNote = colorMyNote,
                musicVolume = 1,
                effectVolume = 1,
                flickJudgeState = 0
            };

            TSystemStatic.ingamePacket = new IngamePacket()
            {
                beatmap = new BeatmapData() { path = resourceBeatmapPath },
                musicPath = resourceMusicPath,
                bgaPath = resourceBGAPath,
                backImagePath = resourceBackImagePath,
                autoPlay = allowAutoPlay,
                noBGA = disableBGA,
                noMusic = disableMusic,
                mirror = enableMirror,
                loadFromResources = true,
                gameMode = JsonMapper.ToObject<TSystemMode>(modeFile.text)
            };

            Debug.Log(TSystemStatic.ingamePacket.gameMode.judgeThreshold.Length);
            TSystemStatic.ingamePacket.gameMode.SetArguments();
            tSystem.SetActive(true);
        }

        public void PauseGame()
        {
            IngameBasis.Now.Pause();
        }

        public void ResumeGame()
        {
            IngameBasis.Now.Resume();
        }
    }
}