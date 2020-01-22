using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    public struct SSTrainData
    {
        public SSTrainMetadata metadata;
        public SSTrainNoteData[] notes;
    }

    public struct SSTrainMetadata
    {
        public int difficulty;
        public int level;
    }

    public struct SSTrainNoteData
    {
        public int id;
        public int type;
        public int startPos;
        public int endPos;
        public int status;
        public int prevNoteId;
        public int nextNoteId;
        public double timing;
    }
}