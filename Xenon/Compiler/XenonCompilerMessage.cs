﻿namespace Xenon.Compiler
{
    public enum XenonCompilerMessageType
    {
        Debug = 0,
        Message = 1,
        Info = 2,
        Warning = 3,
        ManualIntervention = 4,
        Error = 5,
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
                XenonCompilerMessageType.ManualIntervention => "Manual",
                XenonCompilerMessageType.Error => "Error",
                _ => "Default",
            };
        }
    }


    public class XenonCompilerMessage
    {
        public string ErrorName { get; set; }
        public string ErrorMessage { get; set; }
        public Token Token { get; set; }
        public string Generator { get; set; }
        public string Inner { get; set; }
        public XenonCompilerMessageType Level { get; set; }

        public override string ToString()
        {
            return $"[{Level}]\t{ErrorName}\t{ErrorMessage}\t<on token '{Token}'>\tGenerated by: {Generator}\tAdditional Info: {Inner}";
        }
    }
}
