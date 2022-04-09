﻿using Xenon.SlideAssembly;
using System;
using System.Diagnostics;
using System.Runtime;
using System.Collections.Generic;
using System.Text;
using Xenon.Compiler.Suggestions;
using Xenon.Helpers;
using System.Linq;
using Xenon.AssetManagment;
using System.Text.RegularExpressions;

namespace Xenon.Compiler.AST
{
    class XenonASTScript : IXenonASTCommand, IXenonCommandSuggestionCallback
    {

        public string Source { get; set; } = "";
        public IXenonASTElement Parent { get; private set; }

        private Token _srcToken;

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTScript script = new XenonASTScript();
            script._srcToken = Lexer.CurrentToken;
            Lexer.GobbleWhitespace();
            StringBuilder sb = new StringBuilder();
            Lexer.GobbleandLog("{");
            Lexer.GobbleWhitespace();
            while (!Lexer.InspectEOF())
            {
                sb.Append(Lexer.Consume());
                if (Lexer.Inspect("}"))
                {
                    Lexer.Consume();
                    break;
                }
            }
            string src = sb.ToString();
            // handle whitespace
            var slines = src.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            script.Source = string.Join(Environment.NewLine, slines.Select(x => x.Trim()));
            script.Parent = Parent;
            return script;

        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            // create a video slide
            Slide script = new Slide
            {
                Name = "UNNAMED_script",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>()
            };
            script.Format = SlideFormat.Script;
            script.Asset = "";
            script.MediaType = MediaType.Text;

            string src = Source;

            // resolve variables
            // for now only check if a button in the config file
            foreach (var match in Regex.Matches(Source, "%(?<var>.*)%").AsEnumerable())
            {
                // check if we have that one
                if (project.BMDSwitcherConfig.Routing.Any(x => x.ButtonName == match?.Groups["var"]?.Value))
                {
                    src = Regex.Replace(src, $"%{match.Groups["var"].Value}%", project.BMDSwitcherConfig.Routing.FirstOrDefault(x => x.ButtonName == match.Groups["var"].Value).PhysicalInputId.ToString());
                }
                else
                {
                    Logger.Log(new XenonCompilerMessage()
                    {
                        ErrorName = "Invalid Variable Substution",
                        ErrorMessage = $"Attempting to replace varible with name %{match?.Groups["var"]?.Value}%, but could not find it.",
                        Generator = "XenonASTScript::Generate",
                        Inner = "",
                        Level = XenonCompilerMessageType.Error,
                        Token = _srcToken,
                    });
                }
            }


            script.Data["source"] = src;

