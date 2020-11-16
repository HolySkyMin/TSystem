using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    /// <summary>
    /// A component which detects user input for a note.
    /// </summary>
    [RequireComponent(typeof(Note))]
    public class NoteInputDetector : MonoBehaviour
    {
        /// <summary>
        /// Shortcut for ingame component.
        /// </summary>
        protected IngameBasis Game { get { return IngameBasis.Now; } }

        /// <summary>
        /// Shortcut for <see cref="NoteInputManager"/> component from ingame component.
        /// </summary>
        protected NoteInputManager Manager { get { return IngameBasis.Now.noteInput; } }

        /// <summary>
        /// Note object which this script is being assigned to.
        /// </summary>
        protected Note note;
        protected List<Touch> validTouch;

        protected bool flickStarted;
        protected Vector2 flickStartPos;
        protected float flickMovedDistance;
        protected int flickFinger;

        protected bool isHolding;

        protected virtual void Start()
        {
            note = GetComponent<Note>();
            validTouch = new List<Touch>();

            if (!Manager.lines.ContainsKey(note.EndLine))
                Manager.AddLine(note.EndLine);
            Manager.AddNote(note.EndLine, note.ID);

            flickFinger = 100;
        }


        protected virtual void Update()
        {
            if(Game.IsAutoPlay)
            {
                if((int)note.Type < 10 && note.Type != NoteType.Hidden)
                {
                    if (note.Progress > 1 && !note.isHit)
                        note.Judge();
                }
                return;
            }

            validTouch.Clear();

            foreach (var touch in Input.touches)
            {
                var pos = Game.GetTouchPos(touch.position);
                if (Manager.IsValidTouch(note.EndLine, pos) || touch.fingerId.IsEither(note.slideGroupFinger, flickFinger))
                    validTouch.Add(touch);
            }

            CheckTap();
            CheckHold();
            CheckRelease();
            CheckFlick();
        }

        protected virtual void CheckTap()
        {
            // In tap recognition, This note should be the very front.
            if (!Manager.IsInputTarget(note.EndLine, note.ID))
                return;  // If not, why should we run this?

            // Flick note should not receive tap input.
            if (note.Flick != FlickType.NotFlick)
                return;

            // Of course the touch is valid. But, its position is not guaranteed to be valid.
            // Tap should only check the touch which is valid also in position.
            foreach (var touch in validTouch)
            {
                var pos = Game.GetTouchPos(touch.position);
                if(Manager.IsValidTouch(note.EndLine, pos) && touch.phase == TouchPhase.Began)
                {
                    // Here, the touch is finally assumed to be 'Tap'.
                    // This means that the user has tapped for this note.

                    // If this is the flick note and flick has not been started, starts the flick input.
                    if (note.Flick != FlickType.NotFlick && !flickStarted)
                        StartFlick(touch);
                    // Else, checks the tap input.
                    else if(note.Flick == FlickType.NotFlick)
                    {
                        switch(note.Type)
                        {
                            case NoteType.Tap:
                            case NoteType.Hidden:
                            case NoteType.Damage:
                                if(Manager.lines[note.EndLine].tapHitted)
                                {
                                    note.Judge(note.Type == NoteType.Damage);
                                    Manager.lines[note.EndLine].tapHitted = note.isHit;
                                }
                                break;
                            case NoteType.HoldStart:
                            case NoteType.SlideStart:
                                if(Manager.lines[note.EndLine].tapHitted)
                                {
                                    note.Judge();
                                    Manager.lines[note.EndLine].tapHitted = note.isHit;

                                    if(note.isHit && !note.isDead)
                                    {
                                        isHolding = true;
                                        note.slideGroupFinger = touch.fingerId;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        protected virtual void CheckHold()
        {
            foreach (var touch in validTouch)
            {
                if (touch.fingerId != note.slideGroupFinger)
                    continue;

                var pos = Game.GetTouchPos(touch.position);
                if(touch.phase.IsEither(TouchPhase.Began, TouchPhase.Stationary, TouchPhase.Moved))
                {
                    switch (note.Type)
                    {
                        case NoteType.HoldStart:
                        case NoteType.SlideStart:
                            // Do some hold score increment when the touch position is in the 'note'.
                            // This case, the note itself is already being hit.
                            break;
                        case NoteType.HoldEnd:
                        case NoteType.SlideMiddle:
                        case NoteType.SlideEnd:
                            if (note.Type.IsEither(NoteType.HoldEnd, NoteType.SlideEnd) && Game.Mode.requireReleaseAtSlideEnd)
                                break;

                            // This case, the note should be the input target.
                            if(Manager.IsInputTarget(note.EndLine, note.ID))
                                note.JudgeHold();
                            break;
                    }
                }
            }
        }

        // EVERY TIME RELEASING IS THE F*CKING PROBLEM.
        protected virtual void CheckRelease()
        {
            // Notes checking release input: HoldEnd, SlideEnd, HoldStart, SlideStart.
            // When to check release input? - They are the target (Ends) or, they are being holded (Starts).
            if(Manager.IsInputTarget(note.EndLine, note.ID) || isHolding)
            {
                foreach (var touch in validTouch)
                {
                    if (touch.fingerId != note.slideGroupFinger || touch.phase != TouchPhase.Ended)
                        continue;

                    // Here, the touch is guaranteed to be the release touch.

                    var pos = Game.GetTouchPos(touch.position);

                    // Easy one: Hold release.
                    // Hard one: Slide release.
                    switch(note.Type)
                    {
                        case NoteType.HoldStart:
                        case NoteType.SlideStart:
                            // When the next note is Ends and they are appeared and they are input target and finger is on their line...
                            // ...Easily to say, if the next note is Ends and they can detect this release input also, ignore this input.
                            if (note.nextNote.Type.IsEither(NoteType.HoldEnd, NoteType.SlideEnd) 
                                && note.nextNote.isAppeared
                                && Manager.IsInputTarget(note.nextNote.EndLine, note.nextNote.ID) 
                                && Manager.IsValidTouch(note.nextNote.EndLine, pos))
                                continue;

                            if (Game.Mode.requireHoldToSustainSlide)
                                note.Judge(JudgeType.Miss);
                            else
                            {
                                // Disable animation
                                note.animator.gameObject.SetActive(false);
                            }
                            break;
                        case NoteType.HoldEnd:
                        case NoteType.SlideEnd:
                            // Entering this means that this note has been appeared.
                            // Only consider the touch which is for this note. Otherwise, Starts will judge the note... maybe.
                            if (Manager.IsInputTarget(note.EndLine, note.ID) && Manager.IsValidTouch(note.EndLine, pos))
                            {
                                note.Judge();
                            }
                            break;
                    }
                }
            }
        }

        protected virtual void CheckFlick()
        {
            if (note.Flick == FlickType.NotFlick)
                return;

            foreach (var touch in validTouch)
            {
                if(touch.phase == TouchPhase.Moved)
                {
                    if (!flickStarted)
                        StartFlick(touch);
                    
                    var pos = Game.GetTouchPos(touch.position);
                    
                    switch(note.Flick)
                    {
                        case FlickType.Left:
                            flickMovedDistance = flickStartPos.x - pos.x;
                            break;
                        case FlickType.Right:
                            flickMovedDistance = pos.x - flickStartPos.x;
                            break;
                        case FlickType.Up:
                            flickMovedDistance = pos.y - flickStartPos.y;
                            break;
                        case FlickType.Down:
                            flickMovedDistance = flickStartPos.y - pos.y;
                            break;
                        case FlickType.Free:
                            flickMovedDistance = Vector2.Distance(flickStartPos, pos);
                            break;
                    }

                    if(flickMovedDistance >= Game.Mode.flickThreshold && note.TimeDistance >= -Game.Mode.judgeThreshold[5]
                        && !Manager.lines[note.EndLine].flickHitted[note.Flick])
                    {
                        note.Judge();

                        if(note.isHit)
                        {
                            Manager.lines[note.EndLine].SetFlickHit(note.Flick);
                        }
                    }
                }
                else if(touch.phase == TouchPhase.Ended)
                {
                    flickStarted = false;
                    flickFinger = 100;
                }
            }
        }

        protected void StartFlick(Touch touch)
        {
            flickStarted = true;
            flickFinger = touch.fingerId;
            flickStartPos = Game.GetTouchPos(touch.position);
        }
    }

}