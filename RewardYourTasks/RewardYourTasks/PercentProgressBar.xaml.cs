using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

namespace RewardYourTasks
{
    public partial class PercentProgressBar : UserControl
    {
        private int _progressWidth;
        private string _percentString;

        public static readonly DependencyProperty PercentCompleteProperty = DependencyProperty.Register(
            "PercentComplete",
            typeof(int),
            typeof(PercentProgressBar),
            new PropertyMetadata(0, new PropertyChangedCallback(UpdatePercent))
            );

        private static void UpdatePercent(object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as PercentProgressBar).PercentComplete = (int)e.NewValue;
        }


        public PercentProgressBar()
        {
            InitializeComponent();

            this.LayoutRoot.DataContext = this;

            if (DesignerProperties.IsInDesignTool)
                PercentComplete = 50;

            Dispatcher.BeginInvoke(delegate
            {
                LoadNewPercent();
            });
        }

        public int PercentComplete
        {
            get { return (int)GetValue(PercentCompleteProperty); }
            set {
                SetValue(PercentCompleteProperty, value);
                Dispatcher.BeginInvoke(delegate
                {
                    LoadNewPercent();
                });
            }
        }


        public int ProgressWidth
        {
            get { return _progressWidth; }
            set
            {
                _progressWidth = value;
                this.Progress.Width = value;
            }
        }

        public string PercentString
        {
            get { return _percentString; }
            set
            {
                _percentString = value;
                this.PercentTextBlock.Text = value;
            }
        }

        public void LoadNewPercent()
        {
            ProgressWidth = (int)((PercentComplete / 100.0) * this.EmptyBar.ActualWidth);
            PercentString = PercentComplete + "%";
        }
    }
}
