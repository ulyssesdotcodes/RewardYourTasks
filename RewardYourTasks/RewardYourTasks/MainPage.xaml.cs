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
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Globalization;
using System.ComponentModel;

namespace RewardYourTasks
{
    public partial class MainPage : PhoneApplicationPage
    {
        private TaskViewModel viewModel;
        private ObservableCollection<Task> incompleteRecurringTasks, incompleteOnceTasks;
        private ListBox heldRecurringTasks, heldOnceTasks, heldCategorys;


        public BackgroundWorker LoadTasksWorker { get; private set; }

        public MainPage()
        {
            InitializeComponent();

            this.viewModel = App.ViewModel;
            DataContext = viewModel;

            incompleteRecurringTasks = new ObservableCollection<Task>();
            incompleteOnceTasks = new ObservableCollection<Task>();

            LoadTasksWorker = new BackgroundWorker();
            LoadTasksWorker.DoWork += LoadTasksWorker_DoWork;
            LoadTasksWorker.RunWorkerCompleted += LoadTasksWorker_Completed;

            heldCategorys = this.CategoryList_ListBox;

            this.CategoryList_PanoramaItem.Content = new TextBlock { Text = "loading...", FontSize = 20, FontStyle = FontStyles.Italic };

            viewModel.LoadCollectionsWorker.RunWorkerCompleted += LoadCollectionsWorker_MainPage_Completed;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if(!viewModel.LoadCollectionsWorker.IsBusy)
                LoadTasksWorker.RunWorkerAsync();

            heldOnceTasks = this.IncompleteOnceTasks;
            heldRecurringTasks = this.IncompleteRecurringTasks;
            this.RecurringTasks_PanoramaItem.Content = new TextBlock{ Text = "loading...", FontSize=20, FontStyle = FontStyles.Italic };
            this.OnceTasks_PanoramaItem.Content = new TextBlock { Text = "loading...", FontSize = 20, FontStyle = FontStyles.Italic };
        }

        void LoadCollectionsWorker_MainPage_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            this.CategoryList_PanoramaItem.Content = this.heldCategorys;

            LoadTasksWorker.RunWorkerAsync();
        }

        void LoadTasksWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Task> recurringTasksAsync, onceTasksAsync;
            try
            {
                recurringTasksAsync = new List<Task>(viewModel.IncompleteTasks.Where<Task>(t => t.RecurringOption != Task.RecurringOptions.Never && t.When.Date <= DateTime.Now.Date).OrderBy(t => t.When));
                onceTasksAsync = new List<Task>(viewModel.IncompleteTasks.Where<Task>(t => t.RecurringOption == Task.RecurringOptions.Never && t.When.Date <= DateTime.Now.Date).OrderByDescending(t => t.When));
                
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    this.incompleteRecurringTasks = new ObservableCollection<Task>(recurringTasksAsync);
                    this.incompleteOnceTasks = new ObservableCollection<Task>(onceTasksAsync);
                    this.heldRecurringTasks.ItemsSource = this.incompleteRecurringTasks;
                    this.heldOnceTasks.ItemsSource = this.incompleteOnceTasks;
                });
            }
            catch (NullReferenceException nre)
            {
                Debug.WriteLine("No incomplete tasks");
            }
        }

        void LoadTasksWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                Debug.WriteLine(e.Error.Message);
            else
            {
                this.IncompleteOnceTasks.Visibility = Visibility.Visible;
                this.IncompleteRecurringTasks.Visibility = Visibility.Visible;
            }

            this.OnceTasks_PanoramaItem.Content = this.heldOnceTasks;
            this.RecurringTasks_PanoramaItem.Content = this.heldRecurringTasks;
        }

        private void ViewCategory_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button != null)
            {
                Category viewCategory = button.DataContext as Category;
                this.NavigationService.Navigate(new Uri("/CategoryPage.xaml?CategoryId=" + viewCategory.Id, UriKind.Relative));
            }
        }

        private void AddTask_Click(object sender, System.EventArgs e)
        {
            if(App.CheckForTrialMessage())
                this.NavigationService.Navigate(new Uri("/NewTask.xaml", UriKind.Relative));
        }

        private void EditCategoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Category updatingCategory = (sender as MenuItem).DataContext as Category;
            if (updatingCategory.Id == viewModel.All.Id)
                MessageBox.Show("Can't edit the All Category!");
            else
            {
                NavigationService.Navigate(new Uri("/NewCategory.xaml?CategoryId=" + updatingCategory.Id, UriKind.Relative));
            }
            this.viewModel.LoadCollectionsFromDatabase();
        }

        private void DeleteCategoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Category removingCategory = (sender as MenuItem).DataContext as Category;
            if (removingCategory.Id == viewModel.All.Id)
                MessageBox.Show("Can't delete the All Category!");
            else
            {
                if(MessageBox.Show("Are you sure you wish to delete " + removingCategory.Name + "?", "Delete Category", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    this.viewModel.RemoveCategory(removingCategory);
            }
            this.viewModel.LoadCollectionsFromDatabase();
        }

        private void DeleteRewardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Reward removingReward = (sender as MenuItem).DataContext as Reward;
            if (MessageBox.Show("Are you sure you wish to delete " + removingReward.Name + "?", "Delete Category", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                this.viewModel.RemoveReward(removingReward);
            this.viewModel.AllRewards.Remove(removingReward);
        }

        private void EditRewardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/NewReward.xaml?rewardId=" + ((sender as MenuItem).DataContext as Reward).Id, UriKind.Relative));
        }

        private void AddRewardButton_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/NewReward.xaml", UriKind.Relative));
        }

        private void UseRewardButton_Click(object sender, RoutedEventArgs e)
        {
            Reward r = (sender as Button).DataContext as Reward;
            if (!r.UseReward())
                MessageBox.Show("You have no " + this.Name + "rewards to use!", "None left", MessageBoxButton.OK);
        }

        private void DeleteTaskMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Task removingTask = (sender as MenuItem).DataContext as Task;

            if (MessageBox.Show("Are you sure you wish to delete " + removingTask.Name + "?", "Delete Category", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                this.viewModel.RemoveTask(removingTask);

                if (removingTask.RecurringOption == Task.RecurringOptions.Never)
                    this.incompleteOnceTasks.Remove(removingTask);
                else
                    this.incompleteRecurringTasks.Remove(removingTask);
            }
        }

        private void EditTaskMenuItem_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewTask.xaml?taskId=" + ((sender as MenuItem).DataContext as Task).Id, UriKind.Relative));
            this.viewModel.LoadCollectionsFromDatabase();
        }

        private void Complete_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox complete = sender as CheckBox;
            Task task = complete.DataContext as Task;

            if (this.viewModel.IncompleteTasks.Contains(task))
            {
                viewModel.IncompleteTasks.Remove(task);
                if (task.RecurringOption == Task.RecurringOptions.Never)
                    this.incompleteOnceTasks.Remove(task);
                else
                    this.incompleteRecurringTasks.Remove(task);
            }

            if (complete.IsChecked == true && task.IsComplete != true)
            {
                task.IsComplete = true;
                task.Category.AddPoints(task.Reward);
                task.Category.UpdateDateModified();
            }

            viewModel.SaveChangesToDB();
        }

        private void TaskGrid_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/ViewTask.xaml?taskId=" + ((sender as Grid).DataContext as Task).Id, UriKind.Relative));
        }

        private void SettingsMenuItem_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void HelpMenuItem_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/HelpPage.xaml?startItem=1", UriKind.Relative));
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/HelpPage.xaml", UriKind.Relative));
        }
    }
}