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
using Microsoft.Phone.Shell;
using System.Globalization;

namespace RewardYourTasks
{
    public partial class ViewTask : PhoneApplicationPage
    {
        Task viewingTask;
        public ViewTask()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


            String taskId;
            if (NavigationContext.QueryString.TryGetValue("taskId", out taskId))
            {
                this.viewingTask = App.ViewModel.LoadTaskFromDatabase(int.Parse(taskId));
                this.DataContext = this.viewingTask;
                this.Repeats_TextBlock.Text = viewingTask.RecurringOption == Task.RecurringOptions.Never ? "Occurs only once" : "Repeats next on " + viewingTask.Next.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("/yyyy", ""));
                if (viewingTask.IsComplete)
                    (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;
                this.Reminders_TextBlock.Text = viewingTask.HasReminders ? "ON" : "OFF";
            }
        }

        private void EditTaskButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewTask.xaml?taskId=" + viewingTask.Id, UriKind.Relative));
            App.ViewModel.LoadCollectionsFromDatabase();
        }

        private void DeleteTaskButton_Click(object sender, System.EventArgs e)
        {
            if (MessageBox.Show("Are you sure you wish to delete " + viewingTask.Name + "?", "Delete Category", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                App.ViewModel.RemoveTask(viewingTask);
                App.ViewModel.LoadCollectionsFromDatabase();

                this.NavigationService.GoBack();
            }
        }

        private void CompleteTaskButton_Click(object sender, System.EventArgs e)
        {
            viewingTask.IsComplete = true;
            viewingTask.Category.AddPoints(viewingTask.Reward);

            App.ViewModel.LoadCollectionsFromDatabase();

            (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;
        }
    }
}