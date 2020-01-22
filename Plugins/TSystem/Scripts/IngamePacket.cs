using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    public class IngamePacket
    {
        public bool autoPlay;
        public bool noBGA;
        public bool noMusic;
        public bool mirror;
        public bool loadFromResources;
        public string backImagePath;
        public string bgaPath;
        public string musicPath;
        public BeatmapData beatmap;
        public TSystemMode gameMode;
    }
}