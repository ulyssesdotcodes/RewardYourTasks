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
using RewardYourTasks.ViewModels;
using RewardYourTasks.Model;
using System.Collections.ObjectModel;
using Microsoft.Phone.Shell;

namespace RewardYourTasks
{
    public partial class CategoryPage : PhoneApplicationPage
    {
        private TaskViewModel viewModel;
        private Category currentCategory;
        private int CategoryId;
        private ObservableCollection<Task> incompleteTasks;
        private ObservableCollection<Task> completeTasks;

        public CategoryPage()
        {
            InitializeComponent();

            viewModel = App.ViewModel;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string CategoryIdString;
            this.NavigationContext.QueryString.TryGetValue("CategoryId", out CategoryIdString);
            
            CategoryId = int.Parse(CategoryIdString);

            currentCategory = viewModel.CategoryList.First(s => s.Id == CategoryId);
            this.LayoutRoot.DataContext = currentCategory;

            if (currentCategory.Id == this.viewModel.All.Id)
                ((ApplicationBarIconButton)this.ApplicationBar.Buttons[0]).IsEnabled = false;

            if (currentCategory.Id == viewModel.All.Id)
            {
                this.CategoryPivot.Items.Remove(this.AchievedTasks);
            }
            else
            {
                try
                {
                    this.incompleteTasks = new ObservableCollection<Task>(this.currentCategory.Tasks.Where<Task>(t => t.IsComplete == false).OrderByDescending(t => t.When));
                }
                catch (Exception error)
                {
                    this.incompleteTasks = new ObservableCollection<Task>();
                } 
                try
                {

                    this.completeTasks = new ObservableCollection<Task>(this.currentCategory.Tasks.Where<Task>(t => t.IsComplete == true).OrderByDescending(t => t.When));
                }
                catch (Exception)
                {
                    this.completeTasks = new ObservableCollection<Task>();
                }
                this.AllTasks.ItemsSource = incompleteTasks;
                this.CompletedTasks.ItemsSource = completeTasks;
            }

            this.Rewards_ListBox.ItemsSource = this.currentCategory.Rewards;
        }

        private void Complete_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox complete = sender as CheckBox;
            Task task = complete.DataContext as Task;

            task.Category.UpdateDateModified();

            if (complete.IsChecked == true && task.IsComplete != true)
            {
                task.IsComplete = true;
                viewModel.CompleteHandler(task);
                this.incompleteTasks.Remove(task);
                this.completeTasks.Add(task);
            }
            else if(complete.IsChecked == false && task.IsComplete == true)
            {
                task.IsComplete = false;
                viewModel.CompleteHandler(task);
                this.incompleteTasks.Add(task);
                this.completeTasks.Remove(task);
            }

            viewModel.SaveChangesToDB();
        }

        private void AddTask_Click(object sender, System.EventArgs e)
        {
            if(App.CheckForTrialMessage())
                this.NavigationService.Navigate(new Uri("/NewTask.xaml?CategoryId=" + CategoryId, UriKind.Relative));
        }

        private void UseRewardButton_Click(object sender, RoutedEventArgs e)
        {
            Reward r = (sender as Button).DataContext as Reward;
            if (!r.UseReward())
                MessageBox.Show("You have no " + this.Name + "rewards to use!", "None left", MessageBoxButton.OK);
        }

        private void AddRewardButton_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/NewReward.xaml?CategoryId=" + this.CategoryId, UriKind.Relative));
        }

        private void DeleteRewardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Reward removingReward = (sender as MenuItem).DataContext as Reward;
            if (MessageBox.Show("Are you sure you wish to delete " + removingReward.Name + "?", "Delete Category", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                this.viewModel.RemoveReward(removingReward);
                this.viewModel.AllRewards.Remove(removingReward);
            }
        }

        private void EditRewardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/NewReward.xaml?rewardId=" + ((sender as MenuItem).DataContext as Reward).Id, UriKind.Relative));
        }


        private void DeleteTaskMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Task removingTask = (sender as MenuItem).DataContext as Task;

            if (MessageBox.Show("Are you sure you wish to delete " + removingTask.Name + "?", "Delete Category", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                if (removingTask.IsComplete) completeTasks.Remove(removingTask);
                else incompleteTasks.Remove(removingTask);
                this.viewModel.RemoveTask(removingTask);
            }
        }

        private void TaskStackPanel_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/ViewTask.xaml?taskId=" + ((sender as StackPanel).DataContext as Task).Id, UriKind.Relative));
        }

        private void EditCategoryAppButton_Click(object sender, System.EventArgs e)
        {
            if (currentCategory.Id == viewModel.All.Id)
                MessageBox.Show("Can't edit the All category!");
            else
            {
                NavigationService.Navigate(new Uri("/NewCategory.xaml?CategoryId=" + currentCategory.Id, UriKind.Relative));
                this.viewModel.LoadCollectionsFromDatabase();
            }
        }

        private void EditTaskMenuItem_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewTask.xaml?taskId=" + ((sender as MenuItem).DataContext as Task).Id, UriKind.Relative));
            this.viewModel.LoadCollectionsFromDatabase();
        }
    }
}