using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TSystem
{
    public class LineObject : MonoBehaviour
    {
        public IngameBasis Game { get { return IngameBasis.Now; } }

        public int lineIndex;
        public GameObject hitEffect;

        protected float hitEffectTimeCount;
        protected bool isHitPlaying;
        protected Vector2 originPos, originScale;

        protected virtual void Start()
        {
            originPos = GetComponent<RectTransform>().anchoredPosition;
            isHitPlaying = false;
            Game.specialEnterAnim += SpecialEnterAnim;
            Game.specialLeaveAnim += SpecialLeaveAnim;
        }

        protected virtual void Update()
        {
            if(isHitPlaying)
            {
                if(hitEffectTimeCount >= 0.25f)
                {
                    hitEffect.SetActive(false);
                    isHitPlaying = false;
                }
                hitEffect.GetComponent<RectTransform>().localScale = Vector3.one * Mathf.Min(1 + hitEffectTimeCount * 8, 2);
                hitEffect.GetComponent<Graphic>().color = new Color(1, 1, 1, Mathf.Min(1, 2 - hitEffectTimeCount * 8));
                hitEffectTimeCount += Time.deltaTime;
            }
        }

        public void SpecialEnterAnim()
        {
            StartCoroutine(SpecialEnterAnimRoutine());
        }

        public void SpecialLeaveAnim()
        {
            StartCoroutine(SpecialLeaveAnimRoutine());
        }

        public virtual void PlayHitEffect()
        {
            hitEffect.SetActive(true);
            isHitPlaying = true;
            hitEffectTimeCount = 0;
        }

        protected virtual IEnumerator SpecialEnterAnimRoutine()
        {
            TSystemStatic.Log("Special enter Animation per line object is not implemented in this basis.");
            yield return null;
        }

        protected virtual IEnumerator SpecialLeaveAnimRoutine()
        {
            TSystemStatic.Log("Special leave Animation per line object is not implemented in this basis.");
            yield return null;
        }
    }
}