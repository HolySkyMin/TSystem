using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    public class InputStatus
    {
        // General
        public float line;
        public int touchingFinger;

        // Tap
        public bool tapHitted;
        public float tapCooltime;

        // Hold
        public bool isHolding;
        public int holdingNote;
        
        // Flick
        public Dictionary<FlickType, bool> flickHitted;
        public Dictionary<FlickType, float> flickCooltime;
        public bool flickStarted;
        public FlickType flickDirection;
        public Vector2 flickStartPos;
        public float flickMovedDistance;

        public InputStatus(float l)
        {
            line = l;
            touchingFinger = 100;
            tapHitted = false;
            tapCooltime = 0;
            isHolding = false;
            holdingNote = 0;
            flickStarted = false;
            flickDirection = FlickType.NotFlick;
            flickMovedDistance = 0;
            flickHitted = new Dictionary<FlickType, bool>()
            {
                {FlickType.Left, false },
                {FlickType.Right, false },
                {FlickType.Up, false },
                {FlickType.Down, false },
                {FlickType.Free, false }
            };
            flickCooltime = new Dictionary<FlickType, float>()
            {
                {FlickType.Left, 0 },
                {FlickType.Right, 0 },
                {FlickType.Up, 0 },
                {FlickType.Down, 0 },
                {FlickType.Free, 0 }
            };
        }

        public Note GetTargetNote()
        {
            if (IngameBasis.Now.judge.noteQueue[line].Count > 0)
                return IngameBasis.Now.notes[IngameBasis.Now.judge.noteQueue[line][0]];
            else
                return null;
        }

        public void SetTapHit()
        {
            tapHitted = true;
            tapCooltime = 1;
        }

        public void SetHold(int finger, int holding)
        {
            touchingFinger = finger;
            isHolding = true;
            holdingNote = holding;
        }

        public void StartFlickCheck(int finger, FlickType flick, Vector2 startPos, Vector2 deltaPos)
        {
            touchingFinger = finger;
            flickStarted = true;
            flickDirection = flick;
            flickStartPos = startPos;

            UpdateFlickCheck(startPos + deltaPos);
        }

        public void UpdateFlickCheck(Vector2 curPos)
        {
            switch(flickDirection)
            {
                case FlickType.Left:
                    flickMovedDistance = flickStartPos.x - curPos.x;
                    break;
                case FlickType.Right:
                    flickMovedDistance = curPos.x - flickStartPos.x;
                    break;
                case FlickType.Up:
                    flickMovedDistance = curPos.y - flickStartPos.y;
                    break;
                case FlickType.Down:
                    flickMovedDistance = flickStartPos.y - curPos.y;
                    break;
                case FlickType.Free:
                    flickMovedDistance = Vector2.Distance(flickStartPos, curPos);
                    break;
            }
        }

        public bool IsFlickEnough()
        {
            return flickMovedDistance >= IngameBasis.Now.flickThreshold;
        }
    }
}