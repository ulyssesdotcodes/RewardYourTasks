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
using RewardYourTasks.ViewModels;
using System.Diagnostics;

namespace RewardYourTasks
{
    public partial class MainPage : PhoneApplicationPage
    {
        private TaskViewModel viewModel;

        public MainPage()
        {
            InitializeComponent();

            this.viewModel = App.ViewModel;
            DataContext = viewModel;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.DataContext = viewModel;
        }

        private void ViewSkill_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button != null)
            {
                Skill viewSkill = button.DataContext as Skill;

                this.NavigationService.Navigate(new Uri("/SkillPage.xaml?skillId=" + viewSkill.Id, UriKind.Relative));
            }
        }

        private void AddTask_Click(object sender, System.EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/NewTask.xaml", UriKind.Relative));
        }

        private void DeleteSkillMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Skill deletingSkill = (sender as MenuItem).DataContext as Skill;
            this.viewModel.RemovePointsFromSkill(deletingSkill, deletingSkill.CurrentPoints);
        }
    }
}