using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using B83.ExpressionParser;

namespace TSystem
{
    [System.Serializable]
    public class TSystemMode
    {
        // Basic information of the mode
        public string name;             // Displayed name of the mode
        public string description;      // Short description
        public int basis;               // Basis index that the mode uses
        public int preferredLine;       // This helps you determine which file extension is fit.

        // Lines and positions of the mode
        public TSModeLineData[] lineSet;

        // Touch boundary and judge regulations
        public double borderLeft;        // Left border of touch boundary
        public double borderRight;       // Right border of touch boundary
        public double borderUp;          // Upper border of touch boundary
        public double borderDown;        // Lower border of touch boundary
        public int judgeRule;           // Judge rule. 0: Vertical-free, 1: Horizontal-free, 2: Square, 3: Circle
        public double judgeHalfWidth;    // Half width of judge
        public double[] judgeThreshold;  // Threshold value of note judging
        public double[] judgeHFSThreshold;// Threshold value of note (Hold/Flick/Slide) judging

        // Note paths and scales
        public bool useBezierPath;      // Will you use bezier algorithm for note path computing?
        public bool useBezierScale;     // Will you use bezier algorithm for note scale computing?
        public bool useScaledTForPath;
        public bool useScaledTForScale;

        public string scaledT;
        public string pathX;            // Arithmatic expression for X value of note path
        public string pathY;            // Arithmatic expression for Y value of note path
        public string scaleX;           // Arithmatic expression for X value of note scale
        public string scaleY;           // Arithmatic expression for Y value of note scale

        public double[] bezierPathX;     // X value of Bezier control points for note path
        public double[] bezierPathY;     // Y value of Bezier control points for note path
        public double[] bezierScaleX;    // X value of Bezier control points for note scale
        public double[] bezierScaleY;    // Y value of Bezier control points for note scale

        //ExpressionContext context;
        //IGenericExpression<float> eScaledT, ePathX, ePathY, eScaleX, eScaleY;
        ExpressionDelegate eScaledT, ePathX, ePathY, eScaleX, eScaleY;

        public void SetArguments()
        {
            //context = new ExpressionContext();
            //context.Imports.AddType(typeof(System.Math));

            //// Context value reset
            //context.Variables["t"] = 0.0f;
            //context.Variables["T"] = 0.0f;
            //eScaledT = context.CompileGeneric<float>(scaledT);
            //ePathX = context.CompileGeneric<float>(pathX);
            //ePathY = context.CompileGeneric<float>(pathY);
            //eScaleX = context.CompileGeneric<float>(scaleX);
            //eScaleY = context.CompileGeneric<float>(scaleY);

            var parser = new ExpressionParser();

            if (useScaledTForPath || useScaledTForScale)
            {
                eScaledT = parser.EvaluateExpression(scaledT).ToDelegate("t");
                _ = eScaledT(0);
            }

            if (!useBezierPath)
            {
                ePathX = parser.EvaluateExpression(pathX).ToDelegate(useScaledTForPath ? "T" : "t");
                ePathY = parser.EvaluateExpression(pathY).ToDelegate(useScaledTForPath ? "T" : "t");
                _ = ePathX(0);
                _ = ePathY(0);
            }
            if(!useBezierScale)
            {
                eScaleX = parser.EvaluateExpression(scaleX).ToDelegate(useScaledTForScale ? "T" : "t");
                eScaleY = parser.EvaluateExpression(scaleY).ToDelegate(useScaledTForScale ? "T" : "t");
                _ = eScaleX(0);
                _ = eScaleY(0);
            }
        }

