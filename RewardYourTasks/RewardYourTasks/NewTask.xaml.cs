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
using System.Collections.ObjectModel;
using System.Collections;
using Microsoft.Phone.Tasks;

namespace RewardYourTasks
{
    public partial class NewTask : PhoneApplicationPage
    {
        private List<RewardCategory> rewardCategories;
        private List<RecurringOptionListItem> recurringOptions;
        private List<DayOfWeekSelect> daysOfWeek;
        private ObservableCollection<DayOfWeekSelect> selectedDaysOfWeek;
        int categoryCount;
        Task updatingTask;

        public NewTask()
        {
            InitializeComponent();

            rewardCategories = new List<RewardCategory>();
            rewardCategories.Add(new RewardCategory { Name = "Daily", Value = 5 });
            rewardCategories.Add(new RewardCategory { Name = "Bi-weekly", Value = 15 });
            rewardCategories.Add(new RewardCategory { Name = "Weekly", Value = 25 });
            rewardCategories.Add(new RewardCategory { Name = "Long-term goal", Value = 50 });
            rewardCategories.Add(new RewardCategory { Name = "Custom...", Value = 0 });

            this.rewardListPicker.DataContext = rewardCategories;
            
            recurringOptions = new List<RecurringOptionListItem>();
            recurringOptions.Add(new RecurringOptionListItem { Name = "once" });
            recurringOptions.Add(new RecurringOptionListItem { Name = "every day" });
            recurringOptions.Add(new RecurringOptionListItem { Name = "every other day" });
            recurringOptions.Add(new RecurringOptionListItem { Name = "every weekday" });
            recurringOptions.Add(new RecurringOptionListItem { Name = "every " + DateTime.Now.ToString("dddd") });
            recurringOptions.Add(new RecurringOptionListItem { Name = "on days of the week" });
            recurringOptions.Add(new RecurringOptionListItem { Name = "day " + DateTime.Now.Day + " of every month" });

            this.recurringOptionListPicker.DataContext = recurringOptions;

            daysOfWeek = new List<DayOfWeekSelect>();
            daysOfWeek.Add(new DayOfWeekSelect { Name = "Monday", Value = 1 });
            daysOfWeek.Add(new DayOfWeekSelect { Name = "Tuesday", Value = 2 });
            daysOfWeek.Add(new DayOfWeekSelect { Name = "Wednesday", Value = 4 });
            daysOfWeek.Add(new DayOfWeekSelect { Name = "Thursday", Value = 8 });
            daysOfWeek.Add(new DayOfWeekSelect { Name = "Friday", Value = 16 });
            daysOfWeek.Add(new DayOfWeekSelect { Name = "Saturday", Value = 32 });
            daysOfWeek.Add(new DayOfWeekSelect { Name = "Sunday", Value = 64 });

            this.DaysOfWeek_SelectList.DataContext = daysOfWeek;
            this.DaysOfWeek_SelectList.SummaryForSelectedItemsDelegate = SummarizeItems; 

            selectedDaysOfWeek = new ObservableCollection<DayOfWeekSelect>();

            this.newTaskDate_datePicker.Value = DateTime.Now;
            this.newTaskTime_timePicker.Value = DateTime.Now.AddMinutes(5 - DateTime.Now.TimeOfDay.Minutes % 5);

            categoryCount = App.ViewModel.CategoryList.Count;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.categoryListPicker.DataContext = App.ViewModel.CategoryList.Where(s => s.Id != App.ViewModel.All.Id);

            String categoryId;
            if (NavigationContext.QueryString.TryGetValue("categoryId", out categoryId))
                this.categoryListPicker.SelectedIndex = App.ViewModel.CategoryList.IndexOf(App.ViewModel.CategoryList.Single<Category>(s => s.Id == int.Parse(categoryId))) - 1;

            String taskId;
            if (updatingTask == null && NavigationContext.QueryString.TryGetValue("taskId", out taskId))
            {
                updatingTask = App.ViewModel.LoadTaskFromDatabase(int.Parse(taskId));
                this.newTaskName_TextBox.Text = updatingTask.Name;
                if (updatingTask.Notes == null)
                    updatingTask.Notes = "";
                this.newTaskNotes_TextBox.Text = updatingTask.Notes;
                this.categoryListPicker.SelectedIndex = App.ViewModel.CategoryList.IndexOf(updatingTask.Category) - 1;
                this.newTaskDate_datePicker.Value = updatingTask.When;
                this.newTaskTime_timePicker.Value = updatingTask.When;
                this.Completed_CheckBox.IsChecked = updatingTask.IsComplete;
                this.Reminders_ToggleSwitch.IsChecked = updatingTask.HasReminders;
                switch (updatingTask.Reward)
                {
                    case 5: this.rewardListPicker.SelectedIndex = 0; break;
                    case 15: this.rewardListPicker.SelectedIndex = 1; break;
                    case 25: this.rewardListPicker.SelectedIndex = 2; break;
                    case 50: this.rewardListPicker.SelectedIndex = 3; break;
                    default: this.rewardListPicker.SelectedIndex = 4; this.customReward.Visibility = Visibility.Visible; this.rewardCategoryValue_TextBlock.Text = updatingTask.Reward.ToString(); break;
                }
                switch (updatingTask.RecurringOption)
                {
                    case Task.RecurringOptions.Never: this.recurringOptionListPicker.SelectedIndex = 0; break;
                    case Task.RecurringOptions.Days:
                        if (updatingTask.RecurringAmount == 1)
                            this.recurringOptionListPicker.SelectedIndex = 1;
                        else if (updatingTask.RecurringAmount == 2)
                            this.recurringOptionListPicker.SelectedIndex = 2;
                        break;
                    case Task.RecurringOptions.DaysOfWeek:
                        if (updatingTask.RecurringAmount == 31)
                            this.recurringOptionListPicker.SelectedIndex = 3;
                        else if (updatingTask.RecurringAmount == (int)Math.Pow(2, ((int)updatingTask.When.DayOfWeek - 1)))
                        {
                            recurringOptions.RemoveAt(4);
                            recurringOptions.Insert(4, new RecurringOptionListItem { Name = "every " + updatingTask.When.ToString("dddd") });
                            this.recurringOptionListPicker.SelectedIndex = 4;
                        }
                        else
                        {
                            this.recurringOptionListPicker.SelectedIndex = 5;
                            this.DaysOfWeek_SelectList.Visibility = Visibility.Visible;

                            int localRecurringAmount = updatingTask.RecurringAmount;
                            for (int i = 6; i >= 0; i--)
                            {
                                if ((localRecurringAmount - Math.Pow(2, i)) >= 0){
                                    this.selectedDaysOfWeek.Add(daysOfWeek[i]);
                                    localRecurringAmount -= (int)Math.Pow(2, i);
                                }
                            }
                            if (selectedDaysOfWeek.Count != 0)
                            {
                                this.DaysOfWeek_SelectList.SelectedItems = selectedDaysOfWeek;
                            }
                        }
                        break;
                    case Task.RecurringOptions.Months:
                        if (updatingTask.RecurringAmount == 1)
                        {
                            recurringOptions.RemoveAt(5);
                            recurringOptions.Insert(5, new RecurringOptionListItem { Name = "day " + updatingTask.When.Day + " of every month" });
                            this.recurringOptionListPicker.SelectedIndex = 6;
                        }
                        break;
                }
            }

            if (categoryCount < App.ViewModel.CategoryList.Count)
                this.categoryListPicker.SelectedIndex = 0;
            categoryCount = App.ViewModel.CategoryList.Count;

        }

