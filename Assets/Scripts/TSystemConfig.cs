using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TSystem
{
    public class TSystemConfig
    {
        public static TSystemConfig Now { get; set; }
        public static bool FileExists { get { return File.Exists($"{Application.persistentDataPath}/tsysconfig"); } }

        public int flickJudgeState; // 선택된 플릭 판정의 상태를 나타냅니다; 0: sensitive, 1: normal, 2: strict, 3: dynamic.
        public float noteSpeed; // 게임에서 노트가 이동하는 속도 배율값을 나타냅니다.
        public float gameSync; // 노트의 시간과 음원 파일의 시간의 시작 차이를 조정합니다.
        public float musicVolume; // 음원의 볼륨 값입니다.
        public float effectVolume; // 사운드 이펙트의 볼륨 값입니다.
        public bool allowSoundEffect; // 사운드 이펙트 재생을 허용할지에 대한 값입니다.
        public bool colorNote; // 게임에서 노트 색깔을 적용할지에 대한 값입니다.
        public bool enableBgaSound;

        public TSystemConfig()
        {
            flickJudgeState = 1;
            noteSpeed = 1;
            gameSync = 0;
            musicVolume = 1;
            effectVolume = 1;
            allowSoundEffect = true;
            colorNote = false;
            enableBgaSound = false;
        }

        public static void Load()
        {
            var reader = new StreamReader($"{Application.persistentDataPath}/tsysconfig");
            var res = JsonUtility.FromJson<TSystemConfig>(reader.ReadToEnd());
            reader.Close();
            Now = res;
        }

        public static void Save()
        {
            var jt = JsonUtility.ToJson(Now);
            var writer = new StreamWriter($"{Application.persistentDataPath}/tsysconfig");
            writer.Write(jt);
            writer.Close();
        }
    }
}