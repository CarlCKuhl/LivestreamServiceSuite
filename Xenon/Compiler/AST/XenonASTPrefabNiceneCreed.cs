﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTPrefabNiceneCreed : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }

        IXenonASTElement IXenonASTElement.Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            this.Parent = Parent;
            return this;
        }

        List<Slide> IXenonASTElement.Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            List<Slide> slides = new List<Slide>();
            // add 1 slides for each image we have to render
            for (int i = 1; i <= 5; i++)
            {
                Slide slide = new Slide();
                slide.Name = "UNNAMED_prefab";
                slide.Number = project.NewSlideNumber;
                slide.Lines = new List<SlideLine>();
                slide.Asset = "";
                slide.Data["prefabtype"] = PrefabSlides.NiceneCreed;
                slide.Data["layoutnum"] = i;
                slide.MediaType = MediaType.Image;
                slide.Format = SlideFormat.Prefab;
                slide.AddPostset(_Parent, i == 1, i == 5);
                slides.Add(slide);
            }
            return slides;
        }

        void IXenonASTElement.GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTPrefabSlide>");
            Debug.WriteLine(PrefabSlides.NiceneCreed);
            Debug.WriteLine("</XenonASTPrefabSlide>");
        }

        XenonCompilerSyntaxReport IXenonASTElement.Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
