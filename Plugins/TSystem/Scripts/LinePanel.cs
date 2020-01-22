using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    public class LinePanel : MonoBehaviour
    {
        public GameObject lineObjTemplate, specialObjTemplate;
        public CanvasGroup linePanel, specialPanel;
        public RectTransform lineParent, specialParent;
        public LineObject specialLineObj;

        protected Vector2 middlePos;
        List<LineObject> lineObjs = new List<LineObject>();

        public LineObject this[int index]
        {
            get { return lineObjs[index]; }
        }

        public void Set(int lineSet)
        {
            var lineInfo = IngameBasis.Now.Mode.lineSet[lineSet];

            for(int i = 0; i < lineInfo.count; i++)
            {
                var newObj = Instantiate(lineObjTemplate);
                newObj.GetComponent<RectTransform>().SetParent(lineParent);
                newObj.GetComponent<RectTransform>().anchoredPosition = lineInfo.EndPos(i);
                newObj.GetComponent<RectTransform>().localScale = Vector3.one;
                newObj.SetActive(true);
                lineObjs.Add(newObj.GetComponent<LineObject>());
            }

            middlePos = IngameBasis.Now.Mode.GetEndPos(lineSet, (lineInfo.count + 1) / 2f);
            var specialObj = Instantiate(specialObjTemplate);
            specialObj.GetComponent<RectTransform>().SetParent(specialParent);
            specialObj.GetComponent<RectTransform>().anchoredPosition = middlePos;
            specialObj.GetComponent<RectTransform>().localScale = Vector3.one;
            specialObj.SetActive(true);
            specialLineObj = specialObj.GetComponent<LineObject>();

            linePanel.alpha = 1;
            specialPanel.alpha = 0;
        }

        public virtual void SwitchToSpecial(float duration)
        {
            StartCoroutine(SwitchingAnim(duration, false));
        }

        public virtual void SwitchToLine(float duration)
        {
            StartCoroutine(SwitchingAnim(duration, false));
        }

        IEnumerator SwitchingAnim(float duration, bool reversed)
        {
            float t = 0;
            while(t < duration)
            {
                if(reversed)
                {
                    linePanel.alpha = t / duration;
                    specialPanel.alpha = 1 - t / duration;
                }
                else
                {
                    linePanel.alpha = 1 - t / duration;
                    specialPanel.alpha = t / duration;
                }
                t += Time.deltaTime;
                yield return null;
            }
        }
    }
}