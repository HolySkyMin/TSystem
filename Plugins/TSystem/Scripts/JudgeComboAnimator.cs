using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TSystem
{
    public class JudgeComboAnimator : MonoBehaviour
    {
        public RectTransform body;
        public Text text;

        protected float t;
        protected bool isAnimating;

        protected virtual void Update()
        {
            if (isAnimating)
            {
                if (t < 0.2f)
                {
                    body.localScale = Vector3.one * (Mathf.Min(0.5f + t * 5, 1));
                    t += Time.deltaTime;
                }
                else
                {
                    body.localScale = Vector3.one;
                    isAnimating = false;
                }
            }
        }

        public virtual void Show(int n)
        {
            if (n >= 2)
            {
                body.gameObject.SetActive(true);
                text.text = n.ToString();
                isAnimating = true;
                t = 0;
            }
            else
                body.gameObject.SetActive(false);
        }
    }
}