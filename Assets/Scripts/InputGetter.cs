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

        public bool IsValidTouch(float line, Vector2 pos) => isValidTouch[Game.Mode.judgeRule](line, pos);

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

        public void RemoveSlideFinger(int fingerId)
        {
            slidingNote.RemoveAt(slidingFinger.IndexOf(fingerId));
            slidingFinger.Remove(fingerId);
        }

        void Update()
        {
            foreach (var line in lineInput)
                line.Value.tapHitted = false;

            if (!Game.IsStarted || Game.Paused || Game.IsEnded)
                return;
            if (Game.IsAutoPlay)
                return;

            var normalTouch = new Dictionary<float, List<Touch>>();
            var holdingTouch = new Dictionary<float, Touch>();
            var flickingTouch = new Dictionary<float, Touch>();
            var slidingTouch = new List<Touch>();

            foreach(var touch in Input.touches)
            {
                var pos = Game.GetTouchPos(touch.position);
                foreach (var line in lineEnd)
                {
                    if (isValidTouch[Game.Mode.judgeRule](line.Key, pos))
                    {
                        if (!normalTouch.ContainsKey(line.Key))
                            normalTouch.Add(line.Key, new List<Touch>());
                        normalTouch[line.Key].Add(touch);
                    }
                    if(lineInput[line.Key].touchingFinger == touch.fingerId)
                    {
                        if (lineInput[line.Key].isHolding)
                            if(!holdingTouch.ContainsKey(line.Key))
                                holdingTouch.Add(line.Key, touch);
                        if (lineInput[line.Key].flickStarted)
                        {
                            if(!flickingTouch.ContainsKey(line.Key))
                                flickingTouch.Add(line.Key, touch);
                            if (normalTouch.ContainsKey(line.Key) && normalTouch[line.Key].Contains(touch))
                                normalTouch[line.Key].Remove(touch);
                        }
                    }
                }
                if (slidingFinger.Contains(touch.fingerId))
                    slidingTouch.Add(touch);
            }

            // Process rule: normal -> flicking -> holding -> sliding

            foreach(var touchGroup in normalTouch)
            {
                var line = lineInput[touchGroup.Key];
                foreach (var touch in touchGroup.Value)
                {
                    var pos = Game.GetTouchPos(touch.position);
                    if (line.GetTargetNote() != null && line.GetTargetNote().TimeDistance >= -Game.Mode.judgeThreshold[5])
                    {
                        var target = line.GetTargetNote();

                        // If it's FLICK, start flick checking.
                        if (!line.flickStarted && target.Flick != FlickType.NotFlick && touch.phase.IsEither(TouchPhase.Stationary, TouchPhase.Moved) && line.IsFlickAvailable(target.Flick))
                            line.StartFlickCheck(touch.fingerId, target.Flick, pos, Game.GetTouchPos(touch.deltaPosition));
                        else if (target.Flick == FlickType.NotFlick)
                        {
                            // Switching by target's TYPE...
                            switch (target.Type)
                            {
                                case NoteType.Tap:
                                case NoteType.Hidden:
                                    if (touch.phase == TouchPhase.Began && !line.tapHitted)
                                    {
                                        target.Judge();
                                        line.tapHitted = true;
                                    }
                                    break;
                                case NoteType.Damage:
                                    if (touch.phase == TouchPhase.Began && !line.tapHitted)
                                    {
                                        target.Judge(true);
                                        line.tapHitted = true;
                                    }
                                    break;
                                case NoteType.HoldStart:
                                    if (touch.phase == TouchPhase.Began && !line.tapHitted)
                                    {
                                        target.Judge();
                                        if (target.isHit && !target.isDead)
                                        {
                                            target.slideGroupFinger = touch.fingerId;
                                            line.SetHold(touch.fingerId, target.ID);
                                        }
                                        line.tapHitted = true;
                                    }
                                    break;
                                case NoteType.SlideStart:
                                    if (touch.phase == TouchPhase.Began && !line.tapHitted)
                                    {
                                        target.Judge();
                                        if (target.isHit && !target.isDead)
                                        {
                                            slidingFinger.Add(touch.fingerId);
                                            slidingNote.Add(target.ID);
                                            target.slideGroupFinger = touch.fingerId;
                                        }
                                        line.tapHitted = true;
                                    }
                                    break;
                                case NoteType.SlideMiddle:
                                    if (touch.fingerId == target.slideGroupFinger && touch.phase.IsEither(TouchPhase.Began, TouchPhase.Stationary, TouchPhase.Moved))
                                        target.Judge();
                                    break;
                                case NoteType.SlideEnd:
                                    if (touch.fingerId == target.slideGroupFinger && touch.phase == TouchPhase.Ended)
                                    {
                                        target.Judge();
                                        RemoveSlideFinger(touch.fingerId);
                                    }
                                    if (!Game.Mode.requireReleaseAtSlideEnd && touch.fingerId == target.slideGroupFinger && touch.phase.IsEither(TouchPhase.Began, TouchPhase.Stationary, TouchPhase.Moved))
                                        target.Judge();
                                    break;
                            }
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
                            line.SetFlickHit();
                            if (target.Type == NoteType.HoldEnd)
                                line.isHolding = false;
                            if (target.Type == NoteType.SlideEnd)
                                RemoveSlideFinger(target.slideGroupFinger);
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

                if (!Game.Mode.requireReleaseAtSlideEnd && touch.Value.phase.IsEither(TouchPhase.Began, TouchPhase.Stationary, TouchPhase.Moved))
                {
                    if (target != null && target.Type == NoteType.HoldEnd && target.Flick == FlickType.NotFlick && target.Progress >= 1)
                    {
                        if (isValidTouch[Game.Mode.judgeRule](touch.Key, pos))
                            target.Judge();
                        else
                        {
                            Game.notes[line.holdingNote].isDead = true;
                            if (!Game.notes[line.holdingNote].nextNote.isAppeared)
                            {
                                Game.judge.UpdateJudgeResult((int)line.line, JudgeType.Miss, false, false, false);
                                Game.notes[line.holdingNote].Delete();
                            }
                        }
                    }
                }
                
                // Holding and RELEASED,
                else if (touch.Value.phase == TouchPhase.Ended)
                {
                    // If at proper location,
                    if (isValidTouch[Game.Mode.judgeRule](touch.Key, pos))
                    {
                        // If 1) target note exists and 2) target note is hold-end and 3) target note is not flick,
                        if (target != null && target.Type == NoteType.HoldEnd && target.Flick == FlickType.NotFlick)
                            target.Judge();
                        else
                        {
                            Game.notes[line.holdingNote].isDead = true;
                            if (!Game.notes[line.holdingNote].nextNote.isAppeared)
                            {
                                Game.judge.UpdateJudgeResult((int)line.line, JudgeType.Miss, false, false, false);
                                Game.notes[line.holdingNote].Delete();
                            }
                        }
                    }
                    else
                    {
                        Game.notes[line.holdingNote].isDead = true;
                        if (!Game.notes[line.holdingNote].nextNote.isAppeared)
                        {
                            Game.judge.UpdateJudgeResult((int)line.line, JudgeType.Miss, false, false, false);
                            Game.notes[line.holdingNote].Delete();
                        }
                    }
                    line.isHolding = false;
                    line.touchingFinger = 100;
                }
            }

            foreach(var touch in slidingTouch)
            {
                if (slidingFinger.Contains(touch.fingerId) && touch.phase == TouchPhase.Ended)
                {
                    Game.judge.UpdateJudgeResult(0, JudgeType.Miss, false, false, false);
                    Game.notes[slidingNote[slidingFinger.IndexOf(touch.fingerId)]].isDead = true;
                    RemoveSlideFinger(touch.fingerId);
                }
            }

            foreach (var status in lineInput.Values)
                status.UpdateCooltime();
        }
    }
}