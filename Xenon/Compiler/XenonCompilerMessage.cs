﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Xenon.Compiler
{
    public enum XenonCompilerMessageType
    {
        Debug,
        Message,
        Info,
        Warning,
        Error,
    }

    static class XenonCompilerMessageTypeConverter
    {
        public static string ToString(this XenonCompilerMessageType type)
        {
            return type switch
            {
                XenonCompilerMessageType.Debug => "Debug",
                XenonCompilerMessageType.Message => "Message",
                XenonCompilerMessageType.Info => "Info",
                XenonCompilerMessageType.Warning => "Warning",
                XenonCompilerMessageType.Error => "Error",
                _ => "Default",
            };
        }
    }


    public class XenonCompilerMessage
    {
        public string ErrorName { get; set; }
        public string ErrorMessage { get; set; }
        public string Token { get; set; }
        public XenonCompilerMessageType Level { get; set; }

        public override string ToString()
        {
            return $"[{Level}]\t{ErrorName}\t{ErrorMessage}\t<on token '{Token}'>";
        }
    }
}
