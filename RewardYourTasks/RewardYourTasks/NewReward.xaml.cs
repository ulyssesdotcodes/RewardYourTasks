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
using System.Windows.Data;
using System.Diagnostics;
using RewardYourTasks.ViewModels;

namespace RewardYourTasks
{
    public partial class NewReward : PhoneApplicationPage
    {
        int categoryCount;
        Reward updatingReward;
        TaskViewModel viewModel;
        public NewReward()
        {
            InitializeComponent();

            this.viewModel = App.ViewModel;

            categoryCount = this.viewModel.CategoryList.Count;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.categoryListPicker.DataContext = App.ViewModel.CategoryList;

            String categoryId;
            if (NavigationContext.QueryString.TryGetValue("categoryId", out categoryId))
                this.categoryListPicker.SelectedIndex = App.ViewModel.CategoryList.IndexOf(App.ViewModel.CategoryList.Single<Category>(s => s.Id == int.Parse(categoryId)));

            if (categoryCount < App.ViewModel.CategoryList.Count)
                this.categoryListPicker.SelectedIndex = 0;

            categoryCount = App.ViewModel.CategoryList.Count;
            
            String rewardId;
            if (NavigationContext.QueryString.TryGetValue("rewardId", out rewardId))
            {
                updatingReward = this.viewModel.AllRewards.Single<Reward>(r => r.Id == int.Parse(rewardId));
                this.newRewardName_TextBox.Text = updatingReward.Name;
                this.categoryListPicker.SelectedIndex = this.viewModel.CategoryList.IndexOf(updatingReward.Category);
                this.PointsPerReward_TextBox.Text = updatingReward.PointsPerReward.ToString();
            }
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void DoneButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (int.Parse(this.PointsPerReward_TextBox.Text) == 0)
                {
                    MessageBox.Show("It's not a reward if it's rewarded every 0 points!");
                    return;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("You need to enter the number of points needed for the reward.");
                return;
            }

            if (updatingReward == null)
            {
                App.ViewModel.AddReward(new Reward
                {
                    Name = this.newRewardName_TextBox.Text,
                    Category = this.categoryListPicker.SelectedItem as Category,
                    PointsPerReward = int.Parse(this.PointsPerReward_TextBox.Text)
                });
            }
            else
            {
                updatingReward.Name = this.newRewardName_TextBox.Text;
                updatingReward.Category = this.categoryListPicker.SelectedItem as Category;
                updatingReward.PointsPerReward = int.Parse(this.PointsPerReward_TextBox.Text);
                this.viewModel.SaveChangesToDB();
            }
            this.NavigationService.GoBack();
        }

        private void NewCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/NewCategory.xaml", UriKind.Relative));
        }

        private void HelpMenuItem_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/HelpPage.xaml?startItem=3", UriKind.Relative));
        }
    }
}