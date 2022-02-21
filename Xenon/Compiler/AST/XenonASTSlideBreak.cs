﻿using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xenon.Compiler.AST
{
    class XenonASTSlideBreak : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTSlideBreak slidebreak = new XenonASTSlideBreak();
            Lexer.GobbleWhitespace();
            slidebreak.Parent = Parent;
            return slidebreak;

        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            return new List<Slide>();
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("</XenonASTSlideBreak>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
