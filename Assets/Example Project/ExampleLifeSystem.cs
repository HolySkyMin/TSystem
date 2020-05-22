using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TSystem.Example
{
    public class ExampleLifeSystem : MonoBehaviour
    {
        public int life = 100;
        public int maxLife = 100;
        public Slider lifeSlider;

        // Start is called before the first frame update
        void Start()
        {
            IngameBasis.Now.judge.customJudgeAction = UpdateLife;

            lifeSlider.maxValue = maxLife;
            lifeSlider.value = life;
        }

        private void Update()
        {
            lifeSlider.value = life;
        }

        public void UpdateLife(JudgeType result, bool isFlick)
        {
            switch(result)
            {
                case JudgeType.Bad:
                    life -= isFlick ? 3 : 5;
                    break;
                case JudgeType.Miss:
                    life -= isFlick ? 5 : 10;
                    break;
            }

            if (life <= 0)
                ForceEndGame();
        }

        public void ForceEndGame()
        {
            IngameBasis.Now.EndGame();
        }
    }
}