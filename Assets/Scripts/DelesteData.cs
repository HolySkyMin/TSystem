using System;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    public class DelesteGlobalData
    {
        public double CurrentBPM { get; set; }
        public double SpeedMultiplier { get; set; }
        public double BeatMultiplier { get; set; }
        public double EndScrollTime { get; set; }
        public bool IsScrollModified { get; set; }

        public List<double> ChangeBPM = new List<double>(), ChangeBPMPos = new List<double>();
        public List<double> Measure = new List<double>(), MeasurePos = new List<double>();
        public List<double> HS2 = new List<double>(), HS2Pos = new List<double>();
        public List<double> Delay = new List<double>(), DelayPos = new List<double>();
        public List<double[]> Scroll = new List<double[]>(2);
        public List<double> ScrollPos = new List<double>();
        public Dictionary<int, List<double[]>> ConnectorPrev = new Dictionary<int, List<double[]>>(3);
        public Dictionary<int, double> BeforeTime = new Dictionary<int, double>();
        public Dictionary<int, int> TailPrev = new Dictionary<int, int>();
        public Dictionary<double, int> HoldPrev = new Dictionary<double, int>(), BeforeFlickData = new Dictionary<double, int>();

        public DelesteGlobalData()
        {
            BeatMultiplier = 1;
            SpeedMultiplier = 1;
            EndScrollTime = 0;
            IsScrollModified = false;
        }
    }

    public class DelesteBlockData
    {
        public List<string> DataLines { get; set; }
        public List<int> Channel { get; set; }
        public List<byte[]> Color { get; set; }
        public List<double> Speed { get; set; }
        public int Measure { get; set; }
        public int BeatPer4 { get; set; }
        public double BPM { get; set; }
        public bool IsAdded { get; set; }

        public DelesteBlockData(int BlockID)
        {
            DataLines = new List<string>();
            Channel = new List<int>();
            Color = new List<byte[]>(4);
            Speed = new List<double>();
            IsAdded = false;
            BeatPer4 = 4;
            Measure = BlockID;
        }

        public void ParseBlock(IngameBasis Game, ref int ID, ref double Time, ref DelesteGlobalData StaticData)
        {
            int MaxBit = 1;
            List<int> DataIndex = new List<int>(), StartIndex = new List<int>(), EndIndex = new List<int>();
            List<string[]> Datas = new List<string[]>();

            // DataIndex, StartIndex, EndIndex의 초기화와 MaxBit의 갱신, 그리고 Datas의 값 대입을 수행합니다.
            for(int i = 0; i < DataLines.Count; i++)
            {
                Datas.Add(DataLines[i].Split(new char[] { ':' }));
                DataIndex.Add(-1);
                StartIndex.Add(0);
                EndIndex.Add(0);
                MaxBit = LCM(MaxBit, Datas[i][1].Length);
            }

            // MaxBit를 기반으로, 차례차례 데이터를 해석하고 추가하는 전체 과정입니다.
            for(int i = 0; i < MaxBit; i++)
            {
                // 선제적으로, 각종 큐들을 살펴보면서 조건을 만족한다면 데이터를 그에 맞춥니다.
                if(StaticData.Measure.Count > 0 && StaticData.MeasurePos[0] <= Measure + (i / (double)MaxBit))
                {
                    StaticData.BeatMultiplier = StaticData.Measure[0];
                    StaticData.Measure.RemoveAt(0);
                    StaticData.MeasurePos.RemoveAt(0);
                }
                if(StaticData.ChangeBPM.Count > 0 && StaticData.ChangeBPMPos[0] <= Measure + (i / (double)MaxBit))
                {
                    StaticData.CurrentBPM = StaticData.ChangeBPM[0];
                    StaticData.ChangeBPM.RemoveAt(0);
                    StaticData.ChangeBPMPos.RemoveAt(0);
                }
                if(StaticData.HS2.Count > 0 && StaticData.HS2Pos[0] <= Measure + (i / (double)MaxBit))
                {
                    StaticData.SpeedMultiplier = StaticData.HS2[0];
                    StaticData.HS2.RemoveAt(0);
                    StaticData.HS2Pos.RemoveAt(0);
                }
                if(StaticData.Delay.Count > 0 && StaticData.DelayPos[0] <= Measure + (i / (double)MaxBit))
                {
                    Time += StaticData.Delay[0];
                    StaticData.Delay.RemoveAt(0);
                    StaticData.DelayPos.RemoveAt(0);
                }
                if(StaticData.Scroll.Count > 0 && StaticData.ScrollPos[0] <= Measure + (i / (double)MaxBit))
                {
                    Game.CreateNote(new NoteData(ID++, 0, (float)Time, (float)StaticData.Scroll[0][0], 0, 0, NoteType.Scroller, FlickType.NotFlick, new Color32(255, 255, 255, 0), new List<int>()));
                    StaticData.EndScrollTime = Time + StaticData.Scroll[0][1];
                    StaticData.IsScrollModified = true;
                    StaticData.Scroll.RemoveAt(0);
                    StaticData.ScrollPos.RemoveAt(0);
                }
                if(StaticData.EndScrollTime <= Time && StaticData.IsScrollModified)
                {
                    Game.CreateNote(new NoteData(ID++, 0, (float)Time, 1, 0, 0, NoteType.Scroller, FlickType.NotFlick, new Color32(255, 255, 255, 0), new List<int>()));
                    StaticData.IsScrollModified = false;
                }

                // Data를 병렬적으로 처리합니다.
                for (int j = 0; j < Datas.Count; j++)
                {
                    // 만약 현재의 i 지점에 해당하는 해당 데이터의 계산된 인덱스가 기존의 인덱스보다 크다면 값을 그에 맞춥니다.
                    if (i / (MaxBit / Datas[j][1].Length) > DataIndex[j]) { DataIndex[j]++; }
                    else { continue; }

                    // 데이터를 가져옵니다. 코드 간략화를 위함입니다.
                    char Command = Datas[j][1][DataIndex[j]];

                    // 필요한 개별 변수를 설정합니다. (1차)
                    NoteType Mode;
                    FlickType Flick;

                    // 노트의 기본 정보를 해석합니다.
                    if (Command.Equals('1') || char.ToUpper(Command).Equals('L'))
                    {
                        Mode = NoteType.Tap;
                        Flick = FlickType.Left;
                    }
                    else if (Command.Equals('2') || char.ToUpper(Command).Equals('T'))
                    {
                        Mode = NoteType.Tap;
                        Flick = FlickType.NotFlick;
                    }
                    else if (Command.Equals('3') || char.ToUpper(Command).Equals('R'))
                    {
                        Mode = NoteType.Tap;
                        Flick = FlickType.Right;
                    }
                    else if (Command.Equals('4') || char.ToUpper(Command).Equals('H'))
                    {
                        Mode = NoteType.HoldStart;
                        Flick = FlickType.NotFlick;
                    }
                    else if (Command.Equals('5') || char.ToUpper(Command).Equals('S'))
                    {
                        Mode = NoteType.SlideStart;
                        Flick = FlickType.NotFlick;
                    }
                    else { continue; }

                    // 필요한 개별 변수를 설정합니다. (2차)
                    double Start = 3, End = 3;

                    // 시작 지점과 끝 지점을 설정합니다.
                    // 위 코드의 continue 때문에, 유효한 노트 데이터가 해석이 되어야 이 부분부터의 코드에 도달합니다.
                    if (Datas[j].Length >= 3)
                    {
                        int DummyStart;
                        char StartText = Datas[j][2][StartIndex[j]];
                        if (int.TryParse(StartText.ToString(), out DummyStart)) { Start = DummyStart; }
                        else
                        {
                            if (char.ToUpper(StartText).Equals('A')) { Start = 1.5; }
                            else if (char.ToUpper(StartText).Equals('B')) { Start = 2.5; }
                            else if (char.ToUpper(StartText).Equals('C')) { Start = 3.5; }
                            else if (char.ToUpper(StartText).Equals('D')) { Start = 4.5; }
                            else 
                                throw new Exception("While parsing Deleste file: Error in start point parsing.");
                        }
                        StartIndex[j]++;
                        if (Datas[j].Length.Equals(3)) { End = Start; }
                    }
                    if(Datas[j].Length >= 4)
                    {
                        int DummyEnd;
                        char EndText = Datas[j][3][EndIndex[j]];
                        if (int.TryParse(EndText.ToString(), out DummyEnd))
                            End = DummyEnd;
                        else
                        {
                            if (char.ToUpper(EndText).Equals('A')) { End = 1.5; }
                            else if (char.ToUpper(EndText).Equals('B')) { End = 2.5; }
                            else if (char.ToUpper(EndText).Equals('C')) { End = 3.5; }
                            else if (char.ToUpper(EndText).Equals('D')) { End = 4.5; }
                            else 
                                throw new Exception("While parsing Deleste file: Error in end point parsing.");
                        }
                        EndIndex[j]++;
                    }

                    // 필요한 개별 변수를 설정합니다. (3차)
                    List<int> Prevs = new List<int>();
                    if (!StaticData.HoldPrev.ContainsKey(End)) { StaticData.HoldPrev.Add(End, 0); }
                    if (!StaticData.TailPrev.ContainsKey(Channel[j])) { StaticData.TailPrev.Add(Channel[j], 0); }
                    if (!StaticData.ConnectorPrev.ContainsKey(Channel[j])) { StaticData.ConnectorPrev.Add(Channel[j], new List<double[]>(3)); }
                    if (!StaticData.BeforeTime.ContainsKey(Channel[j])) { StaticData.BeforeTime.Add(Channel[j], 0); }

                    // 연결할 이전 노트가 있으면 연결합니다. Tail, Connector 순서로 연결합니다.

                    // Tail은 홀드 노트를 우선으로 합니다 (추후 변경 가능). 중복은 허용하지 않습니다. 
                    if (Mode.Equals(NoteType.Tap) && StaticData.HoldPrev[End] > 0)
                    {
                        Mode = NoteType.HoldEnd;
                        Prevs.Add(StaticData.HoldPrev[End]);
                        StaticData.HoldPrev[End] = 0;
                    }
                    else if(Mode.Equals(NoteType.Tap) && StaticData.TailPrev[Channel[j]] > 0)
                    {
                        Mode = NoteType.SlideEnd;
                        Prevs.Add(StaticData.TailPrev[Channel[j]]);
                        StaticData.TailPrev[Channel[j]] = 0;
                    }
                    if(Mode.Equals(NoteType.SlideStart) && StaticData.TailPrev[Channel[j]] > 0)
                    {
                        Mode = NoteType.SlideMiddle;
                        Prevs.Add(StaticData.TailPrev[Channel[j]]);
                    }

                    // Connector를 연결합니다. 
                    // ConnectorPrev[Channel[j]][x]에서 x=0의 값은 방향(증가 방향이 양수), x=1의 값은 비교 지점, x=2의 값은 노트 ID입니다.
                    if (!Flick.Equals(FlickType.NotFlick) && StaticData.ConnectorPrev[Channel[j]].Count > 0)
                    {
                        for (int k = 0; k < StaticData.ConnectorPrev[Channel[j]].Count; k++)
                        {
                            if (Time - StaticData.BeforeTime[Channel[j]] > 0 && Time - StaticData.BeforeTime[Channel[j]] <= 1)
                            {
                                if (StaticData.ConnectorPrev[Channel[j]][k][0] > 0)
                                {
                                    if (End > StaticData.ConnectorPrev[Channel[j]][k][1])
                                    {
                                        Prevs.Add((int)StaticData.ConnectorPrev[Channel[j]][k][2]);
                                    }
                                }
                                else if (StaticData.ConnectorPrev[Channel[j]][k][0] < 0)
                                {
                                    if (End < StaticData.ConnectorPrev[Channel[j]][k][1])
                                    {
                                        Prevs.Add((int)StaticData.ConnectorPrev[Channel[j]][k][2]);
                                    }
                                }
                                else { Prevs.Add((int)StaticData.ConnectorPrev[Channel[j]][k][2]); }
                            }
                        }
                        if (StaticData.BeforeTime[Channel[j]] != Time)
                        {
                            StaticData.ConnectorPrev[Channel[j]].Clear();
                        }
                    }
                    else if (Flick.Equals(FlickType.NotFlick) && StaticData.ConnectorPrev[Channel[j]].Count > 0 && StaticData.BeforeTime[Channel[j]] != Time) { StaticData.ConnectorPrev[Channel[j]].Clear(); }

                    // 다음 노트와 연결될 노트에 대한 데이터를 전역 데이터에 입력합니다.
                    if (Mode.Equals(NoteType.HoldStart)) { StaticData.HoldPrev[End] = ID; }
                    else if (Mode.Equals(NoteType.SlideStart) || Mode.Equals(NoteType.SlideMiddle)) { StaticData.TailPrev[Channel[j]] = ID; }
                    if(!Flick.Equals(FlickType.NotFlick))
                    {
                        if (Channel[j] % 4 == 0 || Channel[j] % 4 == 1)
                        {
                            if (Flick.Equals(FlickType.Left)) { StaticData.ConnectorPrev[Channel[j]].Add(new double[3] { -1, End, ID }); }
                            else if (Flick.Equals(FlickType.Right)) { StaticData.ConnectorPrev[Channel[j]].Add(new double[3] { 1, End, ID }); }
                        }
                        else if (Channel[j] % 4 == 2 || Channel[j] % 4 == 3) { StaticData.ConnectorPrev[Channel[j]].Add(new double[3] { 0, End, ID }); }
                        StaticData.BeforeTime[Channel[j]] = Time;
                    }

                    // 노트를 게임에 추가합니다.
                    Game.CreateNote(new NoteData(ID++, 1, (float)Time, (float)(Speed[j] * StaticData.SpeedMultiplier), (float)Start, (float)End, Mode, Flick, new Color32(Color[j][0], Color[j][1], Color[j][2], Color[j][3]), Prevs));
                }

                Time += (((240 / StaticData.CurrentBPM) * StaticData.BeatMultiplier) / MaxBit);
            }
        }

        public int LCM(int a, int b)
        {
            return (a * b) / GCD(a, b);
        }

        public int GCD(int a, int b)
        {
            if (b.Equals(0)) { return a; }
            return GCD(b, a % b);
        }
    }
}
