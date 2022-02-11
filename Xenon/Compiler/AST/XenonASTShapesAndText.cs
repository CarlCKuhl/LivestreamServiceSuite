﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Helpers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    internal class XenonASTShapesAndText : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }

        public List<string> Texts { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTShapesAndText shapeAndTexts = new XenonASTShapesAndText();
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{");
            Lexer.GobbleWhitespace();

            // TODO: allow escaping of }
            string x = Lexer.ConsumeUntil("}");
            Lexer.GobbleandLog("}");

            shapeAndTexts.Texts = x.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();
            shapeAndTexts.Parent = Parent;

            return shapeAndTexts;
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Slide slide = new Slide
            {
                Name = "UNNAMED_upnext",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>(),
                Asset = "",
                Format = SlideFormat.ShapesAndTexts,
                MediaType = MediaType.Image,
            };

            slide.Data[ShapeAndTextRenderer.DATAKEY_TEXTS] = Texts;
            slide.Data[ShapeAndTextRenderer.DATAKEY_FALLBACKLAYOUT] = LanguageKeywords.LayoutForType[LanguageKeywordCommand.CustomText].defaultJsonFile;
            (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.CustomText);

            slide.AddPostset(_Parent, true, true);

            return slide.ToList();
        }

        public void GenerateDebug(Project project)
        {
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
