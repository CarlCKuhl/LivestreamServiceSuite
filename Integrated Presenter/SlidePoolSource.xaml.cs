﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Integrated_Presenter
{
    /// <summary>
    /// Interaction logic for SlidePoolSource.xaml
    /// </summary>
    public partial class SlidePoolSource : UserControl
    {
        public SlidePoolSource()
        {
            InitializeComponent();
        }

        private SlideType type = SlideType.Full;
        public SlideType Type
        {
            get => type;
            set
            {
                type = value;
                switch (type)
                {
                    case SlideType.Full:
                        btnStill.Background = Brushes.Orange;
                        btnStill.Foreground = Brushes.Orange;
                        btnLiturgy.Background = Brushes.WhiteSmoke;
                        btnLiturgy.Foreground = Brushes.WhiteSmoke;
                        btnVideo.Background = Brushes.WhiteSmoke;
                        btnVideo.Foreground = Brushes.WhiteSmoke;
                        break;
                    case SlideType.Liturgy:
                        btnStill.Background = Brushes.WhiteSmoke;
                        btnStill.Foreground = Brushes.WhiteSmoke;
                        btnLiturgy.Background = Brushes.Orange;
                        btnLiturgy.Foreground = Brushes.Orange;
                        btnVideo.Background = Brushes.WhiteSmoke;
                        btnVideo.Foreground = Brushes.WhiteSmoke;
                        break;
                    case SlideType.Video:
                        btnStill.Background = Brushes.WhiteSmoke;
                        btnStill.Foreground = Brushes.WhiteSmoke;
                        btnLiturgy.Background = Brushes.WhiteSmoke;
                        btnLiturgy.Foreground = Brushes.WhiteSmoke;
                        btnVideo.Background = Brushes.Orange;
                        btnVideo.Foreground = Brushes.Orange;
                        break;
                    case SlideType.Empty:
                        break;
                    default:
                        break;
                }
            }
        }
        public Uri Source;
        private bool selected = false;
        public bool Selected
        {
            get => selected; set
            {
                selected = value;
                SelectedChanged();
            }
        }

        public Slide Slide { get; private set; }

        private bool loaded = false;

        private void SelectedChanged()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (selected)
                {
                    border.BorderBrush = Brushes.Red;
                }
                else
                {
                    border.BorderBrush = Brushes.Gray;
                }

                if (loaded)
                {
                    BtnTakeInsert.Background = Application.Current.FindResource("GrayLight") as RadialGradientBrush;
                    BtnTakeReplace.Background = Application.Current.FindResource("GrayLight") as RadialGradientBrush;
                }
                else
                {
                    BtnTakeInsert.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
                    BtnTakeReplace.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
                }
            });
        }

        private void ClickSlideMode(object sender, RoutedEventArgs e)
        {
            Type = SlideType.Full;
        }

        private void ClickLiturgyMode(object sender, RoutedEventArgs e)
        {
            Type = SlideType.Liturgy;
        }

        private void ClickVideoMode(object sender, RoutedEventArgs e)
        {
            Type = SlideType.Video;
        }

        public void PlayMedia()
        {
            mediapreview.videoPlayer.Volume = 0;
            mediapreview.PlayMedia();
        }

        public void PauseMedia()
        {
            mediapreview.videoPlayer.Volume = 0;
            mediapreview.PauseMedia();
        }

        public void StopMedia()
        {
            mediapreview.videoPlayer.Volume = 0;
            mediapreview.StopMedia();
        }

        public void RestartMedia()
        {
            mediapreview.videoPlayer.Volume = 0;
            mediapreview.ReplayMedia();
        }

        private void ClickLoadMedia(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Media File";
            ofd.Filter = "Images and Video(*.mp4;*.png)|*.mp4;*.png";
            if (ofd.ShowDialog() == true)
            {

                string file = ofd.FileName;
                var filename = Regex.Match(System.IO.Path.GetFileName(file), "\\d+_(?<type>[^-]*)-?(?<action>.*)\\..*");
                if (filename.Success)
                {
                    string name = filename.Groups["type"].Value;
                    string action = filename.Groups["action"].Value;
                    // look at the name to determine the type
                    switch (name)
                    {
                        case "Full":
                            Type = SlideType.Full;
                            break;
                        case "Liturgy":
                            Type = SlideType.Liturgy;
                            break;
                        case "Video":
                            Type = SlideType.Video;
                            break;
                        default:
                            type = SlideType.Empty;
                            break;
                    }
                    Source = new Uri(ofd.FileName);
                    Slide = new Slide() { Action = action, Guid = Guid.NewGuid(), Source = ofd.FileName, Type = Type, };
                }
                else
                {
                    Source = new Uri(ofd.FileName);
                    string ext = System.IO.Path.GetExtension(ofd.FileName);
                    Type = SlideType.Full;
                    if (ext == ".mp4" || ext == ".MP4")
                    {
                        Type = SlideType.Video;
                    }
                    Slide = new Slide() { Action = "", Guid = Guid.NewGuid(), Source = ofd.FileName, Type = Type };
                }

                mediapreview.SetMedia(Source, Type);
                BtnTakeInsert.Style = (Style)Application.Current.FindResource("SwitcherButton");
                BtnTakeReplace.Style = (Style)Application.Current.FindResource("SwitcherButton");
                loaded = true;

                border.BorderBrush = Brushes.LightBlue;

            }
        }

        private void ClickTakeInsert(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                Selected = !Selected;
                TakeSlidePoolSource?.Invoke(this, Slide, false);
            }
        }

        private void ClickTakeReplace(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                Selected = !Selected;
                TakeSlidePoolSource?.Invoke(this, Slide, true);
            }
        }


        //public event EventHandler ClickTakeEvent;
        public event TakeSlidePoolEvent TakeSlidePoolSource;

    }

    public delegate void TakeSlidePoolEvent(object sender, Slide s, bool replaceMode);

}
