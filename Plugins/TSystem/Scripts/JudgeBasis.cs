using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TSystem
{
    public class JudgeBasis : MonoBehaviour
    {
        public IngameBasis Game { get { return IngameBasis.Now; } }

        public Rect TouchRect { get { return new Rect(
            (float)Game.Mode.borderLeft, (float)Game.Mode.borderDown, (float)(Game.Mode.borderRight - Game.Mode.borderLeft), (float)(Game.Mode.borderUp - Game.Mode.borderDown)); } }

        public Text scoreText, percenText;
        public JudgeTextAnimator judgeText;
        public JudgeComboAnimator comboText;
        public SoundEffectPlayer soundPlayer;

        [HideInInspector] public int score;
        [HideInInspector] public int comboCount;
        [HideInInspector] public float percentage;
        [System.NonSerialized] public Dictionary<float, List<int>> noteQueue = new Dictionary<float, List<int>>();

        protected Dictionary<JudgeType, int> judgeResult = new Dictionary<JudgeType, int>();
        protected int maxCombo = 0;

        public void AddLine(float line)
        {
            noteQueue.Add(line, new List<int>());
        }

        private void Update()
        {
            scoreText.text = score.ToString();
            percenText.text = percentage.ToString("N2") + "%";
        }

        public virtual JudgeType GetJudge(float line, NoteType type, FlickType flick, float deltaTime)
        {
            deltaTime = Mathf.Abs(deltaTime);

            if(flick != FlickType.NotFlick || type.IsEither(NoteType.HoldStart, NoteType.HoldEnd, NoteType.SlideStart, NoteType.SlideMiddle, NoteType.SlideEnd))
            {
                for(int i = 0; i < Game.Mode.judgeHFSThreshold.Length; i++)
                {
                    if (i == 0 && deltaTime <= Game.Mode.judgeHFSThreshold[i])
                        return JudgeType.Fantastic;
                    else if (i != 0 && deltaTime.IsBetween((float)Game.Mode.judgeHFSThreshold[i - 1], (float)Game.Mode.judgeHFSThreshold[i], false, true))
                        return (JudgeType)(6 - i);
                }
            }
            else
            {
                for (int i = 0; i < Game.Mode.judgeThreshold.Length; i++)
                {
                    if (i == 0 && deltaTime <= Game.Mode.judgeThreshold[i])
                        return JudgeType.Fantastic;
                    else if (i != 0 && deltaTime.IsBetween((float)Game.Mode.judgeThreshold[i - 1], (float)Game.Mode.judgeThreshold[i], false, true))
                        return (JudgeType)(5 - i);
                }
            }
            return JudgeType.NotJudged;
        }

        public virtual void UpdateJudgeResult(int line, JudgeType result, bool flick, bool silent)
        {
            if (!judgeResult.ContainsKey(result))
                judgeResult.Add(result, 1);
            else
                judgeResult[result]++;

            if(!silent)
            {
                if (result.IsEither(JudgeType.Great, JudgeType.Perfect, JudgeType.Fantastic))
                {
                    comboCount++;
                    maxCombo = Mathf.Max(maxCombo, comboCount);
                    Game.PlayHitEffect(line);
                    if (TSystemConfig.Now.allowSoundEffect)
                        soundPlayer.PlayHitSound(true, flick);
                }
                else
                {
                    comboCount = 0;
                    if (result != JudgeType.Miss)
                        Game.PlayHitEffect(line);
                    if (TSystemConfig.Now.allowSoundEffect)
                        soundPlayer.PlayHitSound(false, flick);
                }
                judgeText.Show(result);
                comboText.Show(comboCount);
            }
        }
    }
}