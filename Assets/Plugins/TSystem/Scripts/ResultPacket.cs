using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    public class ResultPacket
    {
        public bool autoPlay;
        public bool mirror;
        public string songName;
        public TSystemMode gameMode;
        public BeatmapData beatmap;
        public Dictionary<JudgeType, int> judgeList;
    }
}