using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TSystem
{
    public class Note : MonoBehaviour
    {
        public IngameBasis Game { get { return IngameBasis.Now; } }

        public int ID { get { return data.id; } }
        public int Size { get { return data.size; } }
        public float ReachTime { get { return data.time; } }
        public float Speed { get { return data.speed; } }
        public float StartLine { get { return data.startLine; } }
        public float EndLine { get { return data.endLine; } }
        public NoteType Type { get { return data.type; } }
        public FlickType Flick { get { return data.flick; } }
        public Color32 ColorKey { get { return data.color; } }

        public float Progress { get { return (Game.Time - appearTime) / (ReachTime - appearTime); } } // Value between 0 ~ 1++
        public float TimeDistance { get { return (Game.Time - ReachTime) / Game.TimeSpeed; } }
        public Vector2 Position { get { return Game.Mode.GetCurrentPos(Progress, startPos, endPos); } }
        public RectTransform Body { get { return GetComponent<RectTransform>(); } }

        [HideInInspector] public int slideGroupFinger;
        [HideInInspector] public int lineSet;
        [HideInInspector] public float appearTime;
        [HideInInspector] public float hitTime;
        [HideInInspector] public float halfJudgeWidth;
        [HideInInspector] public float halfTailWidth;
        [HideInInspector] public bool isAppeared;
        [HideInInspector] public bool isDead;
        [HideInInspector] public bool isHit;
        [HideInInspector] public Vector2 startPos;
        [HideInInspector] public Vector2 endPos;
        [HideInInspector] public Note nextNote;
        [HideInInspector] public Tail nextTail;
        [HideInInspector] public Connector nextConnector;
        [HideInInspector] public List<Note> previousNotes;
        [HideInInspector] public List<Tail> previousTails;
        [HideInInspector] public List<Connector> previousConnectors;

        public Image image;
        public Animator animator;

        protected bool slideTransfered;
        protected NoteData data;
        protected JudgeType latestSlideJudge;

        public void Set(NoteData d, int ls = 0)
        {
            data = d;

            previousNotes = new List<Note>();
            previousTails = new List<Tail>();
            previousConnectors = new List<Connector>();

            appearTime = ReachTime - (1f / (Speed * Game.NoteSpeed));
            for (int i = 0; i < data.prevIds.Count; i++)
            {
                if (data.prevIds[i] > 0 && Type != NoteType.SlideDummy)
                {
                    previousNotes.Add(Game.notes[data.prevIds[i]]);
                    Game.notes[data.prevIds[i]].nextNote = this;
                }
            }

            lineSet = ls;
            hitTime = ReachTime;
            latestSlideJudge = JudgeType.NotJudged;

            startPos = Game.Mode.GetStartPos(lineSet, StartLine);
            endPos = Game.Mode.GetEndPos(lineSet, EndLine);
        }

        public void CreateTail()
        {
            foreach (var prev in previousNotes)
            {
                if (Type.IsEither(NoteType.HoldStart, NoteType.HoldEnd) && prev.Type == NoteType.HoldStart) // Hold tail
                {
                    if (prev.nextTail != null)
                    {
                        TSystemStatic.Log($"Note {prev.ID} seems to have multiple hold or slide bodys. Ignoring tail connection with note {ID}.");
                        data.type = NoteType.Tap;
                        continue;
                    }
                    var newObj = Instantiate(Game.tailTemplate);
                    newObj.transform.SetParent(Game.meshParent);
                    newObj.transform.localScale = Vector3.one;
                    newObj.name = "Tail " + prev.ID.ToString();
                    var newTail = newObj.GetComponent<Tail>();
                    newTail.Set(prev, this, false);
                    prev.nextTail = newTail;
                    previousTails.Add(newTail);

                    data.type = NoteType.HoldEnd;
                }
                else if (Type.IsEither(NoteType.SlideStart, NoteType.SlideMiddle, NoteType.SlideEnd) && prev.Type.IsEither(NoteType.SlideStart, NoteType.SlideMiddle)) // Slide tail
                {
                    if (prev.nextTail != null)
                    {
                        TSystemStatic.Log($"Note {prev.ID} seems to have multiple hold or slide bodys. Ignoring tail connection with note {ID}.");
                        if (nextNote != null && nextNote.Type.IsEither(NoteType.SlideStart, NoteType.SlideMiddle, NoteType.SlideEnd))
                            data.type = NoteType.SlideStart;
                        else
                            data.type = NoteType.Tap;
                        continue;
                    }
                    var newObj = Instantiate(Game.tailTemplate);
                    newObj.transform.SetParent(Game.meshParent);
                    newObj.transform.localScale = Vector3.one;
                    newObj.name = "Tail " + prev.ID.ToString();
                    var newTail = newObj.GetComponent<Tail>();
                    newTail.Set(prev, this, true);
                    prev.nextTail = newTail;
                    previousTails.Add(newTail);

                    if (nextNote != null && nextNote.Type.IsEither(NoteType.SlideStart, NoteType.SlideMiddle, NoteType.SlideEnd))
                        data.type = NoteType.SlideMiddle;
                    else
                        data.type = NoteType.SlideEnd;
                }
            }
        }

        public void CreateConnector()
        {
            foreach (var prev in previousNotes)
            {
                if (Flick != FlickType.NotFlick && prev.Flick != FlickType.NotFlick)
                {
                    if (prev.nextConnector != null)
                    {
                        TSystemStatic.Log($"Note {prev.ID} seems to have multiple next-flick connection. Ignoring flick connection with note {ID}.");
                        continue;
                    }
                    var newObj = Instantiate(Game.connectorTemplate);
                    newObj.transform.SetParent(Game.meshParent);
                    newObj.transform.localScale = Vector3.one;
                    newObj.name = "Connector " + prev.ID.ToString();
                    var newConn = newObj.GetComponent<Connector>();
                    newConn.Set(prev, this);
                    prev.nextConnector = newConn;
                    previousConnectors.Add(newConn);
                }
            }
        }

        public void Wakeup()
        {
            if(Type.IsEither(NoteType.HoldEnd, NoteType.SlideMiddle, NoteType.SlideEnd))
            {
                foreach (var note in previousNotes)
                {
                    if (note.Type.IsEither(NoteType.HoldStart, NoteType.SlideStart, NoteType.SlideMiddle) && note.isDead)
                    {
                        isHit = true;
                        isDead = true;
                        if (Type == NoteType.SlideMiddle)
                            Judge(true, true);
                        else
                            Game.judge.noteQueue[EndLine].Remove(ID);
                        Delete();
                        return;
                    }
                }
            }

            if((int)Type < 10)
            {
                if (!Game.judge.noteQueue.ContainsKey(EndLine))
                {
                    Game.judge.AddLine(EndLine);
                    Game.input.AddLine(EndLine);
                }
                Game.judge.noteQueue[EndLine].Add(ID);
            }

            image.sprite = Game.GetNoteImage(data);
            if (Game.IsNoteColored)
                image.color = ColorKey;
            if (image.sprite == null)
                image.color = Color.clear;
            if (Type == NoteType.Hidden)
                image.color = Color.clear;

            if (nextTail != null)
                nextTail.gameObject.SetActive(true);
            if (nextConnector != null)
                nextConnector.gameObject.SetActive(true);
            gameObject.SetActive(true);

            Body.anchoredPosition = startPos;
            isAppeared = true;
        }

        void Update()
        {
            BeforeUpdate();
            if (!Game.Paused)
            {
                Check();
                Move();
                Process();
                GetInput();
            }
            AfterUpdate();
        }

        protected virtual void BeforeUpdate()
        {

        }

        protected virtual void Check()
        {
            if (Type.IsEither(NoteType.HoldEnd, NoteType.SlideMiddle, NoteType.SlideEnd))
            {
                foreach (var note in previousNotes)
                {
                    if (note.Type.IsEither(NoteType.HoldStart, NoteType.SlideStart, NoteType.SlideMiddle) && note.isDead)
                    {
                        isDead = true;
                        if (Type == NoteType.SlideMiddle)
                            Game.judge.noteQueue[EndLine].Remove(ID);
                        Judge(true);
                        Delete();
                    }
                    if (Type.IsEither(NoteType.SlideMiddle, NoteType.SlideEnd) && note.Type.IsEither(NoteType.SlideStart, NoteType.SlideMiddle))
                    {
                        slideGroupFinger = note.slideGroupFinger;
                    }
                }
            }
            if (Type == NoteType.SlideStart && (nextNote.isDead || isDead))
            {
                Delete();
            }
        }

        protected virtual void Move()
        {
            // Tail and Connector process their positions by themselves. They are smart!
            if(isAppeared)
            {
                if(Type == NoteType.SlideStart && (isHit || Progress >= 1))
                {
                    var progress = (nextNote.ReachTime - Game.Time) / (nextNote.ReachTime - Mathf.Min(hitTime, ReachTime));
                    Body.anchoredPosition = Vector2.Lerp(nextNote.endPos, endPos, progress);
                }
                else
                {
                    if (!isHit)
                    {
                        Body.anchoredPosition = Game.Mode.GetCurrentPos(Progress, startPos, endPos);
                        Body.localScale = Game.Mode.GetScale(Progress);
                    }
                }
            }
        }

        protected virtual void Process()
        {
            switch(Type)
            {
                case NoteType.Damage:
                    if(Progress >= 1)
                        Judge();
                    break;
                case NoteType.SlideStart:
                    if (nextNote.Type == NoteType.SlideEnd && nextNote.Progress >= 1)
                        Delete();
                    break;
                case NoteType.SlideMiddle:
                    if(Progress >= 1)
                    {
                        if (!slideTransfered)
                            TransferToBefore();
                        if(latestSlideJudge != JudgeType.NotJudged)
                        {
                            // Slide Middle Judging
                            isHit = true;
                            Game.judge.noteQueue[EndLine].Remove(ID);
                            Game.judge.UpdateJudgeResult(Mathf.RoundToInt(EndLine), latestSlideJudge, false, false);
                            Game.AddScore(data, latestSlideJudge);

                            if (latestSlideJudge == JudgeType.Miss)
                                isDead = true;
                            Delete();
                        }
                    }
                    break;
                case NoteType.Hidden:
                    if (TimeDistance >= Game.Mode.judgeThreshold[1])
                    {
                        Game.judge.noteQueue[EndLine].Remove(ID);
                        Delete();
                    }
                    break;
                case NoteType.Starter:
                    if(Progress >= 1)
                    {
                        Game.StartPlayMusic();
                        Delete();
                    }
                    break;
                case NoteType.Ender:
                    if(Progress >= 1)
                    {
                        Game.EndGame();
                        Delete();
                    }
                    break;
                case NoteType.Scroller:
                    if(Progress >= 1)
                    {
                        Game.TimeSpeed = Speed;
                        Delete();
                    }
                    break;
                case NoteType.LineChanger:
                    if(Progress >= 1)
                    {
                        Game.StartCoroutine(Game.ChangeLine(Size));
                        Delete();
                    }
                    break;
                case NoteType.SlideDummy:
                    if (Game.notes[data.prevIds[0]].isHit || TimeDistance >= Game.Mode.judgeThreshold[5])
                        Delete();
                    break;
            }

            if (!isHit && TimeDistance >= Game.Mode.judgeThreshold[5])   // So, when the note is not hit and reached the end...
            {
                Judge(true);
                if (Type == NoteType.HoldEnd)
                    foreach (var note in previousNotes)
                        if (note.Type == NoteType.HoldStart)
                            note.Delete();
                if(Type == NoteType.SlideStart)
                {
                    Game.judge.noteQueue[EndLine].Remove(ID);
                    Game.judge.UpdateJudgeResult(Mathf.RoundToInt(EndLine), JudgeType.Miss, false, false);
                    Game.AddScore(data, JudgeType.Miss);
                    isDead = true;
                    Delete();
                }
            }
        }

        protected virtual void GetInput()
        {
            if(Game.IsAutoPlay && (int)Type < 10 && Type != NoteType.Hidden)
            {
                if (Progress >= 1 && !isHit)
                    Judge();
            }
        }

        protected virtual void AfterUpdate()
        {

        }

        public void AppendNoteData(NoteData newData, NoteType targetType)
        {
            data = newData.Clone();
            data.type = targetType;
        }

        protected void TransferToBefore()
        {
            // Find previous SlideStart note
            Note prev = null;
            foreach (var cand in previousNotes)
                if (cand.Type == NoteType.SlideStart)
                    prev = cand;

            // Destroy previous tail
            previousTails.Remove(prev.nextTail);
            Destroy(prev.nextTail.gameObject);

            // Append note data to prev
            prev.AppendNoteData(data, NoteType.SlideStart);
            prev.appearTime = appearTime;
            prev.hitTime = ReachTime;
            prev.lineSet = lineSet;
            prev.startPos = startPos;
            prev.endPos = endPos;

            // Tail transfering
            prev.nextTail = nextTail;
            prev.nextTail.headNote = prev;
            prev.nextNote = nextNote;
            nextTail = null;

            nextNote.previousNotes.Remove(this);
            nextNote.previousNotes.Add(prev);

            slideTransfered = true;
        }

        public void Judge(bool fixAsMiss = false, bool silent = false)
        {
            var res = Game.judge.GetJudge(EndLine, Type, Flick, TimeDistance);
            if (fixAsMiss)
                res = JudgeType.Miss;
            if (Type.IsEither(NoteType.HoldEnd, NoteType.SlideEnd) && res == JudgeType.NotJudged)
                res = JudgeType.Miss;

            if (res == JudgeType.NotJudged)
                return;

            // Processing (Slide Middle SAVES the judge, not executing it.)
            if(Type != NoteType.SlideMiddle)
            {
                isHit = true;
                Game.judge.noteQueue[EndLine].Remove(ID);

                Game.judge.UpdateJudgeResult(Mathf.RoundToInt(EndLine), res, Flick != FlickType.NotFlick, silent);
                Game.AddScore(data, res);
            }

            if (Type.IsEither(NoteType.HoldStart, NoteType.SlideStart) && res == JudgeType.Miss)
                isDead = true;

            if (Type.IsEither(NoteType.HoldStart, NoteType.SlideStart) && res != JudgeType.Miss)
            {
                Body.localScale = Vector3.one;
                if (Type == NoteType.HoldStart)
                    HoldHide();
                hitTime = Game.Time;
                animator.Play("DefaultHoldAnimation");
            }
            else if (Type == NoteType.SlideMiddle)
                latestSlideJudge = res;
            else if (!silent)
                Delete();
        }

        public void HoldHide()
        {
            Body.anchoredPosition = endPos;
            image.color = new Color(ColorKey.r, ColorKey.g, ColorKey.b, 0);
        }

        public void Delete()
        {
            // Note state update
            isHit = true;
            animator.StopPlayback();

            // Hold end process code
            if (Type == NoteType.HoldEnd)
                foreach (var tail in previousTails)
                    if (tail.headNote.Type == NoteType.HoldStart)
                        tail.headNote.Delete();

            // Slide end process code
            if(Type == NoteType.SlideEnd)
                foreach (var tail in previousTails)
                    if (tail.headNote.Type == NoteType.SlideStart)
                        tail.headNote.Delete();

            // Tail inactivating
            if (nextTail != null)
                nextTail.gameObject.SetActive(false);

            // Connector inactivating
            if (nextConnector != null)
                nextConnector.gameObject.SetActive(false);
            foreach (var connector in previousConnectors)
                if (connector.gameObject.activeSelf)
                    connector.gameObject.SetActive(false);

            gameObject.SetActive(false);
        }
    }
}