using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LitJson;

namespace TSystem
{
    public class BeatmapParser : MonoBehaviour
    {
        public IngameBasis Game { get { return IngameBasis.Now; } }

        public void ParseStructuredBeatmap(string path, BeatmapType type, out bool succeed)
        {
            string rawData;
            StreamReader stream;

            if (Game.Packet.loadFromResources)
            {
                var beatmapFile = Resources.Load<TextAsset>(path);
                rawData = beatmapFile.text;
            }
            else
            {
                try
                {
                    stream = new StreamReader(path);
                    rawData = stream.ReadToEnd();
                    stream.Close();
                }
                catch (Exception e)
                {
                    TSystemStatic.LogWithException("Beatmap file does not exists.", e);
                    succeed = false;
                    return;
                }
            }

            switch(type)
            {
                case BeatmapType.TWx:
                    succeed = ParseTWx(rawData);
                    break;
                case BeatmapType.SSTrain:
                    succeed = ParseSSTrain(rawData);
                    break;
                default:
                    succeed = false;
                    break;
            }
        }

        public bool ParseSSTrain(string rawData)
        {
            bool initNoteUsed = false;
            float initNotePos;

            try
            {
                var data = JsonMapper.ToObject<SSTrainData>(rawData);

                initNotePos = TSystemConfig.Now.gameSync;
                for (int i = 0; i < data.notes.Length; i++)
                {
                    if (!initNoteUsed && initNotePos <= data.notes[i].timing)
                    {
                        Game.CreateNote(new NoteData
                            (0, 0, initNotePos, Game.NoteSpeed, 0, 0, NoteType.Starter, FlickType.NotFlick, Color.white, new List<int>()));
                        initNoteUsed = true;
                    }
                    var noteData = new NoteData(data.notes[i]);
                    // 예외적 허용: 롱 노트 끝부분 슬라이드를 종류 1번으로 처리한 채보가 존재함. 그 때 값을 2로 수정. 그리고 롱 노트 끝부분의 경우 시작지점 통일
                    if (data.notes[i].prevNoteId > 0 && Game.notes[data.notes[i].prevNoteId].Type == NoteType.HoldStart)
                    {
                        noteData.type = NoteType.HoldEnd;
                        noteData.startLine = Game.notes[data.notes[i].prevNoteId].StartLine;
                    }
                    Game.CreateNote(noteData);
                }
                return true;
            }
            catch (Exception e)
            {
                TSystemStatic.LogWithException("Failed to parse SSTrain type beatmap.", e);
                return false;
            }
        }

        public bool ParseTWx(string rawData)
        {
            bool initNoteUsed = false;
            
            try
            {
                var data = JsonMapper.ToObject<TWxData>(rawData);

                for(int i = 0; i < data.notes.Length; i++)
                {
                    if(!initNoteUsed && TSystemConfig.Now.gameSync <= data.notes[i].Time)
                    {
                        Game.CreateNote(new NoteData
                            (0, 0, TSystemConfig.Now.gameSync, Game.NoteSpeed, 0, 0, NoteType.Starter, FlickType.NotFlick, Color.white, new List<int>()));
                        initNoteUsed = true;
                    }
                    var noteData = new NoteData(data.notes[i]);
                    Game.CreateNote(noteData);
                }
                return true;
            }
            catch(Exception e)
            {
                TSystemStatic.LogWithException("Failed to parse TWx type beatmap.", e);
                return false;
            }
        }

