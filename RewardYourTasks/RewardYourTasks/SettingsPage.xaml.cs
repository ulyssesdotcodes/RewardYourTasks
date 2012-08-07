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
using Microsoft.Phone.Controls;

namespace RewardYourTasks
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.LevelUpNotifications_ToggleSwitch.IsChecked = Settings.LevelUpNotifications.Value;
        }

        private void LevelUpNotifications_ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            Settings.LevelUpNotifications.Value = true;
        }

        private void LevelUpNotifications_ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.LevelUpNotifications.Value = false;
        }
    }

    public static class Settings
    {
        public static readonly Setting<bool> LevelUpNotifications = new Setting<bool>("LevelUpNotifications", true);
    }
}