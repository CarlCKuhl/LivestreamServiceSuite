﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Xenon.Compiler.Suggestions;
using Xenon.Helpers;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTSetVariable : IXenonASTCommand, IXenonCommandSuggestionCallback
    {

        public string VariableName;
        public string Value;

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            Lexer.GobbleWhitespace();
            var args = Lexer.ConsumeArgList(true, "name", "value");

            VariableName = args["name"];
            Value = args["value"];

            this.Parent = Parent;
            return this;
        }

        public void Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            project.AddAttribute(VariableName, Value);
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTSetVariable>");
            Debug.WriteLine($"Name='{VariableName}'");
            Debug.WriteLine($"Value='{Value}'");
            Debug.WriteLine("</XenonASTSetVariable>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }


        List<RegexMatchedContextualSuggestions> IXenonCommandSuggestionCallback.contextualsuggestions { get; } = new List<RegexMatchedContextualSuggestions>()
        {
            ("#set", false, "", new List<(string, string)>() { ("#set", "")}, null),
            ("\\(\"", false, "", new List<(string, string)>() { ("(\"", "insert variable name")}, null),
            ("[^\"]+(?=\")", false, "varname", new List<(string, string)>() { ("otherspeakers", ""), ("global.rendermode.alpha", ""), ("\"", "")}, null),
            ("\"", false, "", new List<(string, string)>() { ("\"", "")}, null),
            (",", false, "", new List<(string, string)>() { (",", "")}, null),
            ("\"", false, "", new List<(string, string)>() { ("\"", "")}, null),
            ("[^\"]+(?=\")",  false,"", null, GetContextualSuggestionsForVariableValue),
            ("\"", false, "", new List<(string, string)>() { ("\"", "enclose variable value")}, null),
            ("\\)", false, "", new List<(string, string)>(){(")", "")}, null),
        };

        static IXenonCommandSuggestionCallback.GetContextualSuggestionsForCommand GetContextualSuggestionsForVariableValue = (Dictionary<string, string> priorcaptures, string sourcesnippet, string remainingsnippet) =>
        {
            if (priorcaptures.GetOrDefault("varname", "") == "global.rendermode.alpha")
            {
                return new List<(string suggestion, string description)>() { ("premultiplied", "Renders images premultiplied against keys."), ("\"", "") };
            }
            return new List<(string suggestion, string description)>();
        };

        public IXenonASTElement Parent { get; private set; }

    }
}
