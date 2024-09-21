using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameTac.Net.Client.Util
{
    public class Print
    {
        public static void PrintM(string info)
        {
            Debug.Log(info);
        }
        public static void PrintE(string info)
        {
            Debug.LogError(info);
        }
        public static void PrintW(string info)
        {
            Debug.LogWarning(info);
        }
    }
}
