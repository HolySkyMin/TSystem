using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TSystem;

namespace TSystem.Example
{
    public class StarlightBasis : IngameBasis
    {
        protected override void Awake()
        {
            // TODO: Flexible flick threshold
            flickThreshold = 80;
            Tail.FixTailPosAtZero = true;
            Connector.HalfWidthCoeff = 0.5f;
            maxReachTime = 0;

            base.Awake();
        }

        public override GameObject GetNoteTemplate(NoteData data)
        {
            return noteTemplate[(int)data.flick];
        }

        public override Sprite GetNoteImage(NoteData data)
        {
            if(data.flick != FlickType.NotFlick)
            {
                return IsNoteColored ? noteWhiteImage[5 + (int)data.flick] : noteColorImage[5 + (int)data.flick];
            }
            else
            {
                switch(data.type)
                {
                    case NoteType.Tap:
                        return IsNoteColored ? noteWhiteImage[0] : noteColorImage[0];
                    case NoteType.HoldStart:
                    case NoteType.HoldEnd:
                        return IsNoteColored ? noteWhiteImage[1] : noteColorImage[1];
                    case NoteType.SlideStart:
                    case NoteType.SlideEnd:
                        return IsNoteColored ? noteWhiteImage[2] : noteColorImage[2];
                    case NoteType.SlideMiddle:
                        return IsNoteColored ? noteWhiteImage[3] : noteColorImage[3];
                    case NoteType.Damage:
                        return IsNoteColored ? noteWhiteImage[4] : noteColorImage[4];
                    case NoteType.Hidden:
                        return IsNoteColored ? noteWhiteImage[5] : noteColorImage[5];
                    default:
                        return null;
                }
            }
        }

        public override void CreateNote(NoteData data)
        {
            if ((int)data.type >= 10)
                data.id = systemNoteIdx--;

            if (data.type == NoteType.LineChanger)
                curLineSet = data.size;

            if(Packet.mirror)
            {
                data.startLine = Mode.lineSet[curLineSet].count + 1 - data.startLine;
                data.endLine = Mode.lineSet[curLineSet].count + 1 - data.endLine;
                if (data.flick == FlickType.Left)
                    data.flick = FlickType.Right;
                else if (data.flick == FlickType.Right)
                    data.flick = FlickType.Left;
            }

            var newObj = Instantiate(GetNoteTemplate(data));
            newObj.GetComponent<RectTransform>().SetParent(noteParent);
            newObj.name = "Note " + data.id.ToString();

            var newNote = newObj.GetComponent<Note>();
            newNote.Set(data, curLineSet);
            newNote.halfTailWidth = 80;
            notes.Add(newNote.ID, newNote);
            if ((int)newNote.Type < 10 && newNote.Type != NoteType.Hidden)
                ValidNoteCount++;

            if (newNote.Type == NoteType.SlideStart && newNote.previousNotes.Count < 1)
                CreateNote(new NoteData(data.id, data.size, data.time, data.speed, data.startLine, data.endLine,
                    NoteType.SlideDummy, data.flick, data.color, new List<int>() { data.id }));

            maxReachTime = Mathf.Max(maxReachTime, data.time);
        }

        protected override void AfterNoteLoading()
        {
            foreach(var note in notes)
            {
                note.Value.CreateTail();
                note.Value.CreateConnector();
            }
        }

        public override void AddScore(NoteData data, JudgeType result)
        {
            float BASE_SCORE = 1000;
            float BASE_PERCENTAGE = 100f / ValidNoteCount;

            if (result == JudgeType.Fantastic)
            {
                BASE_SCORE *= 1.1f;
                BASE_PERCENTAGE *= 1.1f;
            }
            else if (result == JudgeType.Great)
            {
                BASE_SCORE *= 0.7f;
                BASE_PERCENTAGE *= 0.7f;
            }
            else if(result == JudgeType.Nice)
            {
                BASE_SCORE *= 0.4f;
                BASE_PERCENTAGE *= 0.4f;
            }
            else if(result == JudgeType.Bad)
            {
                BASE_SCORE *= 0.1f;
                BASE_PERCENTAGE *= 0.1f;
            }
            else if(result == JudgeType.Miss)
            {
                BASE_SCORE = 0;
                BASE_PERCENTAGE = 0;
            }

            float comboPerc = judge.comboCount / (float)ValidNoteCount;
            if (comboPerc < 0.05f)
                BASE_SCORE *= 1f;
            else if (comboPerc < 0.1f)
                BASE_SCORE *= 1.1f;
            else if (comboPerc < 0.25f)
                BASE_SCORE *= 1.2f;
            else if (comboPerc < 0.5f)
                BASE_SCORE *= 1.3f;
            else if (comboPerc < 0.7f)
                BASE_SCORE *= 1.4f;
            else if (comboPerc < 0.8f)
                BASE_SCORE *= 1.5f;
            else if (comboPerc < 0.9f)
                BASE_SCORE *= 1.7f;
            else if (comboPerc <= 1f)
                BASE_SCORE *= 2f;
            else
                BASE_SCORE *= 2.4f;

            judge.score += Mathf.RoundToInt(BASE_SCORE);
            judge.percentage += BASE_PERCENTAGE;
        }
    }
}