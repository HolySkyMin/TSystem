using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TSystem
{
    public class JudgeTextAnimator : MonoBehaviour
    {
        public Text text;

        protected float t;
        protected bool isAnimating;

        protected virtual void Update()
        {
            if(isAnimating)
            {
                if(t < 0.7f)
                {
                    text.rectTransform.localScale = Vector3.one * Mathf.Min(0.5f + t * 5, 1);
                    t += Time.deltaTime;
                }
                else
                {
                    text.gameObject.SetActive(false);
                    isAnimating = false;
                }
            }
        }

        public virtual void Show(JudgeType type)
        {
            switch(type)
            {
                case JudgeType.Miss:
                    text.text = "MISS";
                    break;
                case JudgeType.Bad:
                    text.text = "BAD";
                    break;
                case JudgeType.Nice:
                    text.text = "NICE";
                    break;
                case JudgeType.Great:
                    text.text = "GREAT";
                    break;
                case JudgeType.Perfect:
                    text.text = "PERFECT";
                    break;
                case JudgeType.Fantastic:
                    text.text = "FANTASTIC";
                    break;
            }
            text.gameObject.SetActive(true);
            isAnimating = true;
            t = 0;
        }
    }
}