using System.IO;
using UnityEditor;
using UnityEngine;

public class Logger
{
    public static void Log(string message)
    {
        Debug.Log(message);
    }

    public static void LogError(string message)
    {
        Debug.LogError(message);
    }
}