        public Vector2 GetStartPos(int set, float curLine)
        {
            if (lineSet[set].StartPosCount < 2)
                return lineSet[set].StartPos(0);
            else
            {
                if (curLine.IsBetween(1, lineSet[set].StartPosCount, true, true))
                {
                    if (Mathf.Approximately(Mathf.Round(curLine - 1), curLine - 1))
                        return lineSet[set].StartPos(Mathf.RoundToInt(curLine - 1));
                    else
                    {
                        var floored = Mathf.FloorToInt(curLine - 1);
                        var ceiled = Mathf.CeilToInt(curLine - 1);
                        var betweenValue = Mathf.InverseLerp(floored, ceiled, curLine - 1);
                        return Vector2.Lerp(lineSet[set].StartPos(floored), lineSet[set].StartPos(ceiled), betweenValue);
                    }
                }
                else
                {
                    if (curLine < 1)
                        return Vector2.LerpUnclamped(lineSet[set].StartPos(0), lineSet[set].StartPos(1), curLine - 1);
                    else
                        return Vector2.LerpUnclamped(lineSet[set].StartPos(lineSet[set].StartPosCount - 2), lineSet[set].StartPos(lineSet[set].StartPosCount - 1), curLine - lineSet[set].StartPosCount + 1);
                }
            }
        }

        public Vector2 GetStartPos(float set, float curLine)
        {
            if (Mathf.Approximately(Mathf.Round(set), set))
                return GetStartPos(Mathf.RoundToInt(set), curLine);

            var floored = Mathf.FloorToInt(set);
            var ceiled = Mathf.CeilToInt(set);
            if (lineSet.Length <= ceiled)
                return GetStartPos(floored, curLine);
            if (floored < 0)
                return GetStartPos(ceiled, curLine);
            
            var t = Mathf.InverseLerp(floored, ceiled, set);
            return Vector2.Lerp(GetStartPos(floored, curLine), GetStartPos(ceiled, curLine), t);
        }

        public Vector2 GetEndPos(int set, float curLine)
        {
            if (lineSet[set].count < 2)
                return lineSet[set].EndPos(0);
            else
            {
                if (curLine.IsBetween(1, lineSet[set].count, true, true))
                {
                    if (Mathf.Approximately(Mathf.Round(curLine - 1), curLine - 1))
                        return lineSet[set].EndPos(Mathf.RoundToInt(curLine - 1));
                    else
                    {
                        var floored = Mathf.FloorToInt(curLine - 1);
                        var ceiled = Mathf.CeilToInt(curLine - 1);
                        var betweenValue = Mathf.InverseLerp(floored, ceiled, curLine - 1);
                        return Vector2.Lerp(lineSet[set].EndPos(floored), lineSet[set].EndPos(ceiled), betweenValue);
                    }
                }
                else
                {
                    if (curLine < 1)
                        return Vector2.LerpUnclamped(lineSet[set].EndPos(0), lineSet[set].EndPos(1), curLine - 1);
                    else
                        return Vector2.LerpUnclamped(lineSet[set].EndPos(lineSet[set].count - 2), lineSet[set].EndPos(lineSet[set].count - 1), curLine - lineSet[set].count);
                }

                
            }
        }

        public Vector2 GetEndPos(float set, float curLine)
        {
            if (Mathf.Approximately(Mathf.Round(set), set))
                return GetEndPos(Mathf.RoundToInt(set), curLine);

            var floored = Mathf.FloorToInt(set);
            var ceiled = Mathf.CeilToInt(set);
            if (lineSet.Length <= ceiled)
                return GetEndPos(floored, curLine);
            if (floored < 0)
                return GetEndPos(ceiled, curLine);
            
            var t = Mathf.InverseLerp(floored, ceiled, set);
            return Vector2.Lerp(GetEndPos(floored, curLine), GetEndPos(ceiled, curLine), t);
        }

        public float GetStartTiltDegree(int set, float curLine)
        {
            var floored = Mathf.FloorToInt(curLine - 1);
            var ceiled = Mathf.CeilToInt(curLine - 1);

            if (lineSet[set].count < 2)
                return (float)lineSet[set].startPosTilt[0];
            else
            {
                if (curLine.IsBetween(1, lineSet[set].count, true, true))
                {
                    var t = Mathf.InverseLerp(floored, ceiled, curLine - 1);
                    return Mathf.Lerp((float)lineSet[set].startPosTilt[floored], (float)lineSet[set].startPosTilt[ceiled], t);
                }
                else
                {
                    if (curLine < 1)
                        return Mathf.LerpUnclamped((float)lineSet[set].startPosTilt[0], (float)lineSet[set].startPosTilt[1], curLine - 1);
                    else
                        return Mathf.LerpUnclamped((float)lineSet[set].startPosTilt[lineSet[set].count - 2], (float)lineSet[set].startPosTilt[lineSet[set].count - 1], curLine - lineSet[set].count + 1);
                }
            }
        }

