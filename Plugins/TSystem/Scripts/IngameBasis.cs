using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    public class IngameBasis : MonoBehaviour
    {
        /************ Static Property ************/

        public static IngameBasis Now { get; private set; }


        /************ Public Property ************/

        public IngamePacket Packet { get { return TSystemStatic.ingamePacket; } }
        public TSystemMode Mode { get { return Packet.gameMode; } }
        public int ValidNoteCount { get; set; }
        public float Time { get; set; }                 // Main time indicator of the game
        public float FixedTime { get; set; }            // Fixed time indicator; not affected by TimeSpeed
        public float TimeSpeed { get; set; }            // Time multiply coefficient
        public float NoteSpeed { get; set; }            // Note speed multiplier; computed from FixedNoteSpeed
        public float FixedNoteSpeed { get { return TSystemConfig.Now.noteSpeed; } }       // Fixed note speed multiplier
        public float GameSync { get { return TSystemConfig.Now.gameSync; } }
        public bool IsPlaying { get; set; }
        public bool IsStarted { get; set; }
        public bool IsEnded { get; set; }
        public bool ReadyToPlay { get; set; }
        public bool Paused { get; set; }
        public bool IsAutoPlay { get { return Packet.autoPlay; } }
        public bool IsNoteColored { get { return TSystemConfig.Now.colorNote; } }


        /************ Public Field ************/
        [Header("Scene related values")]
        public bool isVertical;
        public float referenceWidth;
        public float referenceHeight;
        [Header("Shared setting for all basis")]
        public int curLineSet;
        public float flickThreshold;
        [Header("Shared object for all basis")]
        public GameObject[] noteTemplate;
        public GameObject tailTemplate, connectorTemplate, multiLineTemplate;
        public Sprite[] noteColorImage, noteWhiteImage;
        public RectTransform noteParent, meshParent;
        public GameObject linePanelTemplate;
        public RectTransform lineParent;
        [Header("Game submodules for all basis")]
        public BeatmapParser beatmapParser;
        public MusicPlayer musicPlayer;
        public BackgroundPlayer backPlayer;
        public JudgeBasis judge;
        public InputGetter input;

        public Dictionary<int, Note> notes;

        public delegate void SpecialLineAnim();
        public SpecialLineAnim specialEnterAnim, specialLeaveAnim;


        /************ Protected Field ************/

        protected int systemNoteIdx = -1;
        protected float maxReachTime;
        protected float songTime;
        protected bool hasError = false;
        protected List<LinePanel> linePanels = new List<LinePanel>();


        protected virtual void Awake()
        {
            Now = this;

            Time = GameSync - FixedNoteSpeed;
            FixedTime = Time;
            TimeSpeed = 1;
            NoteSpeed = FixedNoteSpeed;

            notes = new Dictionary<int, Note>();
        }

        protected virtual IEnumerator Start()
        {
            Mode.SetArguments();

            // Line construction
            for(int i = 0; i < Mode.lineSet.Length; i++)
            {
                var newPanel = Instantiate(linePanelTemplate);
                newPanel.GetComponent<RectTransform>().SetParent(lineParent);
                newPanel.GetComponent<RectTransform>().localScale = Vector3.one;
                newPanel.GetComponent<LinePanel>().Set(i);
                linePanels.Add(newPanel.GetComponent<LinePanel>());
                newPanel.SetActive(true);

                if (i != 0)
                    newPanel.GetComponent<LinePanel>().fullBody.alpha = 0;
            }

            // Beatmap Loading
            bool parseSucceed = false;
            maxReachTime = 0;
            switch(Packet.beatmap.type)
            {
                case BeatmapType.TWx:
                    beatmapParser.ParseStructuredBeatmap(Packet.beatmap.path, Packet.beatmap.type, out parseSucceed);
                    break;
                case BeatmapType.Deleste:
                    beatmapParser.ParseDeleste(Packet.beatmap.path, out parseSucceed);
                    break;
                case BeatmapType.SSTrain:
                    beatmapParser.ParseStructuredBeatmap(Packet.beatmap.path, Packet.beatmap.type, out parseSucceed);
                    break;
            }
            if(!parseSucceed)
            {
                TSystemStatic.LogWarning("Failed to parse beatmap. Game will NOT be played.");
                yield break;
            }
            CreateNote(new NoteData(systemNoteIdx--, 0, maxReachTime + 1, 1, 0, 0, NoteType.Ender, FlickType.NotFlick, Color.clear, new List<int>()));
            ChangeLine(0);
            AfterNoteLoading();

            // Audio Loading
            if (!Packet.noMusic)
            {
                yield return musicPlayer.LoadMusic(Packet.musicPath, Packet.musicType);
                if(!musicPlayer.loaded)
                {
                    TSystemStatic.LogWarning("Failed to load music. Game will be played without music.");
                }
            }

            // BGA/Image Loading
            if(!Packet.noBGA)
            {
                if (Packet.bgaPath != "")
                {
                    backPlayer.LoadBGA(Packet.bgaPath);
                    yield return new WaitUntil(() => backPlayer.loaded || backPlayer.hasBgaError);
                    if(backPlayer.hasBgaError)
                    {
                        TSystemStatic.LogWarning("Failed to load BGA. Game will be played without BGA.");
                    }
                }
                else if(Packet.backImagePath != "")
                {
                    yield return backPlayer.LoadBackImage(Packet.backImagePath);
                    if(!backPlayer.loaded)
                    {
                        TSystemStatic.LogWarning("Failed to load background image. Game will be played without background image.");
                    }
                }
            }

            // Game values initializing
            input.Initialize();
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            ReadyToPlay = true;
        }

        protected virtual void AfterNoteLoading()
        {

        }

        public void StartGame()
        {
            IsStarted = true;
        }

        public void StartPlayMusic()
        {
            songTime = Time;

            backPlayer.Resume();
            musicPlayer.Play();
            IsPlaying = true;
        }

        private void Update()
        {
            if(IsStarted && !IsEnded && !Paused)
            {
                Time += UnityEngine.Time.deltaTime * TimeSpeed;
                FixedTime += UnityEngine.Time.deltaTime;

                var activedNotes = new List<int>();

                foreach (var note in notes)
                {
                    if (note.Value.Progress >= 0 && !note.Value.isAppeared)
                    {
                        note.Value.Wakeup();
                        if((int)note.Value.Type < 10 && note.Value.Type != NoteType.Hidden)
                            activedNotes.Add(note.Key);
                    }
                }

                if(activedNotes.Count > 1)
                {
                    activedNotes.Sort((x, y) => notes[x].EndLine > notes[y].EndLine ? 1 : -1);
                    for(int i = 0; i < activedNotes.Count - 1; i++)
                    {
                        var newObj = Instantiate(multiLineTemplate);
                        newObj.name = "Multiliner between " + activedNotes[i].ToString() + " and " + activedNotes[i + 1].ToString();
                        newObj.transform.SetParent(meshParent);
                        newObj.transform.localScale = Vector3.one;
                        var newMulti = newObj.GetComponent<Multiliner>();
                        newMulti.Set(notes[activedNotes[i]], notes[activedNotes[i + 1]]);
                        newObj.SetActive(true);
                    }
                }
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (IsStarted && !IsEnded && !Paused && pause)
                Pause();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (IsStarted && !IsEnded && !Paused && !focus)
                Pause();
        }

        public void Pause()
        {
            musicPlayer.Pause();
            backPlayer.Pause();
            Paused = true;
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }

        public void Resume()
        {
            musicPlayer.Resume();
            musicPlayer.GetComponent<AudioSource>().time = FixedTime - songTime;
            backPlayer.Resume();
            Paused = false;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        public void EndGame()
        {
            TSystemStatic.resultPacket = new ResultPacket()
            {
                songName = Packet.songName,
                judgeList = judge.ExportJudgeResult(),
                allValidNotes = ValidNoteCount,
                score = judge.score,
                percentage = judge.percentage,
                maxCombo = judge.maxCombo,
                gameMode = Mode,
                beatmap = Packet.beatmap,
                autoPlay = Packet.autoPlay,
                mirror = Packet.mirror
            };
            IsEnded = true;
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }

        public Vector2 GetTouchPos(Vector3 raw)
        {
            Vector3 pos = new Vector3();

            if (isVertical)
            {
                if (Screen.height / (float)Screen.width >= 16f / 9)
                    pos.Set((raw.x - Camera.main.pixelWidth / 2f) / Camera.main.pixelWidth * referenceWidth, (raw.y - Camera.main.pixelHeight / 2f) / Camera.main.pixelHeight * (referenceWidth * ((float)Camera.main.pixelHeight / Camera.main.pixelWidth)), 0);
                else
                    pos.Set((raw.x - Camera.main.pixelWidth / 2f) / Camera.main.pixelWidth * (referenceHeight * ((float)Camera.main.pixelWidth / Camera.main.pixelHeight)), (raw.y - Camera.main.pixelHeight / 2f) / Camera.main.pixelHeight * referenceHeight, 0);
            }
            else
            {
                if (Screen.width / (float)Screen.height >= 16f / 9) 
                    pos.Set((raw.x - (Camera.main.pixelWidth / 2f)) / Camera.main.pixelWidth * (referenceHeight * ((float)Camera.main.pixelWidth / Camera.main.pixelHeight)), (raw.y - (Camera.main.pixelHeight / 2f)) / Camera.main.pixelHeight * referenceHeight, 0);
                else 
                    pos.Set((raw.x - (Camera.main.pixelWidth / 2f)) / Camera.main.pixelWidth * referenceWidth, (raw.y - (Camera.main.pixelHeight / 2f)) / Camera.main.pixelHeight * (referenceWidth * ((float)Camera.main.pixelHeight / Camera.main.pixelWidth)), 0);
            }
            return pos;
        }

        public virtual IEnumerator ChangeLine(int newLineSet)
        {
            var beforeLine = curLineSet;
            curLineSet = newLineSet;

            float t = 0;
            while(t <= 0.25f / TimeSpeed)
            {
                linePanels[beforeLine].fullBody.alpha = 1 - t * 4 * TimeSpeed;
                linePanels[newLineSet].fullBody.alpha = t * 4 * TimeSpeed;
                t += UnityEngine.Time.deltaTime;
                yield return null;
            }
            linePanels[beforeLine].fullBody.alpha = 0;
            linePanels[newLineSet].fullBody.alpha = 1;
        }

        public virtual void TriggerSpecialEnter()
        {
            linePanels[curLineSet].SwitchToSpecial(0.25f / TimeSpeed);
        }

        public virtual void TriggerSpecialLeave()
        {
            linePanels[curLineSet].SwitchToLine(0.25f / TimeSpeed);
        }

        public void PlayHitEffect(int line)
        {
            line = Mathf.Clamp(line, 1, Mode.lineSet[curLineSet].count);
            linePanels[curLineSet][line - 1].PlayHitEffect();
        }

        public virtual GameObject GetNoteTemplate(NoteData data)
        {
            TSystemStatic.Log("GetNoteObject not implemented in this basis.");
            return null;
        }

        public virtual Sprite GetNoteImage(NoteData data)
        {
            TSystemStatic.Log("GetNoteImage not implemented in this basis.");
            return null;
        }

        public virtual void CreateNote(NoteData data)
        {
            TSystemStatic.Log("CreateNote not implemented in this basis.");
        }

        public virtual void AddScore(NoteData data, JudgeType result)
        {
            TSystemStatic.Log("AddScore not implemented in this basis.");
        }

        public virtual void AddHoldingScore()
        {
            TSystemStatic.Log("AddHoldingScore not implemented in this basis.");
        }
    }
}

 