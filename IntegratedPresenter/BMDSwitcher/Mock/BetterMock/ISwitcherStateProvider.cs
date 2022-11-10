﻿using SwitcherControl.BMDSwitcher.State;

namespace IntegratedPresenter.BMDSwitcher.Mock
{
    public interface ISwitcherStateProvider
    {
        public BMDSwitcherState GetState();
        /// <summary>
        /// Reports when a animation has completed
        /// </summary>
        /// <param name="keyerID">Id of keyer: 1 = DSK1, 2 = DSK2</param>
        /// <param name="endState">State of keyer after animation. 1 = OnAir, 0 = OffAir</param>
        void ReportDSKFadeComplete(int keyerID, int endState);
        void ReportMETransitionComplete(int activeProgram, int activePreset, bool usk1State);
        void ReportFTBComplete(bool endState);
    }
}