            return script.ToList();
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTScript>");
            Debug.WriteLine(Source);
            Debug.WriteLine("</XenonASTScript>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

        List<RegexMatchedContextualSuggestions> IXenonCommandSuggestionCallback.contextualsuggestions { get; } = new List<RegexMatchedContextualSuggestions>()
        {
            ("#script", false, "", new List<(string, string)>() { ("#script", "")}, null),
            ("\\{", false, "", new List<(string, string)>() { ("{", "begin script")}, null),
            ("[^\\}]+(?=\\})",  false,"", null, GetContextualSuggestionsForScriptCommands),
            ("\\}", false, "", new List<(string, string)>() { ("\"", "end script")}, null),
        };

        static IXenonCommandSuggestionCallback.GetContextualSuggestionsForCommand GetContextualSuggestionsForScriptCommands = (priorcaptures, sourcesnippet, remainingsnippet, knownAssets, knownLayouts) =>
        {
            // its' not validation we're doing, so just get the start of the line and work from there.

            List<(string suggestion, string description)> suggestions = new List<(string suggestion, string description)>();

            string currentline = remainingsnippet.Split(Environment.NewLine, StringSplitOptions.None).LastOrDefault() ?? "";

            // empty line... show commands, fullauto, title option
            // fullauto line -> complete full auto
            // command line -> complete command
            // title line -> complete title

            // Title
            if (currentline.TrimStart().StartsWith("#"))
            {
                if (!currentline.EndsWith(";"))
                {
                    suggestions.Add((";", "end script title"));
                }
            }
            // Full auto line
            else if (currentline.TrimStart().StartsWith("!"))
            {
                if (!currentline.EndsWith(";"))
                {
                    suggestions.Add(("fullauto;", "mark slide as fully automated."));
                    suggestions.Add(("displaysrc='<slide>';", "specify alt graphic for action slide"));
                    suggestions.Add(("keysrc='<slide>';", "specify alt key graphic for action slide"));
                }
            }
            // parse commands
            else if (currentline.TrimStart().Length > 0)
            {
                suggestions.AddRange(GetContextualSuggestionsForAction(currentline.TrimStart()));
            }
            // list all options
            else
            {
                suggestions.Add(("!fullauto;", "mark slide as fully automated."));
                suggestions.Add(("#SCRIPT TITLE;", "add script title"));
                suggestions.Add(("@", "add setup action"));
                suggestions.Add(("arg0:", "add an action taking 0 arguments"));
                suggestions.Add(("arg1:", "add an action taking 1 arguments"));
                //suggestions.AddRange(LanguageKeywords.ScriptActionsMetadata.Select(c => (c.Value.ActionName, "")));
            }

            return (false, suggestions);
        };

        private static List<(string, string)> GetContextualSuggestionsForAction(string action)
        {
            List<(string, string)> suggestions = new List<(string, string)>();
            if (action.StartsWith("@"))
            {
                // doesn't really matter for the purpose of suggestions, so just eat it and try again
                return GetContextualSuggestionsForAction(action.Remove(0, 1));
            }
            if (action.StartsWith("arg0:"))
            {
                return GetcontextualSuggestionsForArg0Action(action.Remove(0, 5));
            }
            else if (action.StartsWith("arg1:"))
            {
                return GetcontextualSuggestionsForArg1Action(action.Remove(0, 5));
            }
            else if (action.StartsWith("argd8:"))
            {
                return GetcontextualSuggestionsForArgi8Action(action.Remove(0, 6));
            }
            else
            {
                suggestions.Add(("arg0:", "add an action taking 0 arguments"));
                suggestions.Add(("arg1:", "add an action taking 1 arguments"));
                suggestions.Add(("argd8:", "add an action taking 8 arguments"));
                return suggestions
                    .OrderByClosestStrictMatch(action)
                    .ToList();
            }
        }

        private static List<(string, string)> GetcontextualSuggestionsForArg0Action(string action)
        {
            List<(string, string)> suggestions = new List<(string, string)>();

            if (IsInsideAnnotation(action))
            {
                return new List<(string, string)>() { ("];", "end command annotation") };
            }

            if (action.EndsWith(']'))
            {
                return new List<(string, string)>() { (";", "end command") };
            }

            // check if we match command, else get closeset match. if matching option end or notes.
            suggestions.AddRange(IntegratedPresenterAPIInterop.MetadataProvider.ScriptActionsMetadata
                .Select(sm => (sm.Value.ActionName, sm.Value))
                .Where(c => c.Value.NumArgs == 0)
                .OrderByClosestStrictMatch(action)
                .Select(x => (x.Item1, ""))
                .Where(x => x.Item1.Length >= action.Length));

            if (suggestions.Any(s => s.Item1.Length == action.Length))
            {
                suggestions.Add(("[", "add command annotation"));
                suggestions.Add((";", ""));
            }

            return suggestions;
        }

        private static bool IsInsideAnnotation(string action)
        {
            int i = action.Length;
            while (--i >= 0 && action[i] != ']')
            {
                if (action[i] == '[')
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsInsideParamList(string action)
        {
            int i = action.Length;
            while (--i >= 0 && action[i] != ')')
            {
                if (action[i] == '(')
                {
                    return true;
                }
            }
            return false;
        }

        private static List<(string, string)> GetcontextualSuggestionsForArg1Action(string action)
        {

            List<(string, string)> suggestions = new List<(string, string)>();

            if (IsInsideAnnotation(action))
            {
                return new List<(string, string)>() { ("];", "end command annotation") };
            }

            if (action.EndsWith(']'))
            {
                return new List<(string, string)>() { (";", "end command") };
            }

            if (IsInsideParamList(action))
            {

                // TODO: confirm request wants a camera
                // for now assume it might and allow variable '%' substituion for any of the named variables in the config


            }


            // check if we match command, else get closeset match. if matching option end or notes.
            suggestions.AddRange(IntegratedPresenterAPIInterop.MetadataProvider.ScriptActionsMetadata
                .Select(sm => (sm.Value.ActionName, sm.Value))
                .Where(c => c.Value.NumArgs == 1)
                .OrderByClosestStrictMatch(action)
                .Select(x => (x.Item1, ""))
                .Where(x => x.Item1.Length >= action.Length));

            if (suggestions.Any(s => s.Item1.Length == action.Length))
            {
                suggestions.Add(("(", ""));
            }

            return suggestions;
        }
        private static List<(string, string)> GetcontextualSuggestionsForArgi8Action(string action)
        {

            List<(string, string)> suggestions = new List<(string, string)>();

            if (IsInsideAnnotation(action))
            {
                return new List<(string, string)>() { ("];", "end command annotation") };
            }

            if (action.EndsWith(']'))
            {
                return new List<(string, string)>() { (";", "end command") };
            }

            if (IsInsideParamList(action))
            {
                // get command and look it up by string

            }


            // check if we match command, else get closeset match. if matching option end or notes.
            suggestions.AddRange(IntegratedPresenterAPIInterop.MetadataProvider.ScriptActionsMetadata
                .Select(sm => (sm.Value.ActionName, sm.Value))
                .Where(c => c.Value.NumArgs == 8)
                .OrderByClosestStrictMatch(action)
                .Select(x => (x.Item1, ""))
                .Where(x => x.Item1.Length >= action.Length));

            if (suggestions.Any(s => s.Item1.Length == action.Length))
            {
                suggestions.Add(("(", ""));
            }

            return suggestions;
        }


    }
}