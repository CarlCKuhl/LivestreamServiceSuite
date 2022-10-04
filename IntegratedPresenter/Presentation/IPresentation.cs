﻿using CCUI_UI;

using Configurations.FeatureConfig;

using IntegratedPresenter.BMDSwitcher.Config;

using System.Collections.Generic;

namespace IntegratedPresenter.Main
{
    public interface IPresentation
    {
        ISlide After { get; }
        CCPUConfig CCPUConfig { get; }
        ISlide Current { get; }
        int CurrentSlide { get; }
        ISlide EffectiveCurrent { get; }
        string Folder { get; set; }
        bool HasCCUConfig { get; }
        bool HasSwitcherConfig { get; }
        bool HasUserConfig { get; }
        ISlide Next { get; }
        ISlide Override { get; set; }
        bool OverridePres { get; set; }
        ISlide Prev { get; }
        int SlideCount { get; }
        List<ISlide> Slides { get; set; }
        BMDSwitcherConfigSettings SwitcherConfig { get; }
        IntegratedPresenterFeatures UserConfig { get; }

        bool Create(string folder);
        void NextSlide();
        void PrevSlide();
        void SkipNextSlide();
        void SkipPrevSlide();
        void StartPres();
    }
}