﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Nomad.Communication.EventAggregation;
using Nomad.Communication.ServiceLocation;
using Nomad.Regions;
using Menu = System.Windows.Controls.Menu;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using UserControl = System.Windows.Controls.UserControl;

namespace FileLoaderModule
{
    /// <summary>
    /// Interaction logic for SelectFileView.xaml
    /// </summary>
    public partial class SelectFileView : UserControl
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly IEventAggregator _eventAggregator;


        public SelectFileView(IServiceLocator serviceLocator, IEventAggregator eventAggregator)
        {
            _serviceLocator = serviceLocator;
            _eventAggregator = eventAggregator;
            InitializeComponent();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            if(fileDialog.ShowDialog() == DialogResult.OK)
            {
                _eventAggregator.Publish(new FileSelectedMessage(fileDialog.FileName));
            }
        }


        private bool m_initialized;

        private void InitializeMenuClick(object sender, RoutedEventArgs e)
        {
            if(!m_initialized)
                InitializeMenu();
            m_initialized = true;
        }

        private void InitializeMenu()
        {
            var myMenu = new System.Windows.Controls.MenuItem() { Header = "FileLoader" }; // this one will be registered in region manager, after that event will be sent
            var regionManager = _serviceLocator.Resolve<RegionManager>();
            var menuRegion = regionManager.GetRegion("mainMenu");
            menuRegion.AddView(myMenu);

            
            regionManager.AttachRegion("FileLoaderMenu", myMenu);

            var region = regionManager.GetRegion("FileLoaderMenu");
            var about = new System.Windows.Controls.MenuItem() { Header = "About" };
            about.Click += (x, y) => System.Windows.Forms.MessageBox.Show("File Loader 1.0.1");
            
            region.AddView(about);

            

            _eventAggregator.Publish(new FileLoaderMenuRegionRegisteredMessage("FileLoaderMenu"));
        }
    }
}
