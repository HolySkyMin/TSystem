using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    /// <summary>
    /// A component which stores static informations needed for note touch
    /// and provides some useful features for <see cref="NoteInputDetector"/>.
    /// </summary>
    public class NoteInputManager : MonoBehaviour
    {
        /// <summary>
        /// Half of the width of judging area per note.
        /// </summary>
        public float HalfWidth { get { return (float)Game.Mode.judgeHalfWidth; } }

        /// <summary>
        /// Shortcut for ingame component.
        /// </summary>
        IngameBasis Game { get { return IngameBasis.Now; } }

        [HideInInspector] public Dictionary<float, Vector2> lineEnd = new Dictionary<float, Vector2>();
        [HideInInspector] public Dictionary<float, NoteInputLine> lines = new Dictionary<float, NoteInputLine>();

        List<Func<float, Vector2, bool>> isValidTouch;

        // Update is called once per frame
        void Update()
        {
            foreach(var line in lines.Values)
            {
                line.UpdateCooltime();
            }
        }

        /// <summary>
        /// Initializes the component.
        /// </summary>
        public virtual void Initialize()
        {
            isValidTouch = new List<Func<float, Vector2, bool>>()
            {
                (line, pos) => pos.x.IsBetween(lineEnd[line].x - HalfWidth, lineEnd[line].x + HalfWidth, true, false),
                (line, pos) => pos.y.IsBetween(lineEnd[line].y - HalfWidth, lineEnd[line].y + HalfWidth, true, false),
                (line, pos) => pos.x.IsBetween(lineEnd[line].x - HalfWidth, lineEnd[line].x + HalfWidth, true, false) && pos.y.IsBetween(lineEnd[line].y - HalfWidth, lineEnd[line].y + HalfWidth, true, false),
                (line, pos) => Vector2.Distance(pos, lineEnd[line]) <= HalfWidth
            };
        }

        /// <summary>
        /// A function which allows others to add their own rules of note touch area.
        /// </summary>
        /// <param name="rules"> Function variables which designates the limitation of touch area.</param>
        public void SetValidTouchRule(params Func<float, Vector2, bool>[] rules)
        {
            foreach (var rule in rules)
                isValidTouch.Add(rule);
        }

        /// <summary>
        /// A function which returns whether a given point is within the touch area of given line.
        /// </summary>
        /// <param name="line">Line of the note.</param>
        /// <param name="pos">Position of the touch.</param>
        /// <returns></returns>
        public bool IsValidTouch(float line, Vector2 pos) => isValidTouch[Game.Mode.judgeRule](line, pos);

        /// <summary>
        /// Adds a new line to internal data and starts managing it.
        /// </summary>
        /// <param name="line">A line you want to add.</param>
        public void AddLine(float line)
        {
            lineEnd.Add(line, Game.Mode.GetEndPos(Game.curLineSet, line));
            lines.Add(line, new NoteInputLine(line));
        }

        /// <summary>
        /// Updates the information of lines.
        /// Usually invoked when the line set has been changed.
        /// </summary>
        public void UpdateLinePos()
        {
            foreach (var line in lineEnd)
                lineEnd[line.Key] = Game.Mode.GetEndPos(Game.curLineSet, line.Key);
        }

        public void AddNote(float line, int id)
        {
            lines[line].notes.Add(id);
        }

        public void RemoveNote(float line, int id)
        {
            lines[line].notes.Remove(id);
        }

        public bool IsInputTarget(float line, int id)
        {
            if (lines[line].notes.Count < 1)
                return false;
            return lines[line].notes[0] == id;
        }
    }
}