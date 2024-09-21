using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameTac.Net.Client.Core
{
    public class Encoding
    {
        public static byte[] GetBytes(string str)
        {
            return System.Text.Encoding.UTF8.GetBytes(str);
        }
        public static string GetString(byte[] bytes, int offset, int count)
        {
            return System.Text.Encoding.UTF8.GetString(bytes, offset, count);
        }

        public static byte[] Serialize<T>(T obj)
        {
            return GetBytes(JsonUtility.ToJson(obj));
        }

        public static object Deserialize(string str, Type type)
        {
            return JsonUtility.FromJson(str, type);
        }
    }
}