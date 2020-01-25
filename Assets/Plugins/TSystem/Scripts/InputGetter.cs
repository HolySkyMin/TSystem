using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    public class InputGetter : MonoBehaviour
    {
        public IngameBasis Game { get { return IngameBasis.Now; } }
        public float HalfWidth { get { return (float)Game.Mode.judgeHalfWidth; } }

        [HideInInspector] public Dictionary<float, Vector2> lineEnd = new Dictionary<float, Vector2>();
        [HideInInspector] public Dictionary<float, InputStatus> lineInput = new Dictionary<float, InputStatus>();

        List<Func<float, Vector2, bool>> isValidTouch;
        List<int> slidingFinger;
        List<int> slidingNote;

        public void Initialize()
        {
            isValidTouch = new List<Func<float, Vector2, bool>>()
            {
                (line, pos) => pos.x.IsBetween(lineEnd[line].x - HalfWidth, lineEnd[line].x + HalfWidth, true, false),
                (line, pos) => pos.y.IsBetween(lineEnd[line].y - HalfWidth, lineEnd[line].y + HalfWidth, true, false),
                (line, pos) => pos.x.IsBetween(lineEnd[line].x - HalfWidth, lineEnd[line].x + HalfWidth, true, false) && pos.y.IsBetween(lineEnd[line].y - HalfWidth, lineEnd[line].y + HalfWidth, true, false),
                (line, pos) => Vector2.Distance(pos, lineEnd[line]) <= HalfWidth
            };
            slidingFinger = new List<int>();
            slidingNote = new List<int>();
        }

        public void SetValidTouchRule(params Func<float, Vector2, bool>[] rules)
        {
            foreach (var rule in rules)
                isValidTouch.Add(rule);
        }

        public void AddLine(float line)
        {
            lineEnd.Add(line, Game.Mode.GetEndPos(Game.curLineSet, line));
            lineInput.Add(line, new InputStatus(line));
        }

        public void UpdateLinePos()
        {
            foreach (var line in lineEnd)
                lineEnd[line.Key] = Game.Mode.GetEndPos(Game.curLineSet, line.Key);
        }

        void Update()
        {
            var normalTouch = new Dictionary<float, Touch>();
            var holdingTouch = new Dictionary<float, Touch>();
            var flickingTouch = new Dictionary<float, Touch>();
            var slidingTouch = new List<Touch>();

            foreach(var touch in Input.touches)
            {
                var pos = Game.GetTouchPos(touch.position);
                foreach (var line in lineEnd)
                {
                    if (isValidTouch[Game.Mode.judgeRule](line.Key, pos))
                        normalTouch.Add(line.Key, touch);
                    if(lineInput[line.Key].touchingFinger == touch.fingerId)
                    {
                        if (lineInput[line.Key].isHolding)
                            holdingTouch.Add(line.Key, touch);
                        if (lineInput[line.Key].flickStarted)
                        {
                            flickingTouch.Add(line.Key, touch);
                            if (normalTouch.ContainsKey(line.Key))
                                normalTouch.Remove(line.Key);
                        }
                    }
                }
                if (slidingFinger.Contains(touch.fingerId))
                    slidingTouch.Add(touch);
            }

            // Process rule: normal -> flicking -> holding -> sliding

            foreach(var touch in normalTouch)
            {
                var pos = Game.GetTouchPos(touch.Value.position);
                var line = lineInput[touch.Key];

                if (line.GetTargetNote() != null && line.GetTargetNote().TimeDistance >= -Game.Mode.judgeThreshold[5])
                {
                    var target = line.GetTargetNote();

                    // If it's FLICK, start flick checking.
                    if (!line.flickStarted && target.Flick != FlickType.NotFlick && touch.Value.phase.IsEither(TouchPhase.Stationary, TouchPhase.Moved))
                        line.StartFlickCheck(touch.Value.fingerId, target.Flick, pos, Game.GetTouchPos(touch.Value.deltaPosition));
                    else if(target.Flick == FlickType.NotFlick)
                    {
                        // Switching by target's TYPE...
                        switch (target.Type)
                        {
                            case NoteType.Tap:
                            case NoteType.Hidden:
                                if (touch.Value.phase == TouchPhase.Began)
                                    target.Judge();
                                break;
                            case NoteType.Damage:
                                if (touch.Value.phase == TouchPhase.Began)
                                    target.Judge(true);
                                break;
                            case NoteType.HoldStart:
                                if (touch.Value.phase == TouchPhase.Began)
                                {
                                    target.Judge();
                                    if (target.isHit && !target.isDead)
                                        line.SetHold(touch.Value.fingerId, target.ID);
                                }
                                break;
                            case NoteType.SlideStart:
                                if (touch.Value.phase == TouchPhase.Began)
                                {
                                    target.Judge();
                                    if (target.isHit && !target.isDead)
                                    {
                                        slidingFinger.Add(touch.Value.fingerId);
                                        slidingNote.Add(target.ID);
                                        target.slideGroupFinger = touch.Value.fingerId;
                                    }
                                }
                                break;
                            case NoteType.SlideMiddle:
                                if (touch.Value.fingerId == target.slideGroupFinger && touch.Value.phase.IsEither(TouchPhase.Began, TouchPhase.Stationary, TouchPhase.Moved))
                                    target.Judge();
                                break;
                            case NoteType.SlideEnd:
                                if (touch.Value.fingerId == target.slideGroupFinger && touch.Value.phase == TouchPhase.Ended)
                                {
                                    target.Judge();
                                    slidingNote.RemoveAt(slidingFinger.IndexOf(touch.Value.fingerId));
                                    slidingFinger.Remove(touch.Value.fingerId);
                                }
                                break;
                        }
                    }
                }
            }

            foreach(var touch in flickingTouch)
            {
                var pos = Game.GetTouchPos(touch.Value.position);
                var line = lineInput[touch.Key];
                var target = line.GetTargetNote();

                if (touch.Value.phase == TouchPhase.Moved)
                {
                    // Update flick pos information, and check if enough.
                    line.UpdateFlickCheck(pos);
                    if (line.IsFlickEnough() && target.TimeDistance >= -Game.Mode.judgeThreshold[5])
                    {
                        target.Judge();
                        if (target.isHit)
                        {
                            line.flickStarted = false;
                            line.touchingFinger = 100;
                            if (target.Type == NoteType.HoldEnd)
                                line.isHolding = false;
                            if (target.Type == NoteType.SlideEnd)
                            {
                                slidingNote.RemoveAt(slidingFinger.IndexOf(target.slideGroupFinger));
                                slidingFinger.Remove(target.slideGroupFinger);
                            }
                        }
                    }
                }
                else if (touch.Value.phase == TouchPhase.Ended)
                {
                    // Roll back everything.
                    line.flickStarted = false;
                    line.touchingFinger = 100;
                }
            }

            foreach(var touch in holdingTouch)
            {
                var pos = Game.GetTouchPos(touch.Value.position);
                var line = lineInput[touch.Key];
                var target = line.GetTargetNote();

                // Holding and RELEASED,
                if (touch.Value.phase == TouchPhase.Ended)
                {
                    // If at proper location,
                    if (isValidTouch[Game.Mode.judgeRule](touch.Key, pos))
                    {
                        // If 1) target note exists and 2) target note is hold-end and 3) target note is not flick,
                        if (target != null && target.Type == NoteType.HoldEnd && target.Flick == FlickType.NotFlick)
                            target.Judge();
                        else
                        {
                            // Else, it's MISS.
                            Game.notes[line.holdingNote].isDead = true;
                            if (!Game.notes[line.holdingNote].nextNote.isAppeared)
                                Game.notes[line.holdingNote].Delete();
                        }
                    }
                    else
                    {
                        // Else, it's MISS.
                        Game.notes[line.holdingNote].isDead = true;
                        if (!Game.notes[line.holdingNote].nextNote.isAppeared)
                            Game.notes[line.holdingNote].Delete();
                    }
                    line.isHolding = false;
                    line.touchingFinger = 100;
                }
            }

            foreach(var touch in slidingTouch)
            {
                if (slidingFinger.Contains(touch.fingerId) && touch.phase == TouchPhase.Ended)
                {
                    Game.notes[slidingNote[slidingFinger.IndexOf(touch.fingerId)]].isDead = true;
                    slidingNote.RemoveAt(slidingFinger.IndexOf(touch.fingerId));
                    slidingFinger.Remove(touch.fingerId);
                }
            }
        }
    }
}