﻿using Xenon.Compiler;
using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Media.Imaging;
using Xenon.Helpers;
using System.Linq.Expressions;

namespace Xenon.Renderer
{
    class PrefabSlideRenderer
    {
        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(Slide slide, List<XenonCompilerMessage> messages)
        {
            RenderedSlide res = new RenderedSlide();
            res.AssetPath = "";
            res.MediaType = MediaType.Image;
            res.RenderedAs = "Full";

            Bitmap bmp = new Bitmap(Layouts.PrefabLayout.Size.Width, Layouts.PrefabLayout.Size.Height);
            Graphics gfx = Graphics.FromImage(bmp);

            gfx.Clear(Color.White);


            Bitmap src = null;

            try
            {

                switch (slide.Data["prefabtype"])
                {
                    case PrefabSlides.Copyright:
                        res.RenderedAs = "Full-startrecord";
                        src = ProjectResources.PrefabSlides.CopyrightLicense;
                        break;
                    case PrefabSlides.ViewServices:
                        src = ProjectResources.PrefabSlides.ViewServices;
                        break;
                    case PrefabSlides.ViewSeries:
                        src = ProjectResources.PrefabSlides.ViewSessions;
                        break;
                    case PrefabSlides.ApostlesCreed:
                        switch (slide.Data["layoutnum"])
                        {
                            case 1:
                                src = ProjectResources.PrefabSlides.ApostlesCreed1;
                                break;
                            case 2:
                                src = ProjectResources.PrefabSlides.ApostlesCreed2;
                                break;
                            case 3:
                                src = ProjectResources.PrefabSlides.ApostlesCreed3;
                                break;
                        }
                        break;
                    case PrefabSlides.NiceneCreed:
                        switch (slide.Data["layoutnum"])
                        {
                            case 1:
                                src = ProjectResources.PrefabSlides.NiceneCreed1;
                                break;
                            case 2:
                                src = ProjectResources.PrefabSlides.NiceneCreed2;
                                break;
                            case 3:
                                src = ProjectResources.PrefabSlides.NiceneCreed3;
                                break;
                            case 4:
                                src = ProjectResources.PrefabSlides.NiceneCreed4;
                                break;
                            case 5:
                                src = ProjectResources.PrefabSlides.NiceneCreed5;
                                break;
                        }
                        break;
                    case PrefabSlides.LordsPrayer:
                        src = ProjectResources.PrefabSlides.LordsPrayer;
                        break;
                }

            }
            catch (Exception ex)
            {
                res.Bitmap = bmp;
                string tmp = "KEY ERROR on data attribute";
                object a;
                slide.Data.TryGetValue("prefabtype", out a);
                if (a is PrefabSlides && a != null)
                {
                    tmp = ((PrefabSlides)a).Convert();
                }
                messages.Add(new XenonCompilerMessage() { Level = XenonCompilerMessageType.Error, ErrorMessage = $"Requested prefab image not loaded. While rendering slide {slide.Number}", ErrorName = "Prefab not found", Token = tmp });
                throw ex;
            }

            if (src != null)
            {
                gfx.DrawImage(src, new Rectangle(new Point(0, 0), Layouts.PrefabLayout.Size));
            }

            res.Bitmap = bmp;
            return res;
        }
    }
}
