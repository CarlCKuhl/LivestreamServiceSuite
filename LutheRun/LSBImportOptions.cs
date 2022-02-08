﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LutheRun
{
    public class LSBImportOptions
    {
        public bool InferPostset { get; set; } = true;
        public bool UseUpNextForHymns { get; set; } = true;
        public bool OnlyKnownCaptions { get; set; } = true;
        public bool UseResponsiveLiturgy { get; set; } = true;
        public bool UseComplexReading { get; set; } = true;

        public LSBElementFilter Filter { get; set; } = new LSBElementFilter();
    }

    public class BoolSettingAttribute : Attribute
    {

    }

    public class LSBElementFilter
    {
        [BoolSetting]
        public bool Liturgy { get; set; } = true;
        [BoolSetting]
        public bool SungLiturgy { get; set; } = true;
        [BoolSetting]
        public bool Reading { get; set; } = true;
        [BoolSetting]
        public bool ComplexReading { get; set; } = true;
        [BoolSetting]
        public bool Caption { get; set; } = true;
        [BoolSetting]
        public bool Introit { get; set; } = true;
        [BoolSetting]
        public bool Hymn { get; set; } = true;
        [BoolSetting]
        public bool Prefab { get; set; } = true;
        [BoolSetting]
        public bool Unknown { get; set; } = false;
        [BoolSetting]
        public bool Acknowledgments { get; set; } = false;

        public List<Type> FilteredTypes
        {
            get
            {
                List<Type> elements = new List<Type>();
                if (Liturgy)
                    elements.Add(typeof(LSBElementLiturgy));
                if (SungLiturgy)
                    elements.Add(typeof(LSBElementLiturgySung));
                if (Reading)
                    elements.Add(typeof(LSBElementReading));
                if (ComplexReading)
                    elements.Add(typeof(LSBElementReadingComplex));
                if (Caption)
                    elements.Add(typeof(LSBElementCaption));
                if (Introit)
                    elements.Add(typeof(LSBElementIntroit));
                if (Hymn)
                    elements.Add(typeof(LSBElementHymn));
                if (Prefab)
                    elements.Add(typeof(LSBElementIsPrefab));
                if (Unknown)
                    elements.Add(typeof(LSBElementUnknown));
                if (Acknowledgments)
                    elements.Add(typeof(LSBElementAcknowledments));

                return elements;
            }
        }
    }

}
