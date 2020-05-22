using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TSystem.Example
{
    public class TouchChecker : MonoBehaviour
    {
        public Text firstTouchText, secondTouchText;

        // Update is called once per frame
        void Update()
        {
            foreach (var touch in Input.touches)
            {
                var pos = IngameBasis.Now.GetTouchPos(touch.position);
                if (touch.fingerId == 0)
                    firstTouchText.text = pos.ToString();
            }
        }
    }

}