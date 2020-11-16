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
            flickHitted = new Dictionary<FlickType, bool>()
            {
                { FlickType.Left, false },
                { FlickType.Right, false },
                { FlickType.Up, false },
                { FlickType.Down, false },
                { FlickType.Free, false }
            };
            flickCooltime = new Dictionary<FlickType, float>()
            {
                { FlickType.Left, 0f },
                { FlickType.Right, 0f },
                { FlickType.Up, 0f },
                { FlickType.Down, 0f },
                { FlickType.Free, 0f }
            };
        }

        public void UpdateCooltime()
        {
            tapHitted = false;

            for(int i = 1; i <= 5; i++)
            {
                var flick = (FlickType)i;
                if (flickHitted[flick])
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
            flickHitted[type] = true;
            flickCooltime[type] = 0.06f;
        }
    }
}