        public float GetStartTiltDegree(float set, float curLine)
        {
            var floored = Mathf.FloorToInt(set);
            var ceiled = Mathf.CeilToInt(set);
            if (lineSet.Length <= ceiled)
                return GetStartTiltDegree(floored, curLine);
            if (floored < 0)
                return GetStartTiltDegree(ceiled, curLine);
            if (floored == ceiled)
                return GetStartTiltDegree(floored, curLine);
            var t = Mathf.InverseLerp(floored, ceiled, set);
            return Mathf.Lerp(GetStartTiltDegree(floored, curLine), GetStartTiltDegree(ceiled, curLine), t);
        }

        public float GetEndTiltDegree(int set, float curLine)
        {
            var floored = Mathf.FloorToInt(curLine - 1);
            var ceiled = Mathf.CeilToInt(curLine - 1);

            if (lineSet[set].count < 2)
                return (float)lineSet[set].endPosTilt[0];
            else
            {
                if (curLine.IsBetween(1, lineSet[set].count, true, true))
                {
                    var t = Mathf.InverseLerp(floored, ceiled, curLine - 1);
                    return Mathf.Lerp((float)lineSet[set].endPosTilt[floored], (float)lineSet[set].endPosTilt[ceiled], t);
                }
                else
                {
                    if (curLine < 1)
                        return Mathf.LerpUnclamped((float)lineSet[set].endPosTilt[0], (float)lineSet[set].endPosTilt[1], curLine - 1);
                    else
                        return Mathf.LerpUnclamped((float)lineSet[set].endPosTilt[lineSet[set].count - 2], (float)lineSet[set].endPosTilt[lineSet[set].count - 1], curLine - lineSet[set].count);
                }
            }
        }

        public float GetEndTiltDegree(float set, float curLine)
        {
            var floored = Mathf.FloorToInt(set);
            var ceiled = Mathf.CeilToInt(set);
            if (lineSet.Length <= ceiled)
                return GetEndTiltDegree(floored, curLine);
            if (floored < 0)
                return GetEndTiltDegree(ceiled, curLine);
            if (floored == ceiled)
                return GetEndTiltDegree(floored, curLine);
            var t = Mathf.InverseLerp(floored, ceiled, set);
            return Mathf.Lerp(GetEndTiltDegree(floored, curLine), GetEndTiltDegree(ceiled, curLine), t);
        }

        public Vector2 GetCurrentPos(float set, float progress, float startLine, float endLine)
        {
            Vector2 start = GetStartPos(set, startLine), end = GetEndPos(set, endLine);

            progress = Mathf.Max(progress, 0);
            if (useBezierPath)
            {
                Vector2 res = Vector2.zero;
                var n = bezierPathX.Length - 1;
                for (int i = 0; i <= n; i++)
                {
                    res += new Vector2(
                        (float)bezierPathX[i] * Mathf.Pow(1 - progress, n - i) * Mathf.Pow(progress, i),
                        (float)bezierPathY[i] * Mathf.Pow(1 - progress, n - i) * Mathf.Pow(progress, i));
                }
                return new Vector2(Mathf.LerpUnclamped(start.x, end.x, res.x), Mathf.LerpUnclamped(start.y, end.y, res.y));
            }
            else
            {
                //context.Variables["t"] = progress;
                //if (useScaledTForPath)
                //    context.Variables["T"] = eScaledT.Evaluate();
                //var computedX = ePathX.Evaluate();
                //var computedY = ePathY.Evaluate();
                var finalT = progress;
                if (useScaledTForPath)
                    finalT = (float)eScaledT(progress);
                var computedX = (float)ePathX(finalT);
                var computedY = (float)ePathY(finalT);
                return new Vector2(Mathf.LerpUnclamped(start.x, end.x, computedX), Mathf.LerpUnclamped(start.y, end.y, computedY));
            }
        }

