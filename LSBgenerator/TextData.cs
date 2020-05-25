﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace LSBgenerator
{


    public enum Speaker
    {
        Pastor,
        Congregation,
        Leader,
        Assistant,
        Left,
        Right,
        Men,
        Women,
        Respond,
        Versicle,
        None,
    }

    public class LiturgySpeaker
    {
        public Speaker Speaker { get; set; }

        public void Render() { }

    }

    public interface ITypesettable
    {
        RenderSlide TypesetSlide(RenderSlide slide, TextRenderer r);

    }

    public class LiturgyLineState
    {
        public bool SpeakerWrap { get; set; } = false;
        public Speaker LastSpeaker { get; set; } = Speaker.None;
    }

    public class LiturgyLine : ITypesettable
    {
        public Speaker Speaker { get; set; }
        public string Text { get; set; }

        public int SubSplit { get; set; } = 0;

        public bool IsSubsplit { get; set; } = false;


        /// <summary>
        /// Computes the display length of the string
        /// </summary>
        /// <returns></returns>
        public int ComputeWidth(Font f)
        {
            return 0;
        }

        public RenderSlide TypesetSlide(RenderSlide slide, TextRenderer r)
        {
            LiturgyLine line = this;


            // update state if not in wrapmode
            if (!r.LLState.SpeakerWrap)
            {
                r.LLState.LastSpeaker = line.Speaker;
            }
            else
            {
                line.Speaker = r.LLState.LastSpeaker;
            }

            // try to fit to current slide
            int lineheight = (int)Math.Ceiling(slide.gfx.MeasureString("CPALTgy", r.SpeakerFonts.TryGetVal(line.Speaker, r.Font)).Height);

            // check if whole line will fit

            SizeF s = slide.gfx.MeasureString(line.Text, r.SpeakerFonts.TryGetVal(line.Speaker, r.Font), r.ETextRect.Width);

            if (slide.YOffset + s.Height <= r.ETextRect.Height)
            {
                // it fits, add this line at position and compute
                RenderLine rl = new RenderLine() { Height = (int)s.Height, Width = (int)s.Width, ShowSpeaker = line.SubSplit == 0 || slide.Lines == 0, Speaker = line.Speaker, Text = line.Text, RenderX = 0, RenderY = 0, RenderLayoutMode = LayoutMode.Auto, LineNum = slide.Lines++, Font = r.SpeakerFonts.TryGetVal(line.Speaker, r.Font) };
                slide.RenderLines.Add(rl);
                slide.YOffset += (int)s.Height;
                // wrap only works for one line
                r.LLState.SpeakerWrap = false;
                return slide;
            }
            // if slide isn't blank, then try it on a blank slide
            if (!slide.Blank)
            {
                r.Slides.Add(r.FinalizeSlide(slide));
                // wrap only works for one line
                r.LLState.SpeakerWrap = false;
                return line.TypesetSlide(new RenderSlide() { Order = slide.Order + 1 }, r);
            }
            // if doesn't fit on a blank slide
            // try to split into sentences. put as many sentences as fit on one slide until done


            // If have already split into sentences... split into chunks of words?
            if (line.IsSubsplit)
            {
                RenderSlide errorslide;
                if (!slide.Blank)
                {
                    r.Slides.Add(r.FinalizeSlide(slide));
                    errorslide = new RenderSlide() { Order = slide.Order + 1 };
                }
                else
                {
                    errorslide = slide;
                }
                // for now abandon
                string emsg = "ERROR";
                Size esize = slide.gfx.MeasureString(emsg, r.Font).ToSize();
                RenderLine errorline = new RenderLine() { Height = esize.Height, Width = esize.Width, ShowSpeaker = r.LLState.SpeakerWrap, Speaker = Speaker.None, Text = emsg, RenderX = 0, RenderY = 0, RenderLayoutMode = LayoutMode.Auto, LineNum = 0, Font = r.Font, TextBrush = Brushes.Red };
                errorslide.RenderLines.Add(errorline);
                r.Slides.Add(r.FinalizeSlide(errorslide));
                // wrap only works for one line
                r.LLState.SpeakerWrap = false;
                return new RenderSlide() { Order = errorslide.Order + 1 };

            }


            // split line text into sentences
            var sentences = line.Text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);

            // keep trying to stuff a sentence
            List<LiturgyLine> sublines = new List<LiturgyLine>();
            int splitnum = 0;
            foreach (var sentence in sentences)
            {
                // create a bunch of sub-liturgy-lines and try to typeset them all
                sublines.Add(new LiturgyLine() { Speaker = line.Speaker, Text = sentence.Trim() + ".", SubSplit = splitnum++, IsSubsplit = true });
            }

            foreach (var sl in sublines)
            {
                slide = sl.TypesetSlide(slide, r);
            }


            // wrap only works for one line
            r.LLState.SpeakerWrap = false;
            return slide;

        }

    }


    public enum Command
    {
        NewSlide,
        WrapSpeakerText,
    }
    public class TypesetCommand : ITypesettable
    {
        public Command Command { get; set; }

        public RenderSlide TypesetSlide(RenderSlide slide, TextRenderer r)
        {
            if (Command == Command.NewSlide)
            {
                // force new slide
                r.Slides.Add(r.FinalizeSlide(slide));
                return new RenderSlide() { Order = slide.Order + 1 };
            }
            if (Command == Command.WrapSpeakerText)
            {
                r.LLState.SpeakerWrap = true;
            }
            return slide;
        }
    }

    public class ReadingLine : ITypesettable
    {

        public string Title { get; set; }
        public string Reference { get; set; }

        public RenderSlide TypesetSlide(RenderSlide slide, TextRenderer r)
        {
            // force a new slide
            RenderSlide rslide = slide;
            if (!slide.Blank)
            {
                r.Slides.Add(r.FinalizeSlide(slide));
                rslide = new RenderSlide() { Order = slide.Order + 1 };
            }


            // create the slide 


            Font titlefont = new Font(r.Font, FontStyle.Bold);
            Font reffont = new Font(r.Font, FontStyle.Bold | FontStyle.Italic);

            // add render lines for title and for reference
            Size tsize = rslide.gfx.MeasureString(Title, titlefont).ToSize();
            int titley = r.ETextRect.Height / 2 - (tsize.Height / 2);


            RenderLine titleline = new RenderLine() { Height = tsize.Height, Width = tsize.Width, LineNum = 0, RenderLayoutMode = LayoutMode.Fixed, ShowSpeaker = false, Speaker = Speaker.None, Text = Title, RenderX = 0, RenderY = titley, Font = titlefont };

            Size rsize = rslide.gfx.MeasureString(Reference, reffont).ToSize();
            // add equal column spacing
            int padright = r.ETextRect.X - r.TextboxRect.X + r.PaddingCol;
            int refx = r.ETextRect.Width - rsize.Width - padright;
            int refy = r.ETextRect.Height / 2 - (rsize.Height / 2);
            RenderLine refline = new RenderLine() { Height = rsize.Height, Width = rsize.Width, LineNum = 0, RenderLayoutMode = LayoutMode.Fixed, ShowSpeaker = false, Speaker = Speaker.None, Text = Reference, RenderX = refx, RenderY = refy, Font = reffont };

            rslide.RenderLines.Add(titleline);
            rslide.RenderLines.Add(refline);


            r.Slides.Add(r.FinalizeSlide(rslide));

            // return a new black slide
            return new RenderSlide() { Order = rslide.Order + 1 };

        }
    }

    public class SermonTitle : ITypesettable
    {
        public string Title { get; set; }
        public string SermonName { get; set; }
        public string SermonText { get; set; }

        public RenderSlide TypesetSlide(RenderSlide slide, TextRenderer r)
        {
            // force a new slide
            RenderSlide rslide = slide;
            if (!slide.Blank)
            {
                r.Slides.Add(r.FinalizeSlide(slide));
                rslide = new RenderSlide() { Order = slide.Order + 1 };
            }

            // create the slide 


            Font titlefont = new Font(r.Font, FontStyle.Bold);
            Font sermontitlefont = new Font(r.Font, FontStyle.Bold);
            Font reffont = new Font(r.Font, FontStyle.Bold | FontStyle.Italic);

            // add render lines for title and for reference
            Size tsize = rslide.gfx.MeasureString(Title, r.Font).ToSize();
            Size rsize = rslide.gfx.MeasureString(SermonText, reffont).ToSize();
            Size nsize = rslide.gfx.MeasureString(SermonName, r.Font).ToSize();

            int yslack = r.ETextRect.Height - Math.Max(tsize.Height, rsize.Height) - nsize.Height;

            int titley = yslack / 3;

            RenderLine titleline = new RenderLine() { Height = tsize.Height, Width = tsize.Width, LineNum = 0, RenderLayoutMode = LayoutMode.Fixed, ShowSpeaker = false, Speaker = Speaker.None, Text = Title, RenderX = 0, RenderY = titley, Font = titlefont };

            // add equal column spacing
            int padright = r.ETextRect.X - r.TextboxRect.X + r.PaddingCol;
            int refx = r.ETextRect.Width - rsize.Width - padright;
            int refy = yslack / 3;
            RenderLine refline = new RenderLine() { Height = rsize.Height, Width = rsize.Width, LineNum = 0, RenderLayoutMode = LayoutMode.Fixed, ShowSpeaker = false, Speaker = Speaker.None, Text = SermonText, RenderX = refx, RenderY = refy, Font = reffont };

            rslide.RenderLines.Add(titleline);
            rslide.RenderLines.Add(refline);

            int stitley = yslack / 3 * 2 + Math.Max(tsize.Height, rsize.Height);
            int stitlex = r.ETextRect.Width / 2 - padright - nsize.Width / 2;
            RenderLine sermontitleline = new RenderLine() { Height = nsize.Height, Width = nsize.Width, LineNum = 1, RenderLayoutMode = LayoutMode.Fixed, ShowSpeaker = false, Speaker = Speaker.None, Text = SermonName, RenderX = stitlex, RenderY = stitley, Font = sermontitlefont };

            rslide.RenderLines.Add(sermontitleline);



            r.Slides.Add(r.FinalizeSlide(rslide));

            // return a new black slide
            return new RenderSlide() { Order = rslide.Order + 1 };

        }
    }

    public class TextData
    {

        public List<ITypesettable> LineData = new List<ITypesettable>();

        public void ParseText(string text)
        {
            // preprocessor to make linewraping more convenient
            text = text.Replace(@"\wrap", @"\\\wrap\\");
            var lines = text.Split(new[] { @"\\" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                string tl = line.Trim().Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " ").Replace("\r\n", " ");

                if (tl.StartsWith(@"\break"))
                {
                    TypesetCommand cmd = new TypesetCommand() { Command = Command.NewSlide };
                    LineData.Add(cmd);
                    continue;
                }

                if (tl.StartsWith(@"\wrap"))
                {
                    TypesetCommand cmd = new TypesetCommand() { Command = Command.WrapSpeakerText };
                    LineData.Add(cmd);
                    continue;
                }

                // typeset the reading on a new slide
                // expected params are comma delimited
                // eg. \reading(<Title>,<Reference>)
                if (tl.StartsWith(@"\reading"))
                {
                    var contents = tl.Split('(', ',', ')');
                    ReadingLine rl = new ReadingLine() { Title = contents[1], Reference = contents[2] };
                    LineData.Add(rl);
                    continue;
                }

                if (tl.StartsWith(@"\sermon"))
                {
                    var contents = tl.Split('(', ',', ')');
                    SermonTitle sermonTitle = new SermonTitle() { Title = "Sermon", SermonName = contents[1], SermonText = contents[2] };
                    LineData.Add(sermonTitle);
                    continue;
                }

                LiturgyLine l = new LiturgyLine();
                if (tl.StartsWith("P"))
                {
                    l.Text = tl.Replace(" T ", " + ").Substring(1).Trim();
                    l.Speaker = Speaker.Pastor;
                }
                else if (tl.StartsWith("C"))
                {
                    l.Text = tl.Replace(" T ", " + ").Substring(1).Trim();
                    l.Speaker = Speaker.Congregation;
                }
                else if (tl.StartsWith("A"))
                {
                    l.Text = tl.Replace(" T ", " + ").Substring(1).Trim();
                    l.Speaker = Speaker.Assistant;
                }
                else
                {
                    l.Text = tl.Trim();
                    l.Speaker = Speaker.None;
                }
                l.IsSubsplit = false;
                LineData.Add(l);
            }


        }



        StringFormat format = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near, Trimming = StringTrimming.Word };

        public void Render_PreviewText(Graphics gfx, TextRenderer r, Font f, bool clipLines = true)
        {


            int linestartX = 10 + (r.DisplayWidth - r.TextboxWidth) / 2;
            int linestartY = r.DisplayHeight - r.TextboxHeight;

            int speakerblocksize = (int)(f.Size * gfx.DpiX / 72);

            gfx.Clear(Color.Black);


            gfx.FillRectangle(Brushes.White, (r.DisplayWidth - r.TextboxWidth) / 2, r.DisplayHeight - r.TextboxHeight, r.TextboxWidth, r.TextboxHeight);



            // render in centered mode
            foreach (var l in LineData)
            {
                LiturgyLine ll;
                if (l is LiturgyLine)
                {
                    ll = l as LiturgyLine;
                }
                else
                {
                    return;
                }

                RectangleF displayaera = new RectangleF(3 + linestartX + speakerblocksize, linestartY + 3, r.TextboxWidth - speakerblocksize, r.TextboxHeight - 3);
                Font df;
                if (ll.Speaker == Speaker.Congregation)
                {
                    df = new Font(f, FontStyle.Bold);
                }
                else
                {
                    df = new Font(f, FontStyle.Regular);
                }

                if (clipLines)
                {
                    if (linestartY + (int)gfx.MeasureString(ll.Text, df, displayaera.Size).Height >= r.DisplayHeight)
                    {
                        return;
                    }
                }

                // draw speaker

                // compute lineheight of single character
                int lineheight = (int)Math.Ceiling(gfx.MeasureString("C", df).Height);
                int speakeroffsety = (int)Math.Floor((lineheight - speakerblocksize) / 2d) - 1;

                //gfx.DrawRectangle(Pens.Black, new Rectangle(linestartX, linestartY + speakeroffsety, speakerblocksize, speakerblocksize));
                Rectangle speakerblock = new Rectangle(linestartX, linestartY + speakeroffsety, speakerblocksize, speakerblocksize);


                if (ll.Speaker == Speaker.Congregation)
                {
                    Font cf = new Font(f, FontStyle.Bold);
                    gfx.FillPath(Brushes.Black, DrawRoundedRect(speakerblock, 2));
                    gfx.DrawString("C", cf, Brushes.White, speakerblock, format);
                }

                if (ll.Speaker == Speaker.Pastor)
                {
                    Font cf = new Font(f, FontStyle.Bold);
                    gfx.FillPath(Brushes.White, DrawRoundedRect(speakerblock, 2));
                    gfx.DrawPath(Pens.Black, DrawRoundedRect(speakerblock, 2));
                    gfx.DrawString("P", cf, Brushes.Black, speakerblock, format);
                }

                // account for offset of speaker graphic

                gfx.DrawString(ll.Text, df, Brushes.Black, displayaera, format);
                int yoffset = (int)gfx.MeasureString(ll.Text, df, new Size(r.TextboxWidth, r.TextboxHeight)).Height;
                linestartY += yoffset;

            }


        }

        private GraphicsPath DrawRoundedRect(Rectangle bounds, int borderradius)
        {

            int diameter = borderradius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (borderradius == 0)
            {
                path.AddRectangle(arc);
                return path;
            }

            path.AddArc(arc, 180, 90);

            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;

        }







    }
}
