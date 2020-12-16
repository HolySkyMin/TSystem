﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    public class TSystemStatic
    {
        public static IngamePacket ingamePacket;
        public static ResultPacket resultPacket;

        public static string GetPlatformSpecificPath(string target)
        {
            switch(Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.Android:
                    return target.Replace("\\", "/");
                default:
                    return "file://" + target.Replace("\\", "/");
            }
        }

        public static void Log(string message)
        {
            Debug.Log("TSystem: " + message);
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning("TSystem: " + message);
        }

        public static void LogWithException(string coreMessage, System.Exception e)
        {
            Debug.LogWarning(
                "TSystem: " + coreMessage + "\n\tException message: " + e.Message + 
                "\n\tException stack trace is listed below:\n" + e.StackTrace);
        }
    }
}