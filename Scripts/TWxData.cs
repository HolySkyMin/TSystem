using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    public struct TWxData
    {
        public int version;
        public TWxMetadata metadata;
        public TWxNoteData[] notes;
    }

    public struct TWxMetadata
    {
        public int level;
        public int density;
        public string artist;
        public string mapper;
    }

    public struct TWxNoteData
    {
        public int ID;
        public int Size;
        public double Time;
        public double Speed;
        public double StartLine;
        public double EndLine;
        public int Mode;
        public int Flick;
        public int[] Color;
        public int[] PrevIDs;
    }
}