        public void ParseDeleste(string path, out bool succeed)
        {
            FileStream file = null;
            StreamReader scanner = null;
            try
            {
                file = new FileStream(path, FileMode.Open, FileAccess.Read);
                scanner = new StreamReader(file, System.Text.Encoding.UTF8);
            }
            catch(Exception e)
            {
                TSystemStatic.LogWithException("Beatmap file does not exists.", e);
                succeed = false;
                return;
            }

            int d, MaxBlockNum = -1, ID = 1;
            char[] div = { ' ', ',', ':' };
            DelesteGlobalData StaticData = new DelesteGlobalData();
            Dictionary<int, DelesteBlockData> Blocks = new Dictionary<int, DelesteBlockData>();
            double Time = 0, Speed = 1.0;
            byte[] Color = new byte[] { 255, 255, 255, 255 };

            var SongTime = TSystemConfig.Now.gameSync;

            // Steadily, stacks the data.
            while (scanner.Peek() != -1)
            {
                try
                {
                    string dataLine = scanner.ReadLine();
                    string[] data = dataLine.Split(div, StringSplitOptions.RemoveEmptyEntries);

                    if (data.Length > 1 && data[0].StartsWith("#") && (data[0].Substring(1).ToUpper().Equals("BPM") || data[0].Substring(1).ToUpper().Equals("TEMPO")))
                    {
                        StaticData.CurrentBPM = double.Parse(data[1]);
                    }
                    else if (data.Length > 1 && data[0].StartsWith("#") && data[0].Substring(1).ToUpper().Equals("OFFSET"))
                    {
                        Time += (int.Parse(data[1]) / 1000d);
                    }
                    else if (data.Length > 1 && data[0].StartsWith("#") && (data[0].Substring(1).ToUpper().Equals("SONGOFFSET") || data[0].Substring(1).ToUpper().Equals("MUSICOFFSET") || data[0].Substring(1).ToUpper().Equals("BGMOFFSET")))
                    {
                        Time -= (int.Parse(data[1]) / 1000d);
                    }
                    else if (data.Length > 1 && data[0].StartsWith("#") && data[0].Substring(1).ToUpper().Equals("ATTRIBUTE"))
                    {
                        if (data[1].ToUpper().Equals("CUTE") || data[1].ToUpper().Equals("CU") || data[1].Equals("1")) { Color = new byte[] { 255, 100, 200, 255 }; }
                        else if (data[1].ToUpper().Equals("COOL") || data[1].ToUpper().Equals("CO") || data[1].Equals("2")) { Color = new byte[] { 85, 135, 255, 255 }; }
                        else if (data[1].ToUpper().Equals("PASSION") || data[1].ToUpper().Equals("PA") || data[1].Equals("3")) { Color = new byte[] { 255, 220, 50, 255 }; }
                        else if (data[1].ToUpper().Equals("ALL") || data[1].Equals("4")) { Color = new byte[] { 230, 255, 255, 255 }; }
                    }
                    else if (data.Length > 2 && data[0].StartsWith("#") && (data[0].Substring(1).ToUpper().Equals("MEASURE") || data[0].Substring(1).ToUpper().Equals("MEAS") || data[0].Substring(1).ToUpper().Equals("MEA")))
                    {
                        if (data[2].Contains("/"))
                        {
                            string[] numbers = data[2].Split(new char[] { '/' });
                            StaticData.Measure.Add(double.Parse(numbers[0]) / double.Parse(numbers[1]));
                        }
                        else { StaticData.Measure.Add(double.Parse(data[2])); }
                        StaticData.MeasurePos.Add(double.Parse(data[1]));
                    }
                    else if (data.Length > 2 && (data[0].StartsWith("#") && (data[0].Substring(1).ToUpper().Equals("CHANGEBPM") || data[0].Substring(1).ToUpper().Equals("CHANGETEMPO"))))
                    {
                        StaticData.ChangeBPM.Add(double.Parse(data[2]));
                        StaticData.ChangeBPMPos.Add(double.Parse(data[1]));
                    }
                    else if (data.Length > 1 && data[0].StartsWith("#") && data[0].Substring(1).ToUpper().Equals("CHANGEATTRIBUTE"))
                    {
                        if (data[1].ToUpper().Equals("CUTE") || data[1].ToUpper().Equals("CU") || data[1].Equals("1")) { Color = new byte[] { 255, 100, 200, 255 }; }
                        else if (data[1].ToUpper().Equals("COOL") || data[1].ToUpper().Equals("CO") || data[1].Equals("2")) { Color = new byte[] { 85, 135, 255, 255 }; }
                        else if (data[1].ToUpper().Equals("PASSION") || data[1].ToUpper().Equals("PA") || data[1].Equals("3")) { Color = new byte[] { 255, 220, 50, 255 }; }
                        else if (data[1].ToUpper().Equals("ALL") || data[1].Equals("4")) { Color = new byte[] { 230, 255, 255, 255 }; }
                    }
                    else if (data.Length > 1 && data[0].StartsWith("#") && (data[0].Substring(1).ToUpper().Equals("HISPEED") || data[0].Substring(1).ToUpper().Equals("HS")))
                    {
                        Speed = double.Parse(data[1]);
                        if (Speed <= 0) 
                            throw new Exception("TSystem does not support HiSpeed zero and below."); 
                    }
                    else if (data.Length > 2 && data[0].StartsWith("#") && data[0].Substring(1).ToUpper().Equals("HS2"))
                    {
                        //if (double.Parse(data[2]) <= 0)
                        //    throw new Exception("TSystem does not support HiSpeed zero and below."));
                        //StaticData.HS2.Add(double.Parse(data[2]));
                        //StaticData.HS2Pos.Add(double.Parse(data[1]));
                        TSystemStatic.Log("While parsing Deleste file: TSystem does not support HS2.");
                    }
                    else if (data.Length > 2 && data[0].StartsWith("#") && data[0].Substring(1).ToUpper().Equals("DELAY"))
                    {
                        StaticData.Delay.Add(double.Parse(data[2]));
                        StaticData.DelayPos.Add(double.Parse(data[1]));
                    }
                    else if (data.Length > 3 && data[0].StartsWith("#") && data[0].Substring(1).ToUpper().Equals("SCROLL"))
                    {
                        StaticData.Scroll.Add(new double[2] { double.Parse(data[2]) / 1000d, double.Parse(data[3]) / 1000d });
                        StaticData.ScrollPos.Add(double.Parse(data[1]));
                    }
                    else if (data.Length > 0 && data[0].StartsWith("#") && int.TryParse(data[0].Substring(1, 1), out d))
                    {
                        int CurBlock = int.Parse(data[1]);
                        if (!Blocks.ContainsKey(CurBlock)) { Blocks.Add(CurBlock, new DelesteBlockData(CurBlock)); }
                        Blocks[CurBlock].DataLines.Add(dataLine);
                        Blocks[CurBlock].Channel.Add(int.Parse(data[0].Substring(1)));
                        Blocks[CurBlock].Color.Add(Color);
                        Blocks[CurBlock].Speed.Add(Speed * Game.FixedNoteSpeed);

                        if (MaxBlockNum < CurBlock) { MaxBlockNum = CurBlock; }
                    }
                }
                catch (Exception e)
                {
                    TSystemStatic.LogWithException("Failed to parse Deleste type beatmap.", e);
                    scanner.Close();
                    file.Close();
                    succeed = false;
                    return;
                }
            }
            scanner.Close();
            file.Close();

            // Creates note at here.
            Game.CreateNote(new NoteData(0, 0, SongTime, (float)Speed * Game.FixedNoteSpeed, 0, 0, NoteType.Starter, FlickType.NotFlick, new Color32(255, 255, 255, 255), new List<int>()));
            for (int i = 0; i <= MaxBlockNum; i++)
            {
                if (Blocks.ContainsKey(i))
                {
                    Blocks[i].ParseBlock(Game, ref ID, ref Time, ref StaticData);
                }
                else { Time += ((240 / StaticData.CurrentBPM) * StaticData.BeatMultiplier); }
            }

            succeed = true;
        }
    }
}