        public Vector2 GetCurrentPos(float progress, Vector2 startPos, Vector2 endPos)
        {
            progress = Mathf.Max(progress, 0);
            if (useBezierPath)
            {
                Vector2 res = Vector2.zero;
                var n = bezierPathX.Length - 1;
                for (int i = 0; i <= n; i++)
                {
                    res += new Vector2(
                        (float)bezierPathX[i] * Mathf.Pow(1 - progress, n - i) * Mathf.Pow(progress, i),
                        (float)bezierPathY[i] * Mathf.Pow(1 - progress, n - i) * Mathf.Pow(progress, i));
                }
                return new Vector2(Mathf.LerpUnclamped(startPos.x, endPos.x, res.x), Mathf.LerpUnclamped(startPos.y, endPos.y, res.y));
            }
            else
            {
                //context.Variables["t"] = progress;
                //if (useScaledTForPath)
                //    context.Variables["T"] = eScaledT.Evaluate();
                //var computedX = ePathX.Evaluate();
                //var computedY = ePathY.Evaluate();
                var finalT = progress;
                if (useScaledTForPath)
                    finalT = (float)eScaledT(progress);
                var computedX = (float)ePathX(finalT);
                var computedY = (float)ePathY(finalT);
                return new Vector2(Mathf.LerpUnclamped(startPos.x, endPos.x, computedX), Mathf.LerpUnclamped(startPos.y, endPos.y, computedY));
            }
        }

        public float GetCurrentTiltDegree(float set, float progress, float startLine, float endLine)
        {
            float startTilt = GetStartTiltDegree(set, startLine), endTilt = GetEndTiltDegree(set, endLine);
            return Mathf.Lerp(startTilt, endTilt, progress);
        }

        public Vector2 GetScale(float progress)
        {
            progress = Mathf.Max(progress, 0);
            if (useBezierScale)
            {
                Vector2 res = Vector2.zero;
                var n = bezierPathX.Length;
                for (int i = 0; i <= n; i++)
                {
                    res += new Vector2(
                        (float)bezierScaleX[i] * Mathf.Pow(1 - progress, n - i) * Mathf.Pow(progress, i),
                        (float)bezierScaleY[i] * Mathf.Pow(1 - progress, n - i) * Mathf.Pow(progress, i));
                }
                return res;
            }
            else
            {
                //context.Variables["t"] = progress;
                //if (useScaledTForScale)
                //    context.Variables["T"] = eScaledT.Evaluate();
                //var computedX = eScaleX.Evaluate();
                //var computedY = eScaleY.Evaluate();
                var finalT = progress;
                if (useScaledTForScale)
                    finalT = (float)eScaledT(progress);
                var computedX = (float)eScaleX(finalT);
                var computedY = (float)eScaleY(finalT);
                return new Vector2(computedX, computedY);
            }
        }
    }

    [System.Serializable]
    public class TSModeLineData
    {
        public int StartPosCount { get { return startPosX.Length; } }

        public int count;
        public double[] startPosX;
        public double[] startPosY;
        public double[] startPosTilt;
        public double[] endPosX;
        public double[] endPosY;
        public double[] endPosTilt;

        public Vector2 StartPos(int lineIndex)
        {
            return new Vector2((float)startPosX[lineIndex], (float)startPosY[lineIndex]);
        }

        public Vector2 EndPos(int lineIndex)
        {
            return new Vector2((float)endPosX[lineIndex], (float)endPosY[lineIndex]);
        }
    }

    [System.Serializable]
    public class TSModePathData
    {
        public int method;
        public bool useScaledT;
        public double[] controlKeyT;
        public double[] controlKeyF;
    }
}