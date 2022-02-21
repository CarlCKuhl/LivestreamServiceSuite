﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.Json.Serialization;

using Xenon.Helpers;

namespace Xenon.LayoutInfo
{
    class LWJPoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        public LWJPoint() { }
        public LWJPoint(Point p)
        {
            X = p.X;
            Y = p.Y;
        }
        public Point GetPoint()
        {
            return new Point(X, Y);
        }

        public PointF GetPointF()
        {
            return new PointF(X, Y);
        }
    }

    class LWJSize
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public LWJSize() { }
        public LWJSize(Size size)
        {
            Width = size.Width;
            Height = size.Height;
        }
        public Size GetSize()
        {
            return new Size(Width, Height);
        }
    }

    class LWJRect
    {
        public LWJPoint Origin { get; set; }
        public LWJSize Size { get; set; }
        public LWJRect() { }
        public LWJRect(Rectangle rect)
        {
            Origin = new LWJPoint(rect.Location);
            Size = new LWJSize(rect.Size);
        }
        public Rectangle GetRectangle()
        {
            return new Rectangle(Origin.GetPoint(), Size.GetSize());
        }

        internal RectangleF GetRectangleF()
        {
            return new RectangleF(Origin.GetPoint(), Size.GetSize());
        }
    }

    class LWJPolygon
    {
        public LWJTransformSet Transforms { get; set; } = new LWJTransformSet();
        public List<LWJPoint> Verticies { get; set; } = new List<LWJPoint>();
        public int BorderWidth { get; set; }
        public LWJColor FillColor { get; set; }
        public LWJColor KeyFillColor { get; set; }
        public LWJColor BorderColor { get; set; }
        public LWJColor KeyBorderColor { get; set; }

    }

    class LWJTransformSet
    {
        public LWJScaleTransform Scale { get; set; } = new LWJScaleTransform();
        public LWJTranslateTransform Translate { get; set; } = new LWJTranslateTransform();
    }

    abstract class LWJTransform
    {
        public abstract PointF[] Apply(PointF[] points);
    }

    class LWJScaleTransform : LWJTransform
    {
        public double XScale { get; set; } = 1;
        public double YScale { get; set; } = 1;

        public override PointF[] Apply(PointF[] points)
        {
            return points.Select(p => new PointF((float)(p.X * XScale), (float)(p.Y * YScale))).ToArray();
        }
    }
    class LWJTranslateTransform : LWJTransform
    {
        public double XShift { get; set; } = 0;
        public double YShift { get; set; } = 0;

        public override PointF[] Apply(PointF[] points)
        {
            return points.Select(p => new PointF((float)(p.X + XShift), (float)(p.Y + YShift))).ToArray();
        }
    }


    class LWJFont
    {
        public string Name { get; set; }
        public float Size { get; set; }
        public int Style { get; set; }
        public LWJFont() { }
        public LWJFont(Font f)
        {
            Name = f.Name;
            Size = f.Size;
            Style = (int)f.Style;
        }
        public Font GetFont()
        {
            return new Font(Name, Size, (FontStyle)Style);
        }

        public FontStyle GetStyle()
        {
            return (FontStyle)Style;
        }
    }

    enum LWJHAlign
    {
        Left,
        Center,
        Right,
        Justified
    }
    enum LWJVAlign
    {
        Top,
        Center,
        Bottom,
        Equidistant
    }


    class LWJColor
    {
        /// <summary>
        /// #AARRGGBB
        /// </summary>
        public string Hex { get; set; }

        [JsonIgnore]
        public int Alpha { get => Hex?.Length == 9 ? int.Parse(Hex?.Substring(1, 2), System.Globalization.NumberStyles.HexNumber) : 0; }

        [JsonIgnore]
        public int Red { get => Hex?.Length == 9 ? int.Parse(Hex?.Substring(3, 2), System.Globalization.NumberStyles.HexNumber) : 0; }

        [JsonIgnore]
        public int Green { get => Hex?.Length == 9 ? int.Parse(Hex?.Substring(5, 2), System.Globalization.NumberStyles.HexNumber) : 0; }

        [JsonIgnore]
        public int Blue { get => Hex?.Length == 9 ? int.Parse(Hex?.Substring(7, 2), System.Globalization.NumberStyles.HexNumber) : 0; }

        public LWJColor() { }
        public LWJColor(Color col)
        {
            Hex = $"#{col.A:X2}{col.R:X2}{col.G:X2}{col.B:X2}";
        }
        public Color GetColor()
        {
            return Color.FromArgb(Alpha, Red, Green, Blue);
        }

        public SixLabors.ImageSharp.Color ToColor()
        {
            return SixLabors.ImageSharp.Color.FromRgba((byte)Red, (byte)Green, (byte)Blue, (byte)Alpha);
        }

    }

    class LWJTLVerseLayout
    {
        public LWJSize Size { get; set; }
        public LWJRect Key { get; set; }
        public LWJRect Textbox { get; set; }
        public LWJRect TitleLine { get; set; }
        public LWJFont Font { get; set; }
    }



}
