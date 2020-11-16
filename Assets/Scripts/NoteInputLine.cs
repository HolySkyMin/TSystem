using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    public class NoteInputLine
    {
        public float line;
        public List<int> notes;

        public bool tapHitted;

        public bool isHolding;
        public int holdingNote;

        public Dictionary<FlickType, bool> flickHitted;
        public Dictionary<FlickType, float> flickCooltime;

        public NoteInputLine(float l)
        {
            line = l;

            notes = new List<int>();
            flickHitted = new Dictionary<FlickType, bool>();
            flickCooltime = new Dictionary<FlickType, float>();
        }

        public void UpdateCooltime()
        {
            tapHitted = false;
            foreach(var flick in flickHitted.Keys)
            {
                if(flickHitted[flick])
                {
                    flickCooltime[flick] -= Time.deltaTime;
                    if (flickCooltime[flick] <= 0)
                    {
                        flickCooltime[flick] = 0;
                        flickHitted[flick] = false;
                    }
                }
            }
        }

        public void SetFlickHit(FlickType type)
        {
            if (!flickHitted.ContainsKey(type))
            {
                flickHitted.Add(type, false);
                flickCooltime.Add(type, 0);
            }
            flickHitted[type] = true;
            flickCooltime[type] = 0.06f;
        }
    }
}