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

using RewardYourTasks.Model;

namespace RewardYourTasks
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
        }


        private void ViewSkill_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button != null)
            {
                Skill viewSkill = button.DataContext as Skill;

                this.NavigationService.Navigate(new Uri("/SkillPage.xaml/skillId=" + viewSkill.Id, UriKind.Relative));
            }
        }
    }
}