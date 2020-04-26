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
        public BeatmapType beatmapType;
        public bool allowAutoPlay;
        public bool disableBGA;
        public bool disableMusic;
        public bool enableMirror;
        public bool colorMyNote;
        public TextAsset modeFile;
        [Header("TSystem one-pack object")]
        public GameObject tSystem;
        [Header("Ingame manager objects")]
        public GameObject endText, fullComboText;

        bool isEndAnimated;

        private void Awake()
        {
            isEndAnimated = false;
            StartTSystem();
        }

        public void StartTSystem()
        {
            TSystemConfig.Now = new TSystemConfig()
            {
                noteSpeed = 0.5f,
                gameSync = 0,
                allowSoundEffect = true,
                colorNote = colorMyNote,
                musicVolume = 1,
                effectVolume = 1,
                flickJudgeState = 0
            };

            TSystemStatic.ingamePacket = new IngamePacket()
            {
                beatmap = new BeatmapData() { path = resourceBeatmapPath, type = beatmapType },
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

        private void Update()
        {
            if(IngameBasis.Now.IsEnded && !isEndAnimated)
            {
                if (!TSystemStatic.resultPacket.judgeList.ContainsKey(JudgeType.Nice) &&
                    !TSystemStatic.resultPacket.judgeList.ContainsKey(JudgeType.Bad) &&
                    !TSystemStatic.resultPacket.judgeList.ContainsKey(JudgeType.Miss))
                    fullComboText.SetActive(true);
                endText.SetActive(true);
                isEndAnimated = true;
            }
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