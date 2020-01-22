using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    public enum NoteType
    {
        Tap, HoldStart, HoldEnd, SlideStart, SlideMiddle, SlideEnd, Damage, Hidden,
        Starter = 10, Ender, Scroller, LineChanger, SlideDummy,
        SpecialEnterer = 20, SpecialLeaver
    }

    public enum FlickType
    {
        NotFlick, Left, Right, Up, Down, Free
    }

    public enum BeatmapType { TWx, Deleste, SSTrain }

    public enum JudgeType { NotJudged, Miss, Bad, Nice, Great, Perfect, Fantastic }
}