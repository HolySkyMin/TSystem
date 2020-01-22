using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    public class NoteData
    {
        public int id;
        public int size;
        public float time;
        public float speed;
        public float startLine;
        public float endLine;
        public NoteType type;
        public FlickType flick;
        public Color32 color;
        public List<int> prevIds;

        public NoteData() { }

        public NoteData(int i, int si, float t, float sp, float sl, float el, NoteType ty, FlickType f, Color32 c, List<int> p)
        {
            id = i;
            size = si;
            time = t;
            speed = sp;
            startLine = sl;
            endLine = el;
            type = ty;
            flick = f;
            color = c;
            prevIds = p;
        }

        public NoteData(TWxNoteData twx)
        {
            id = twx.ID;
            size = twx.Size;
            time = (float)twx.Time;
            speed = (float)twx.Speed;
            startLine = (float)twx.StartLine;
            endLine = (float)twx.EndLine;
            flick = (FlickType)twx.Flick;
            color = new Color32((byte)twx.Color[0], (byte)twx.Color[1], (byte)twx.Color[2], (byte)twx.Color[3]);
            prevIds = new List<int>();
            foreach (var prev in twx.PrevIDs)
                if (prev > 0)
                    prevIds.Add(prev);

            switch(twx.Mode)
            {
                case 0:
                    type = NoteType.Tap;
                    break;
                case 1:
                    type = NoteType.HoldStart;
                    break;
                case 2:
                    type = NoteType.SlideStart;
                    break;
                case 3:
                    type = NoteType.Damage;
                    break;
                case 4:
                    type = NoteType.Hidden;
                    break;
            }
        }

        public NoteData(SSTrainNoteData sst)
        {
            id = sst.id;
            size = 1;
            time = (float)sst.timing;
            speed = 1;
            startLine = sst.startPos;
            endLine = sst.endPos;
            flick = (FlickType)sst.status;
            color = new Color32(255, 255, 255, 255);
            if (sst.prevNoteId > 0)
                prevIds = new List<int> { sst.prevNoteId };
            else
                prevIds = new List<int>();

            if (sst.type == 1)
                type = NoteType.Tap;
            else if(sst.type == 2)
            {
                if (sst.prevNoteId > 0)
                    type = NoteType.HoldEnd;
                else
                    type = NoteType.HoldStart;
            }
            else if(sst.type == 3)
            {
                if (sst.prevNoteId <= 0)
                    type = NoteType.SlideStart;
                else if (sst.nextNoteId <= 0)
                    type = NoteType.SlideEnd;
                else
                    type = NoteType.SlideMiddle;
            }
        }
    }
}