        private string SummarizeItems(IList items)
        {
            if (items != null && items.Count > 0)
            {
                string summarizedString = "";
                for (int i = 0; i < items.Count; i++)
                {
                    DayOfWeekSelect day = (items[i] as DayOfWeekSelect);
                    summarizedString += day.Name.Substring(0, 3);

                    // If not last item, add a comma to seperate them
                    if (i != items.Count - 1)
                        summarizedString += ", ";
                }

                return summarizedString;
            }
            else
                return "No days selected";
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void DoneButton_Click(object sender, System.EventArgs e)
        {
            if (this.newTaskName_TextBox.Text == "")
            {
                MessageBox.Show("Your new task needs a name!");
                return;
            }

            RewardCategory rewardCat = this.rewardListPicker.SelectedItem as RewardCategory;
            if (rewardCat.Value == 0)
            {
                if (this.rewardCategoryValue_TextBlock.Text == "") rewardCat.Value = 0;
                else rewardCat.Value = int.Parse(this.rewardCategoryValue_TextBlock.Text);
            }

            Task.RecurringOptions recurringOption = Task.RecurringOptions.Never;
            int recurringAmount = 0;

            switch (this.recurringOptionListPicker.SelectedIndex)
            {
                case 0: recurringOption = Task.RecurringOptions.Never; break;
                case 1: recurringOption = Task.RecurringOptions.Days; recurringAmount = 1; break;
                case 2: recurringOption = Task.RecurringOptions.Days; recurringAmount = 2; break;
                case 3: recurringOption = Task.RecurringOptions.DaysOfWeek; recurringAmount = 31; break;
                case 4: recurringOption = Task.RecurringOptions.DaysOfWeek; recurringAmount = (int)Math.Pow(2, ((int)DateTime.Now.DayOfWeek - 1)); break;
                case 5: 
                    recurringOption = Task.RecurringOptions.DaysOfWeek;
                    foreach (object o in this.DaysOfWeek_SelectList.SelectedItems)
                        recurringAmount += (o as DayOfWeekSelect).Value;
                    break;
                case 6: recurringOption = Task.RecurringOptions.Months; recurringAmount = 1; break;
            }

            if (updatingTask == null)
            {
                App.ViewModel.AddTask(new Task
                {
                    Name = this.newTaskName_TextBox.Text,
                    Notes = this.newTaskNotes_TextBox.Text,
                    Category = this.categoryListPicker.SelectedItem as Category,
                    When = (bool)this.Reminders_ToggleSwitch.IsChecked ? ((DateTime)this.newTaskDate_datePicker.Value).Date.Add(((DateTime)this.newTaskTime_timePicker.Value).TimeOfDay) : ((DateTime)this.newTaskDate_datePicker.Value).Date.AddSeconds(5),
                    IsComplete = (bool)this.Completed_CheckBox.IsChecked,
                    HasReminders = (bool)this.Reminders_ToggleSwitch.IsChecked,
                    Reward = rewardCat.Value,
                    RecurringOption = recurringOption,
                    RecurringAmount = recurringAmount
                });
            }
            else
            {
                updatingTask.Name = this.newTaskName_TextBox.Text;
                updatingTask.Notes = this.newTaskNotes_TextBox.Text;

                if(updatingTask.IsComplete != (bool)this.Completed_CheckBox.IsChecked)
                {
                    updatingTask.IsComplete = (bool)this.Completed_CheckBox.IsChecked;
                    App.ViewModel.CompleteHandler(updatingTask);
                }

                if(updatingTask.Category.Id != (this.categoryListPicker.SelectedItem as Category).Id)
                {
                    if (updatingTask.IsComplete)
                    {
                        updatingTask.Category.RemovePoints(updatingTask.Reward);
                        (this.categoryListPicker.SelectedItem as Category).AddPoints(rewardCat.Value);
                    }
                    updatingTask.Category.Tasks.Remove(updatingTask);
                    updatingTask.Category = (this.categoryListPicker.SelectedItem as Category);
                    updatingTask.Category.Tasks.Add(updatingTask);
                }

                updatingTask.Reward = rewardCat.Value;
                

                DateTime newWhen = ((DateTime)this.newTaskDate_datePicker.Value).Date.Add(((DateTime)this.newTaskTime_timePicker.Value).TimeOfDay);
                if(updatingTask.When != newWhen || updatingTask.RecurringOption != recurringOption || updatingTask.RecurringAmount != recurringAmount)
                {

                    updatingTask.RecurringOption = recurringOption;
                    updatingTask.RecurringAmount = recurringAmount;
                    updatingTask.When = newWhen;
                }

                if (updatingTask.HasReminders != (bool)this.Reminders_ToggleSwitch.IsChecked)
                {
                    updatingTask.HasReminders = (bool)this.Reminders_ToggleSwitch.IsChecked;

                    if (updatingTask.HasReminders)
                    {
                        updatingTask.When = updatingTask.When.Date.Add(((DateTime)this.newTaskTime_timePicker.Value).TimeOfDay);
                        updatingTask.Schedule();
                    }
                    else
                    {
                        updatingTask.When = updatingTask.When.Date.AddSeconds(5);
                        updatingTask.Unschedule();
                    }
                }

                updatingTask.Category.UpdateDateModified();

                App.ViewModel.SaveChangesToDB();
            }
            this.NavigationService.GoBack();
        }

        private void NewCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/NewCategory.xaml", UriKind.Relative));
        }

