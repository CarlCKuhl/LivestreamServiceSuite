﻿using BMDSwitcherAPI;
using Integrated_Presenter.BMDSwitcher;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Integrated_Presenter
{

    public delegate void SwitcherStateChange(BMDSwitcherState args);

    public class BMDSwitcherManager : IBMDSwitcherManager
    {

        private IBMDSwitcherDiscovery _BMDSwitcherDiscovery;
        private IBMDSwitcher _BMDSwitcher;

        private IBMDSwitcherMixEffectBlock _BMDSwitcherMixEffectBlock1;
        private IBMDSwitcherKey _BMDSwitcherUpstreamKey1;
        private IBMDSwitcherDownstreamKey _BMDSwitcherDownstreamKey1;
        private IBMDSwitcherDownstreamKey _BMDSwitcherDownstreamKey2;
        private List<IBMDSwitcherInput> _BMDSwitcherInputs = new List<IBMDSwitcherInput>();
        private IBMDSwitcherMultiView _BMDSwitcherMultiView;

        private SwitcherMonitor _switcherMonitor;
        private MixEffectBlockMonitor _mixEffectBlockMonitor;
        private UpstreamKeyMonitor _upstreamKey1Monitor;
        private DownstreamKeyMonitor _dsk1Monitor;
        private DownstreamKeyMonitor _dsk2Monitor;
        private List<InputMonitor> _inputMonitors = new List<InputMonitor>();
        // prehaps don't need to monitor multiviewer

        private BMDSwitcherState _state;


        public event SwitcherStateChange SwitcherStateChanged;

        public bool GoodConnection { get; set; } = false;

        public Window _parent;

        public BMDSwitcherManager(Window parent)
        {
            _parent = parent;
            _switcherMonitor = new SwitcherMonitor();
            _switcherMonitor.SwitcherDisconnected += _switcherMonitor_SwitcherDisconnected;


            _mixEffectBlockMonitor = new MixEffectBlockMonitor();
            _mixEffectBlockMonitor.PreviewInputChanged += _mixEffectBlockMonitor_PreviewInputChanged;
            _mixEffectBlockMonitor.ProgramInputChanged += _mixEffectBlockMonitor_ProgramInputChanged;
            _mixEffectBlockMonitor.FateToBlackFullyChanged += _mixEffectBlockMonitor_FateToBlackFullyChanged;

            _upstreamKey1Monitor = new UpstreamKeyMonitor();
            _upstreamKey1Monitor.UpstreamKeyOnAirChanged += _upstreamKey1Monitor_UpstreamKeyOnAirChanged;

            _dsk1Monitor = new DownstreamKeyMonitor();
            _dsk1Monitor.OnAirChanged += _dsk1Manager_OnAirChanged;
            _dsk1Monitor.TieChanged += _dsk1Manager_TieChanged;
            _dsk2Monitor = new DownstreamKeyMonitor();
            _dsk2Monitor.OnAirChanged += _dsk2Manager_OnAirChanged;
            _dsk2Monitor.TieChanged += _dsk2Manager_TieChanged;

            _BMDSwitcherDiscovery = new CBMDSwitcherDiscovery();
            if (_BMDSwitcherDiscovery == null)
            {
                MessageBox.Show("Could not create Switcher Discovery Instance.\nATEM Switcher Software not found/installed", "Error");
            }
            _state = new BMDSwitcherState();
            SwitcherDisconnected();
        }

        private void _upstreamKey1Monitor_UpstreamKeyOnAirChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_USK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _mixEffectBlockMonitor_FateToBlackFullyChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_FTB();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _dsk2Manager_TieChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_DSK2();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _dsk2Manager_OnAirChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_DSK2();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _dsk1Manager_TieChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_DSK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _dsk1Manager_OnAirChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_DSK1();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _mixEffectBlockMonitor_ProgramInputChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_ProgramInput();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _mixEffectBlockMonitor_PreviewInputChanged(object sender, object args)
        {
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate_PreviewInput();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void _switcherMonitor_SwitcherDisconnected(object sender, object args)
        {
            SwitcherDisconnected();
        }

        public bool TryConnect(string address)
        {

            _BMDSwitcherConnectToFailure failReason = 0;
            try
            {
                _BMDSwitcherDiscovery.ConnectTo(address, out _BMDSwitcher, out failReason);
                SwitcherConnected();
                return true;
            }
            catch (COMException)
            {
                switch (failReason)
                {
                    case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureNoResponse:
                        MessageBox.Show("No response from Switcher", "Error");
                        break;
                    case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureIncompatibleFirmware:
                        MessageBox.Show("Switcher has incompatible firmware", "Error");
                        break;
                    default:
                        MessageBox.Show("Switcher failed to connect for unknown reason", "Error");
                        break;
                }
                return false;
            }

        }

        private bool InitializeInputSources()
        {
            // get all input sources
            IBMDSwitcherInputIterator inputIterator = null;
            IntPtr inputIteratorPtr;
            Guid inputIteratorIID = typeof(IBMDSwitcherInputIterator).GUID;
            _BMDSwitcher.CreateIterator(ref inputIteratorIID, out inputIteratorPtr);
            if (inputIteratorPtr != null)
            {
                inputIterator = (IBMDSwitcherInputIterator)Marshal.GetObjectForIUnknown(inputIteratorPtr);
            }
            else
            {
                return false;
            }
            if (inputIterator != null)
            {
                IBMDSwitcherInput input;
                inputIterator.Next(out input);
                while (input != null)
                {
                    _BMDSwitcherInputs.Add(input);
                    InputMonitor inputMonitor = new InputMonitor(input);
                    input.AddCallback(inputMonitor);
                    inputMonitor.LongNameChanged += InputMonitor_LongNameChanged;
                    inputMonitor.ShortNameChanged += InputMonitor_ShortNameChanged;
                    _inputMonitors.Add(inputMonitor);
                    inputIterator.Next(out input);
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool InitializeMultiView()
        {
            IBMDSwitcherMultiViewIterator multiViewIterator = null;
            IntPtr multiViewPtr;
            Guid multiViewIID = typeof(IBMDSwitcherMultiView).GUID;
            _BMDSwitcher.CreateIterator(ref multiViewIID, out multiViewPtr);
            if (multiViewPtr == null)
            {
                return false;
            }
            multiViewIterator = (IBMDSwitcherMultiViewIterator)Marshal.GetObjectForIUnknown(multiViewPtr);
            if (multiViewIterator == null)
            {
                return false;
            }

            multiViewIterator.Next(out _BMDSwitcherMultiView);

            return true;
        }

        private void InputMonitor_ShortNameChanged(object sender, object args)
        {
            //throw new NotImplementedException();
        }

        private void InputMonitor_LongNameChanged(object sender, object args)
        {
            //throw new NotImplementedException();
        }

        private bool InitializeMixEffectBlock()
        {
            // get mixeffectblock1
            IBMDSwitcherMixEffectBlockIterator meIterator = null;
            IntPtr meIteratorPtr;
            Guid meIteratorIID = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
            _BMDSwitcher.CreateIterator(ref meIteratorIID, out meIteratorPtr);
            if (meIteratorPtr != null)
            {
                meIterator = (IBMDSwitcherMixEffectBlockIterator)Marshal.GetObjectForIUnknown(meIteratorPtr);
            }
            if (meIterator == null)
                return false;


            meIterator.Next(out _BMDSwitcherMixEffectBlock1);

            if (_BMDSwitcherMixEffectBlock1 == null)
            {
                MessageBox.Show("Unexpected: Could not get first mix effect block", "Error");
                return false;
            }

            // add callbacks
            _BMDSwitcherMixEffectBlock1.AddCallback(_mixEffectBlockMonitor);

            return true;
        }


        private bool InitializeUpstreamKeyers()
        {
            // get upstream keyer
            IBMDSwitcherKeyIterator keyIterator = null;
            IntPtr keyIteratorPtr;
            Guid keyIteratorIID = typeof(IBMDSwitcherKeyIterator).GUID;
            _BMDSwitcher.CreateIterator(ref keyIteratorIID, out keyIteratorPtr);
            if (keyIteratorPtr != null)
            {
                keyIterator = (IBMDSwitcherKeyIterator)Marshal.GetObjectForIUnknown(keyIteratorPtr);
            }
            if (keyIterator == null)
                return false;
            if (keyIterator != null)
            {
                keyIterator.Next(out _BMDSwitcherUpstreamKey1);
            }
            if (_BMDSwitcherUpstreamKey1 == null)
            {
                MessageBox.Show("Unexpected: Could not get upstream keyer 1", "Error");
                return false;
            }

            // add callbacks
            _BMDSwitcherUpstreamKey1.AddCallback(_upstreamKey1Monitor);

            return true;
        }

        private bool InitializeDownstreamKeyers()
        {
            // get downstream keyers
            IBMDSwitcherDownstreamKeyIterator dskIterator = null;
            IntPtr dskIteratorPtr;
            Guid dskIteratorIID = typeof(IBMDSwitcherDownstreamKeyIterator).GUID;
            _BMDSwitcher.CreateIterator(ref dskIteratorIID, out dskIteratorPtr);
            if (dskIteratorPtr != null)
            {
                dskIterator = (IBMDSwitcherDownstreamKeyIterator)Marshal.GetObjectForIUnknown(dskIteratorPtr);
            }
            if (dskIterator == null)
                return false;

            if (dskIterator != null)
            {
                // get first dsk
                dskIterator.Next(out _BMDSwitcherDownstreamKey1);
            }
            if (dskIterator != null)
            {
                // get second dsk
                dskIterator.Next(out _BMDSwitcherDownstreamKey2);
            }

            if (_BMDSwitcherDownstreamKey1 == null || _BMDSwitcherDownstreamKey2 == null)
            {
                MessageBox.Show("Unexpected: Could not get one of the downstream keyers", "Error");
                return false;
            }

            // add callbacks
            _BMDSwitcherDownstreamKey1.AddCallback(_dsk1Monitor);
            _BMDSwitcherDownstreamKey2.AddCallback(_dsk2Monitor);

            return true;

        }

        private void SwitcherConnected()
        {
            // add callbacks (monitors switcher connection)
            _BMDSwitcher.AddCallback(_switcherMonitor);


            bool mixeffects = InitializeMixEffectBlock();
            //bool upstreamkeyers = InitializeUpstreamKeyers();
            bool downstreamkeyers = InitializeDownstreamKeyers();

            bool inputsources = InitializeInputSources();
            //bool multiviewer = InitializeMultiView();

            //GoodConnection = mixeffects && upstreamkeyers && downstreamkeyers;
            //GoodConnection = mixeffects && downstreamkeyers && inputsources && multiviewer;
            GoodConnection = mixeffects && downstreamkeyers && inputsources;

            MessageBox.Show("Connected to Switcher", "Connection Success");

            // update state
            _parent.Dispatcher.Invoke(() =>
            {
                ForceStateUpdate();
                SwitcherStateChanged?.Invoke(_state);
            });
        }

        private void SwitcherDisconnected()
        {
            GoodConnection = false;

            // remove callbacks
            if (_BMDSwitcherMixEffectBlock1 != null)
            {
                _BMDSwitcherMixEffectBlock1.RemoveCallback(_mixEffectBlockMonitor);
                _BMDSwitcherMixEffectBlock1 = null;
            }

            if (_BMDSwitcherUpstreamKey1 != null)
            {
                _BMDSwitcherUpstreamKey1.RemoveCallback(_upstreamKey1Monitor);
                _BMDSwitcherUpstreamKey1 = null;
            }

            if (_BMDSwitcherDownstreamKey1 != null)
            {
                _BMDSwitcherDownstreamKey1.RemoveCallback(_dsk1Monitor);
                _BMDSwitcherDownstreamKey1 = null;
            }
            if (_BMDSwitcherDownstreamKey2 != null)
            {
                _BMDSwitcherDownstreamKey2.RemoveCallback(_dsk2Monitor);
                _BMDSwitcherDownstreamKey2 = null;
            }

            if (_BMDSwitcher != null)
            {
                _BMDSwitcher.RemoveCallback(_switcherMonitor);
                _switcherMonitor = null;
            }

            int i = 0;
            foreach (var input in _BMDSwitcherInputs)
            {
                input.RemoveCallback(_inputMonitors[i++]);
            }
            _inputMonitors.Clear();
            _BMDSwitcherInputs.Clear();

            if (_BMDSwitcherMultiView != null)
            {
                // no callback yet
                _BMDSwitcherMultiView = null;
            }

        }



        public BMDSwitcherState ForceStateUpdate()
        {
            if (GoodConnection)
            {
                // update state
                ForceStateUpdate_ProgramInput();
                ForceStateUpdate_PreviewInput();
                //ForceStateUpdate_USK1();
                ForceStateUpdate_DSK1();
                ForceStateUpdate_DSK2();
                ForceStateUpdate_FTB();
            }
            return _state;
        }

        private void ForceStateUpdate_USK1()
        {
            //int onair;
            //_BMDSwitcherUpstreamKey1.GetOnAir(out onair);
            //_state.USK1OnAir = onair != 0;
        }

        private void ForceStateUpdate_DSK1()
        {
            int dsk1onair;
            int dsk1tie;
            _BMDSwitcherDownstreamKey1.GetOnAir(out dsk1onair);
            _BMDSwitcherDownstreamKey1.GetTie(out dsk1tie);

            _state.DSK1OnAir = dsk1onair != 0;
            _state.DSK1Tie = dsk1tie != 0;
        }
        private void ForceStateUpdate_DSK2()
        {
            int dsk2onair;
            int dsk2tie;
            _BMDSwitcherDownstreamKey2.GetOnAir(out dsk2onair);
            _BMDSwitcherDownstreamKey2.GetTie(out dsk2tie);

            _state.DSK2OnAir = dsk2onair != 0;
            _state.DSK2Tie = dsk2tie != 0;
        }
        private void ForceStateUpdate_ProgramInput()
        {
            // get current program source
            long programid;
            _BMDSwitcherMixEffectBlock1.GetProgramInput(out programid);
            _state.ProgramID = programid;
        }

        private void ForceStateUpdate_PreviewInput()
        {
            // get current preset source
            long presetid;
            _BMDSwitcherMixEffectBlock1.GetPreviewInput(out presetid);
            _state.PresetID = presetid;
        }

        private void ForceStateUpdate_FTB()
        {
            int ftb;
            _BMDSwitcherMixEffectBlock1.GetFadeToBlackFullyBlack(out ftb);
            _state.FTB = ftb != 0;
        }

        public BMDSwitcherState GetCurrentState()
        {
            return _state;
        }



        public void ConfigureSwitcher()
        {
            ConfigureMixEffectBlock();
            ConfigureCameraSources();
            ConfigureDownstreamKeys();
        }

        private void ConfigureMixEffectBlock()
        {
            _BMDSwitcherMixEffectBlock1.SetFadeToBlackRate(30);
        }

        private void ConfigureCameraSources()
        {
            // set input source names
            foreach (var inputsource in _BMDSwitcherInputs)
            {
                long sourceid;
                inputsource.GetInputId(out sourceid);

                // rename it if required
                if (sourceid == (long)BMDSwitcherSources.Input5)
                {
                    inputsource.SetLongName("LECTERN");
                    inputsource.SetShortName("LTRN");
                }
                if (sourceid == (long)BMDSwitcherSources.Input1)
                {
                    inputsource.SetLongName("CENTER");
                    inputsource.SetShortName("CNTR");
                }
                if (sourceid == (long)BMDSwitcherSources.Input6)
                {
                    inputsource.SetLongName("PULPIT");
                    inputsource.SetShortName("PLPT");
                }
                if (sourceid == (long)BMDSwitcherSources.Input2)
                {
                    inputsource.SetLongName("ORGAN");
                    inputsource.SetShortName("ORGN");
                }
                if (sourceid == (long)BMDSwitcherSources.Input4)
                {
                    inputsource.SetLongName("SLIDESHOW");
                    inputsource.SetShortName("SLIDE");
                }
            }
        }

        private void ConfigureDownstreamKeys()
        {
            ConfigureDSK1Liturgy();
            ConfigureDSK2Split();
        }

        private void ConfigureDSK1Liturgy()
        {
            _BMDSwitcherDownstreamKey1.SetInputFill((long)BMDSwitcherSources.Input4);
            _BMDSwitcherDownstreamKey1.SetInputCut((long)BMDSwitcherSources.MediaPlayer1);
            _BMDSwitcherDownstreamKey1.SetRate(30);
            _BMDSwitcherDownstreamKey1.SetPreMultiplied(0);
            _BMDSwitcherDownstreamKey1.SetMasked(0);
        }

        private void ConfigureDSK2Split()
        {
            _BMDSwitcherDownstreamKey2.SetInputFill((long)BMDSwitcherSources.Input4);
            _BMDSwitcherDownstreamKey2.SetInputCut((long)BMDSwitcherSources.MediaPlayer2);
            _BMDSwitcherDownstreamKey2.SetRate(30);
            _BMDSwitcherDownstreamKey2.SetPreMultiplied(0);
            _BMDSwitcherDownstreamKey2.SetMasked(0);
        }



        public void PerformPresetSelect(int sourceID)
        {
            if (GoodConnection)
            {
                _BMDSwitcherMixEffectBlock1.SetPreviewInput((long)sourceID);
            }
        }

        public void PerformProgramSelect(int sourceID)
        {
            if (GoodConnection)
            {
                _BMDSwitcherMixEffectBlock1.SetProgramInput((long)sourceID);
            }
        }

        public void PerformCutTransition()
        {
            if (GoodConnection)
            {
                _BMDSwitcherMixEffectBlock1.PerformCut();
            }
        }

        public void PerformAutoTransition()
        {
            if (GoodConnection)
            {
                _BMDSwitcherMixEffectBlock1.PerformAutoTransition();
            }
        }

        public void PerformToggleUSK1()
        {
            _BMDSwitcherUpstreamKey1.SetOnAir(_state.USK1OnAir ? 0 : 1);
        }

        public void PerformToggleDSK1()
        {
            _BMDSwitcherDownstreamKey1.SetOnAir(_state.DSK1OnAir ? 0 : 1);
        }
        public void PerformTieDSK1()
        {
            _BMDSwitcherDownstreamKey1.SetTie(_state.DSK1Tie ? 0 : 1);
        }
        public void PerformTakeAutoDSK1()
        {
            _BMDSwitcherDownstreamKey1.PerformAutoTransition();
        }
        public void PerformAutoOnAirDSK1()
        {
            _BMDSwitcherDownstreamKey1.PerformAutoTransitionInDirection(1);
        }
        public void PerformAutoOffAirDSK1()
        {
            _BMDSwitcherDownstreamKey1.PerformAutoTransitionInDirection(0);
        }
        public void PerformToggleDSK2()
        {
            _BMDSwitcherDownstreamKey2.SetOnAir(_state.DSK2OnAir ? 0 : 1);
        }
        public void PerformTieDSK2()
        {
            _BMDSwitcherDownstreamKey2.SetTie(_state.DSK2Tie ? 0 : 1);
        }
        public void PerformTakeAutoDSK2()
        {
            _BMDSwitcherDownstreamKey2.PerformAutoTransition();
        }
        public void PerformAutoOnAirDSK2()
        {
            _BMDSwitcherDownstreamKey2.PerformAutoTransitionInDirection(1);
        }
        public void PerformAutoOffAirDSK2()
        {
            _BMDSwitcherDownstreamKey2.PerformAutoTransitionInDirection(0);
        }

        public void PerformToggleFTB()
        {
            _BMDSwitcherMixEffectBlock1.PerformFadeToBlack();
        }

        public void Close()
        {
            SwitcherDisconnected();
        }
    }
}
