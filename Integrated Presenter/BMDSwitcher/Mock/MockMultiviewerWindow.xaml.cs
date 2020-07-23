﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Integrated_Presenter.BMDSwitcher.Mock
{
    /// <summary>
    /// Interaction logic for MockMultiviewerWindow.xaml
    /// </summary>
    public partial class MockMultiviewerWindow : Window
    {
        public MockMultiviewerWindow()
        {
            InitializeComponent();
        }

        private int ProgramSource;
        private int PresetSource;

        private bool DSK1;
        private bool DSK2;

        private ImageSource InputSourceToImage(int inputID)
        {
            switch (inputID)
            {
                case 1:
                    return new BitmapImage(new Uri("pack://application:,,,/BMDSwitcher/Mock/Images/leftcam.png"));
                case 2:
                    return new BitmapImage(new Uri("pack://application:,,,/BMDSwitcher/Mock/Images/centercam.png"));
                case 3:
                    return new BitmapImage(new Uri("pack://application:,,,/BMDSwitcher/Mock/Images/rightcam.png"));
                case 4:
                    return new BitmapImage(new Uri("pack://application:,,,/BMDSwitcher/Mock/Images/organcam.png"));
                case 5:
                    return ImgSlide.Source;
                default:
                    return new BitmapImage(new Uri("pack://application:,,,/BMDSwitcher/Mock/Images/black.png"));
            }
        }

        public void SetPreviewSource(int inputID)
        {
            PresetSource = inputID;
            ImgPreset.Source = InputSourceToImage(inputID);
        }

        public void SetProgramSource(int inputID)
        {
            ProgramSource = inputID;
            ImgProgram.Source = InputSourceToImage(inputID);
        }

        public void UpdateAuxSource(Slide slide)
        {
            UpdateSourceFromAux(ImgSlide, slide);
            if (ProgramSource == 5)
            {
                UpdateSourceFromAux(ImgProgram, slide);
            }
            if (PresetSource == 5)
            {
                UpdateSourceFromAux(ImgPreset, slide);
            }

            if (DSK1)
            {
                UpdateSourceFromAux(ImgProgramLowerThird, slide);
            }
            if (DSK2)
            {
                UpdateSourceFromAux(ImgProgramSplit, slide);
            }
        }

        private void UpdateSourceFromAux(Image control, Slide slide)
        {
            if (slide.Type == SlideType.Video)
            {
                control.Source = new BitmapImage(new Uri("pack://application:,,,/BMDSwitcher/Mock/Images/videofile.png"));
            }
            else if (slide.Type == SlideType.Empty)
            {
                control.Source = new BitmapImage(new Uri("pack://application:,,,/BMDSwitcher/Mock/Images/black.png"));
            }
            else
            {
                control.Source = new BitmapImage(new Uri(slide.Source));
            }


        }

        public void ShowProgramDSK1()
        {
            DSK1 = true;
            ImgProgramLowerThird.Source = ImgSlide.Source;
        }

        public void HideProgramDSK1()
        {
            DSK1 = false;
            ImgProgramLowerThird.Source = null;
        }

        public void ShowProgramDSK2()
        {
            DSK2 = true;
            ImgProgramSplit.Source = ImgSlide.Source;
        }

        public void HideProgramDSK2()
        {
            DSK2 = false;
            ImgProgramSplit.Source = null;
        }


        public void SetFTB(bool black)
        {
            if (black)
            {
                ProgramFTB.Visibility = Visibility.Visible;
            }
            else
            {
                ProgramFTB.Visibility = Visibility.Hidden;
            }
        }

    }
}
