﻿using AngleSharp;
using AngleSharp.Dom;

using LutheRun;
using LutheRun.Wizard;

using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Xenon.SlideAssembly;

using IO = System.IO;

namespace SlideCreater
{
    /// <summary>
    /// Interaction logic for LSBImportWizardUI.xaml
    /// </summary>
    public partial class LSBImportWizardUI : Window
    {
        string m_serviceFilename;
        public LSBImportWizardUI(string filename)
        {
            InitializeComponent();
            m_serviceFilename = filename;

            Task.Run(LoadAndBuild);
        }

        List<IElement> elems = new List<IElement>();
        private async Task LoadAndBuild()
        {
            LSBParser parser = new LSBParser();
            parser.LSBImportOptions = new LSBImportOptions();
            await parser.ParseHTML(m_serviceFilename);
            parser.Serviceify(parser.LSBImportOptions);

            var rc = new LSBReCompiler();
            string tmpFile = IO.Path.GetTempFileName() + "lsbrecompile.html" + Guid.NewGuid().ToString() + ".html";


            parser.LSBImportOptions.Macros = IProjectLayoutLibraryManager.GetDefaultBundledLibraries().FirstOrDefault(x => x.LibName == "Xenon.CommonColored")?.Macros ?? new Dictionary<string, string>();

            rc.CompileToXenonMappedToSource(m_serviceFilename, parser.LSBImportOptions, parser.ServiceElements);

            var tmpCss = IO.Path.GetTempFileName() + "lsbrecompile.css" + Guid.NewGuid().ToString() + ".css";
            try
            {
                var path = IO.Path.GetDirectoryName(m_serviceFilename);
                var chromeDownload = IO.Path.Combine(path, IO.Path.GetFileNameWithoutExtension(m_serviceFilename) + "_files");

                var files = IO.Directory.GetFiles(chromeDownload);
                // find the app-guid.css file
                var file = files.First(f => IO.Path.GetFileName(f).StartsWith("app-") && f.EndsWith(".css"));

                File.Copy(file, tmpCss);
            }
            catch (Exception ex)
            {
                throw;
            }

            await File.WriteAllTextAsync(tmpFile, rc.GenerateHTMLReport(parser.ServiceElements, tmpCss));


            Dispatcher.Invoke(() =>
            {
                browser.Source = new Uri(tmpFile);
            });
        }

    }
}
