#region License
// Procedural planet generator.
//  
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//     notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: 2017.05.02
// Creation Time: 5:04 PM
// Creator: zameran
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace SpaceEngine.Core.Debugging
{
    public enum LoggerCategory
    {
        None = -1,
        Important = 0,
        Triggers = 1,
        InGameUI = 2,
        Camera = 3,
        Data = 4,
        Input = 5,
        Core = 6,
        Animation = 7,
        Player = 8,
        Editor = 9,
        Gameplay = 10,
        External = 11,
        Other = 12,
        Graphics = 13
    }

    public enum LogType
    {
        Log = 0,

        Warning = 1,
        Exception = 2,
        Error= 3
    }

    public sealed class LoggerPalette
    {
        private readonly Dictionary<LoggerCategory, Color> CategoryPalette = new Dictionary<LoggerCategory, Color>
        {
            { LoggerCategory.None, XKCDColors.Grey },
            { LoggerCategory.Important, XKCDColors.Amber },
            { LoggerCategory.Triggers, XKCDColors.Orange },
            { LoggerCategory.InGameUI, XKCDColors.DullGreen },
            { LoggerCategory.Camera, XKCDColors.Green },
            { LoggerCategory.Data, XKCDColors.BrightSkyBlue },
            { LoggerCategory.Input, XKCDColors.Yellow },
            { LoggerCategory.Core, XKCDColors.DullRed },
            { LoggerCategory.Animation, XKCDColors.Purple },
            { LoggerCategory.Player, XKCDColors.Greenish },
            { LoggerCategory.Editor, XKCDColors.White },
            { LoggerCategory.Gameplay, XKCDColors.DarkPurple },
            { LoggerCategory.External, XKCDColors.Brownish },
            { LoggerCategory.Other, XKCDColors.DirtyGreen },
            { LoggerCategory.Graphics, XKCDColors.BrightRed }
        };

        private readonly Dictionary<LogType, Color> TypePalette = new Dictionary<LogType, Color>
        {
            { LogType.Log, XKCDColors.Greyish },
            { LogType.Warning, XKCDColors.Yellow },
            { LogType.Exception, XKCDColors.Red },
            { LogType.Error, XKCDColors.Red }
        };

        public Color GetColorFromCategory(LoggerCategory loggerCategory)
        {
            return CategoryPalette[loggerCategory];
        }

        public Color GetColorFromType(LogType logType)
        {
            return TypePalette[logType];
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
    public class UseLogger : Attribute
    {
        public LoggerCategory LoggerCategory;

        public UseLogger(LoggerCategory loggerCategory)
        {
            LoggerCategory = loggerCategory;
        }
    }

    // NOTE : Do not use in threading!
    public static class Logger
    {
#if UNITY_EDITOR
        private static readonly LoggerPalette Palette = new LoggerPalette();
#endif
        private static readonly LoggerCategory DefaultLoggerCategory = LoggerCategory.Other;

        private static bool DebuggerActive;

        #region API

        public static string Colored(this string message, string colorCode)
        {
            return string.Format("<color={0}>{1}</color>", colorCode, message);
        }

        public static string Bold(this string message)
        {
            return string.Format("<b>{0}</b>", message);
        }

        public static string Italic(this string message)
        {
            return string.Format("<i>{0}</i>", message);
        }

        public static string ToRGBHex(Color color)
        {
            return XKCDColors.ColorTranslator.ToRGBHex(color);
        }

        public static string FormatForConsole(string categoryColorCode, string typeColorCode, string categoryString, string typeString, object obj)
        {
            return string.Format("{0} : {1} : {2}", Colored(string.Format("[{0}]", typeString.ToUpper()), typeColorCode), Colored(string.Format("[{0}]", categoryString.ToUpper()), categoryColorCode), obj);
        }

        public static string FormatForFile(DateTime timeStamp, string categoryString, string typeString, object obj)
        {
            return string.Format("[{0:H:mm:ss}] : [{1}] : [{2}] : {3}", timeStamp, typeString.ToUpper(), categoryString.ToUpper(), obj);
        }

        #endregion

        private static void Detect(out LoggerCategory loggerCategory)
        {
            var frame = new StackFrame(2, true);
            var frameMethod = frame.GetMethod();
            var frameType = frameMethod.DeclaringType;

            if (frameType != null)
            {
                var loggerClassAttributes = frameType.GetCustomAttributes(typeof(UseLogger), true) as UseLogger[];

                if (loggerClassAttributes != null && loggerClassAttributes.Length != 0)
                {
                    loggerCategory = loggerClassAttributes[0].LoggerCategory;
                    DebuggerActive = true;
                }
                else
                {
                    loggerCategory = DefaultLoggerCategory;
                    DebuggerActive = false;
                }
            }
            else
            {
                loggerCategory = DefaultLoggerCategory;
                DebuggerActive = false;
            }
        }

        public static void Log(object obj)
        {
            LoggerCategory loggerCategory;

            Detect(out loggerCategory);

            PrintToLog(obj, LogType.Log, loggerCategory);
        }

        public static void LogWarning(object obj)
        {
            LoggerCategory loggerCategory;

            Detect(out loggerCategory);

            PrintToLog(obj, LogType.Warning, loggerCategory);
        }

        public static void LogException(object obj)
        {
            throw new NotImplementedException();
        }

        public static void LogError(object obj)
        {
            LoggerCategory loggerCategory;

            Detect(out loggerCategory);

            // NOTE : Override...
            DebuggerActive = true;

            PrintToLog(obj, LogType.Error, loggerCategory);
        }

        private static void PrintToLog(object obj, LogType logType, LoggerCategory loggerCategory)
        {
            if (!DebuggerActive)
            {
                Debug.Log(obj.ToString());

                return;
            }

            var typeString = logType.ToString();
            var categoryString = loggerCategory.ToString();

#if UNITY_EDITOR
            obj = FormatForConsole(ToRGBHex(Palette.GetColorFromCategory(loggerCategory)), ToRGBHex(Palette.GetColorFromType(logType)), categoryString, typeString, obj);
#elif UNITY_STANDALONE
            obj = FormatForFile(DateTime.Now, categoryString, typeString, obj);

            try
            {
                using (var outputStream = System.IO.File.AppendText(System.IO.Path.GetFullPath(string.Format("{0}/../Log.log", Application.dataPath))))
                {
                    outputStream.WriteLine(obj);
                    outputStream.Flush();
                    outputStream.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                
            }
#endif
            Debug.Log(obj);
        }
    }
}