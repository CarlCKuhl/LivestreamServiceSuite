﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Helpers;
using Xenon.Layouts;

namespace Xenon.LayoutEngine.L2
{

    class SizedTextBlurb : TextBlurb
    {
        public SizeF Size { get; private set; }

        public static SizedTextBlurb CreateMeasured(TextBlurb blurb, Graphics gfx, string defaultFont, float defaultFontSize, FontStyle defaultStyle, RectangleF rect)
        {
            SizedTextBlurb sblurb = new SizedTextBlurb(blurb);
            sblurb.Size = sblurb.Measure(gfx, defaultFont, defaultFontSize, defaultStyle, rect);
            return sblurb;
        }

        public SizedTextBlurb(TextBlurb blurb) : base(blurb.Pos, blurb.Text, blurb.AltFont, blurb.FontStyle, blurb.FontSize, blurb.Space, blurb.IsWhitespace, blurb.HexFColor)
        {
            Size = SizeF.Empty;
        }

        public override SizedTextBlurb Clone()
        {
            return new SizedTextBlurb(base.Clone()) { Size = this.Size };
        }

    }

    internal class TextBlurb
    {
        public Point Pos { get; private set; }
        public string Text { get; private set; }
        public string AltFont { get; private set; }
        public FontStyle FontStyle { get; private set; }
        public float FontSize { get; private set; }

        public bool IsWhitespace { get => Space || NewLine; }
        public bool Space { get; private set; }
        public bool NewLine { get; private set; }

        public string HexFColor { get; private set; }

        public virtual TextBlurb Clone()
        {
            return new TextBlurb(Pos, Text, AltFont, FontStyle, FontSize, Space, NewLine, HexFColor);
        }


        public TextBlurb(Point pos, string text, string altfont = "", FontStyle style = FontStyle.Regular, float size = -1, bool space = false, bool newline = false, string hexColor = null)
        {
            Pos = pos;
            Text = text;
            AltFont = altfont;
            FontStyle = style;
            FontSize = size;
            Space = space;
            NewLine = newline;
            HexFColor = hexColor;
        }

        public SizeF Measure(Graphics gfx, string defaultFont, float defaultFontSize, FontStyle defaultStyle, RectangleF rect)
        {
            FontStyle style = defaultStyle | FontStyle;
            float fsize = FontSize > 0 ? FontSize : defaultFontSize;
            string fname = defaultFont;
            if (!string.IsNullOrEmpty(AltFont))
            {
                var installedFonts = new InstalledFontCollection();
                if (installedFonts.Families.Any(x => x.Name == AltFont))
                {
                    fname = AltFont;
                }
            }

            Font f = new Font(fname, fsize, style);
            SizeF size = gfx.MeasureStringCharacters(Text, ref f, rect);
            f.Dispose();
            return size;
        }

        public void Place(Point p)
        {
            Pos = p;
        }

        public void Render(Graphics gfx, Graphics kgfx, Color defaultfcolor, Color defaultkcolor, string defaultFont, float defaultFontSize, FontStyle defaultStyle)
        {
            FontStyle style = defaultStyle | FontStyle;
            float fsize = FontSize > 0 ? FontSize : defaultFontSize;
            string fname = defaultFont;
            if (!string.IsNullOrEmpty(AltFont))
            {
                var installedFonts = new InstalledFontCollection();
                if (installedFonts.Families.Any(x => x.Name == AltFont))
                {
                    fname = AltFont;
                }
            }

            Color fcolor = defaultfcolor;
            Color kcolor = defaultkcolor;

            if (!string.IsNullOrEmpty(HexFColor))
            {
                var fcol = new LWJColor() { Hex = HexFColor };
                fcolor = Color.FromArgb(255, fcol.Red, fcol.Green, fcol.Blue);
                kcolor = Color.FromArgb(255, fcol.Alpha, fcol.Alpha, fcol.Alpha);
            }


            using (Font f = new Font(fname, fsize, style))
            using (var gbrush = new SolidBrush(fcolor))
            using (var kbrush = new SolidBrush(kcolor))
            {
                gfx.DrawString(Text, f, gbrush, Pos);
                kgfx.DrawString(Text, f, kbrush, Pos);
            }

        }

    }

}
