﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Xenon.Compiler.AST;
using Xenon.Helpers;

namespace Xenon.Compiler
{
    public class XenonSuggestionService
    {


        public static (bool completed, List<(string suggestion, string description)> suggestions) GetDescriptionsForRegexMatchedSequence(List<(string regex, bool optional, string captureas, List<(string, string)> suggestions, string externalfunctionname)> sequentialoptions, string sourcecode, IXenonCommandSuggestionCallback root)
        {
            /*
            1. start matching regexes in sequence
            2. provide the suggestions for the first failed match
             */

            StringBuilder strbuffer = new StringBuilder();
            strbuffer.Append(sourcecode);

            Dictionary<string, string> priormatches = new Dictionary<string, string>();

            List<(string, string)> lastsuggestions = new List<(string, string)>();

            foreach (var option in sequentialoptions)
            {
                // try matching regex against source code
                // upon success remove the matched part and continue on
                var match = Regex.Match(strbuffer.ToString(), option.regex);
                if (match.Success)
                {
                    if (!string.IsNullOrEmpty(option.captureas))
                    {
                        priormatches[option.captureas] = match.Value;
                    }
                    strbuffer.Remove(0, match.Length + match.Index);
                    lastsuggestions.Clear();
                }
                else if (option.optional)
                {
                    lastsuggestions.AddRange(option.suggestions);
                }
                else
                {
                    if (!string.IsNullOrEmpty(option.externalfunctionname))
                    {
                        // call back to base type and have them use a contextual suggestion based on current option
                        var extfunc = root.GetType().GetField(option.externalfunctionname, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        if (extfunc.FieldType == typeof(IXenonCommandSuggestionCallback.GetContextualSuggestionsForCommand))
                        {
                            IXenonCommandSuggestionCallback.GetContextualSuggestionsForCommand dfunc = (IXenonCommandSuggestionCallback.GetContextualSuggestionsForCommand)extfunc.GetValue(root);
                            var suggestions = dfunc.Invoke(priormatches, sourcecode, strbuffer.ToString()).Concat(lastsuggestions);
                            return (false, suggestions.ToList());
                        }
                    }

                    // order suggestions by partial completion, then alphabetically
                    string remainder = strbuffer.ToString();

                    return (false, option.suggestions.Concat(lastsuggestions).OrderByClosestMatch(remainder).ToList());
                }
            }

            return (true, lastsuggestions);

        }


        public static List<(string item, string description)> GetSuggestions(string sourcetext, int caretpos)
        {
            List<(string, string)> suggestions = new List<(string, string)>();

            // perform a lookbehind and dispatch based on that
            suggestions.AddRange(GetSuggestionsByLookBehind(sourcetext, caretpos));


            return suggestions;
        }


        private static List<(string item, string description)> GetSuggestionsByLookBehind(string sourcetext, int caretpos)
        {
            /*
            1. compute current context (if preformance allows) -> scan backwards and try to build an infered stack of where in the AST we are. Only need to go to the most curent top level element
            2. if we don't have a top level element -> provide list of valid commands
            3. if we have a top level element -> drill down into the appropriate help tree based on the stack we build for this command
             */

            var toplevelcmd = WalkBackToTopLevelCommand(sourcetext, caretpos);
            if (toplevelcmd.cmd == LanguageKeywordCommand.INVALIDUNKNOWN)
            {
                List<(string, string)> res = new List<(string, string)>();
                if (toplevelcmd.index == -1 || toplevelcmd.index + LanguageKeywords.Commands.GetValueOrDefault(toplevelcmd.cmd, "#").Length == caretpos)
                {
                    return AllCommandKeywords();
                }
                else
                {
                    var partialmatch = Regex.Match(sourcetext.Substring(toplevelcmd.index), @"#(?<partial>\w*)").Groups["partial"].Value;
                    var partialsuggestions = GetPartialMatchedTopLevelCommands(partialmatch).Select(m => ("#" + m.cmdstr, ""));
                    if (partialsuggestions.Any())
                    {
                        return partialsuggestions.ToList();
                    }
                    return AllCommandKeywords();
                }
            }
            else
            {
                if (toplevelcmd.completed)
                {
                    return AllCommandKeywords();
                }
                //if (toplevelcmd.index + LanguageKeywords.Commands[toplevelcmd.cmd].Length <= caretpos)
                //{
                //return AllCommandKeywords();
                //}
                //return new List<(string item, string description)>() { ("command already found", LanguageKeywords.Commands[toplevelcmd.cmd]) };
                return GetCommandContextualSuggestions(toplevelcmd.cmd, sourcetext.Substring(toplevelcmd.index, caretpos - toplevelcmd.index));
            }

        }

        private static List<(string, string)> AllCommandKeywords()
        {
            /*
            List<(string, string)> res = new List<(string, string)>();
            foreach (var kwd in LanguageKeywords.Commands.Values)
            {
                res.Add(("#" + kwd, ""));
            }
            return res;
            */
            return LanguageKeywords.Commands
                .Where(cmd => LanguageKeywords.LanguageKeywordMetadata[cmd.Key].toplevel == true)
                .Select(cmd => ($"#{cmd.Value}", ""))
                .ToList();
        }



        private static (bool completed, LanguageKeywordCommand cmd, int index) WalkBackToTopLevelCommand(string sourcetext, int caretpos)
        {

            int index = caretpos - 1;
            int firstcmdindex = -1;
            while (index >= 0)
            {
                if (sourcetext[index] == '#')
                {
                    if (firstcmdindex == -1)
                    {
                        firstcmdindex = index;
                    }
                    // perform a relative (to index) lookahead to match the largest command string
                    var match = Regex.Match(sourcetext.Substring(index), @"#(?<command>\w*)");
                    if (match.Groups.ContainsKey("command"))
                    {
                        if (LanguageKeywords.Commands.Values.Contains(match.Groups["command"].Value))
                        {
                            // if we end up having significant nesting of commands may need to revisit idea of walking back to toplevel
                            // but for now we'll ignore non-toplevel commands (eg. #break, #verse)
                            LanguageKeywordCommand cmd = LanguageKeywords.Commands.First(k => k.Value == match.Groups["command"].Value).Key;
                            if (LanguageKeywords.LanguageKeywordMetadata[cmd].toplevel)
                            {
                                if (!IsCommandComplete(cmd, sourcetext.Substring(index, caretpos - index)))
                                {
                                    return (false, cmd, index);
                                }
                                else
                                {
                                    return (true, cmd, firstcmdindex);
                                }
                            }
                        }
                        else
                        {
                            return (false, LanguageKeywordCommand.INVALIDUNKNOWN, firstcmdindex);
                        }
                    }
                }
                index--;
            }
            return (false, LanguageKeywordCommand.INVALIDUNKNOWN, firstcmdindex);
        }

        private static List<(LanguageKeywordCommand cmd, string cmdstr)> GetPartialMatchedTopLevelCommands(string partialstr)
        {
            return LanguageKeywords.Commands
                .Where(cmd => LanguageKeywords.LanguageKeywordMetadata[cmd.Key].toplevel == true && cmd.Value.StartsWith(partialstr))
                .Select(cmd => (cmd.Value, cmd))
                .OrderByClosestMatch(partialstr)
                .Select(x => (x.Item2.Key, x.Item2.Value))
                .ToList();
        }

        private static List<(string suggestion, string description)> GetCommandContextualSuggestions(LanguageKeywordCommand cmd, string sourcecode)
        {
            //CommandContextutalSuggestionDispatcher.GetValueOrDefault(cmd, (_) => new List<(string, string)>())
            return CommandContextutalSuggestionDispatcher.GetValueOrDefault(cmd, (_) => (true, new List<(string, string)>())).Invoke(sourcecode).suggestions;
        }

        private static bool IsCommandComplete(LanguageKeywordCommand cmd, string sourcecode)
        {
            return CommandContextutalSuggestionDispatcher.GetValueOrDefault(cmd, (_) => (true, new List<(string, string)>())).Invoke(sourcecode).completed;
        }

        private static Dictionary<LanguageKeywordCommand, Func<string, (bool completed, List<(string suggestion, string description)> suggestions)>> CommandContextutalSuggestionDispatcher = new Dictionary<LanguageKeywordCommand, Func<string, (bool, List<(string, string)>)>>()
        {
            [LanguageKeywordCommand.SetVar] = (str) => (IXenonASTCommand.GetInstance<XenonASTSetVariable>() as IXenonASTCommand).GetContextualSuggestions(str),
            [LanguageKeywordCommand.Liturgy] = (str) => (IXenonASTCommand.GetInstance<XenonASTLiturgy>() as IXenonASTCommand).GetContextualSuggestions(str),

            [LanguageKeywordCommand.LordsPrayer] = (_) => (true, new List<(string, string)>()),
        };



    }
}
