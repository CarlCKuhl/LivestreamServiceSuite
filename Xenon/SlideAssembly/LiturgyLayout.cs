﻿using System.Drawing;
using System.Text.Json.Serialization;

using Xenon.LayoutEngine;

namespace Xenon.SlideAssembly
{
    public class LiturgyLayout
    {
        public Size Size { get; set; }
        public Rectangle Key { get; set; }
        public Rectangle Speaker { get; set; }
        public Rectangle Text { get; set; }
        public int InterLineSpacing { get; set; }
        [JsonIgnore]
        public Font Font { get; set; }

        public LiturgyLayout()
        {
            Font = new Font("Arial", 36, FontStyle.Regular);
        }

        public LiturgyLayoutRenderInfo GetRenderInfo()
        {
            var res = new LiturgyLayoutRenderInfo
            {
                BoldFont = new Font(Font, FontStyle.Bold),
                RegularFont = Font,
                SpeakerBox = Speaker,
                TextBox = Text
            };
            return res;
        }
    }
}
