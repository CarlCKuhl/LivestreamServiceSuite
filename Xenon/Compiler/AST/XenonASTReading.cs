﻿using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.Helpers;
using System.Linq;
using Xenon.Renderer;

namespace Xenon.Compiler.AST
{


    class XenonASTReading : IXenonASTCommand
    {
        public string Name { get; set; }
        public string Reference { get; set; }
        public IXenonASTElement Parent { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTReading reading = new XenonASTReading();
            Lexer.GobbleWhitespace();
            var args = Lexer.ConsumeArgList(true, "name", "reference");
            reading.Name = args["name"];
            reading.Reference = args["reference"];
            reading.Parent = Parent;
            return reading;

        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Slide readingslide = new Slide();
            readingslide.Name = "UNNAMED_reading";
            readingslide.Number = project.NewSlideNumber;
            readingslide.Lines = new List<SlideLine>();
            readingslide.Asset = "";
            readingslide.Format = SlideFormat.ShapesAndTexts;
            readingslide.MediaType = MediaType.Image;

            List<string> strings = new List<string>
            {
                Name, Reference,
            };

            readingslide.Data[ShapeAndTextRenderer.DATAKEY_TEXTS] = strings;
            readingslide.Data[ShapeAndTextRenderer.DATAKEY_FALLBACKLAYOUT] = LanguageKeywords.LayoutForType[LanguageKeywordCommand.Reading].defaultJsonFile;
            (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, readingslide, LanguageKeywordCommand.Reading);

            readingslide.AddPostset(_Parent, true, true);

            return readingslide.ToList();
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTReading>");
            Debug.WriteLine($"Name='{Name}'");
            Debug.WriteLine($"Reference='{Reference}'");
            Debug.WriteLine("</XenonASTReading>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