        private void RewardListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListPicker listPicker = sender as ListPicker;
            RewardCategory reward = listPicker.SelectedItem as RewardCategory;
            if (reward.Value == 0)
            {
                this.customReward.Visibility = Visibility.Visible;
            }
            else
            {
                this.customReward.Visibility = Visibility.Collapsed;
            }
        }

        private void RecurringOptionListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListPicker).SelectedIndex == 5)
                this.DaysOfWeek_SelectList.Visibility = Visibility.Visible;
            else
                this.DaysOfWeek_SelectList.Visibility = Visibility.Collapsed;
        }

        private void Reminders_ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            this.newTaskTime_timePicker.Visibility = Visibility.Visible;
        }

        private void Reminders_ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            this.newTaskTime_timePicker.Visibility = Visibility.Collapsed;
        }

        private void HelpMenuItem_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/HelpPage.xaml?startItem=2", UriKind.Relative));
        }
    }

    public class RewardCategory
    {
        private string _name;
        private int _value;

        public string Name
        {
            get { return _name; }
            set { 
                _name = value;
                DisplayName = value + " - " + Value;
            }
        }


        public int Value
        {
            get { return _value; }
            set {
                _value = value;
                if(Name != "Custom")
                    DisplayName = Name + " - " + value;
            }
        }

        public string DisplayName { get; set; }
        
    }

    public class RecurringOptionListItem
    {

        private string _name;
        
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }
    }

    public class DayOfWeekSelect
    {

        private string _name;
        private int _value;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        public int Value
        {
            get { return _value; }
            set { _value = value; } 
        }
    }
}