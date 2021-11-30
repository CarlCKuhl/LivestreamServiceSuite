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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Xenon.SlideAssembly;

namespace UIControls
{
    /// <summary>
    /// Interaction logic for LayoutTreeItem.xaml
    /// </summary>
    public partial class LayoutTreeItem : UserControl
    {

        string json;
        string name;
        string group;
        string lib;

        List<string> libs;

        bool canedit;

        SaveLayoutToLibrary SaveLayout;

        Action updateCallback;

        public LayoutTreeItem(string libname, List<string> libs, string layoutname, string layoutjson, string group, bool canedit, SaveLayoutToLibrary save, Action updatecallback)
        {
            InitializeComponent();
            name = layoutname;
            json = layoutjson;
            lib = libname;
            this.libs = libs;
            this.canedit = canedit;
            this.group = group;
            SaveLayout = save;
            tbDisplayName.Text = layoutname;
            updateCallback = updatecallback;

            if (!canedit)
            {
                btn_delete.IsEnabled = false;
                btn_delete.Visibility = Visibility.Collapsed;
            }
        }

        private void Click_EditLayout(object sender, RoutedEventArgs e)
        {
            LayoutDesigner designer = new LayoutDesigner(lib, libs, name, json, group, canedit, SaveLayout, updateCallback);
            designer.Show();
        }

        private void Click_Delete(object sender, RoutedEventArgs e)
        {

        }
    }
}
