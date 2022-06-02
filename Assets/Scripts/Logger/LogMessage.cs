using System;
using UnityEngine;

namespace Logger
{
    public class LogMessage
    {
        public LogType Type { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }

        public LogMessage(LogType type, string message)
        {
            Type = type;
            Time = DateTime.UtcNow;
            Message = message;
        }
    }
}