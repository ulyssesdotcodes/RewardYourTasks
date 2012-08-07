using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using RewardYourTasks.Model;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System.Windows;
using System.Threading;
using System.Windows.Threading;

namespace RewardYourTasks.ViewModels
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private TasksDataContext taskDB;

        public BackgroundWorker LoadCollectionsWorker { get; private set; }

        public BackgroundWorker AddPointsToCategoryWorker { get; private set; }
        public int PointsToAdd { get; set; }
        public Category CategoryToAddTo { get; set; }

        public Category CategoryToUpdate { get; set; }

        public List<String> CategoriesLeveledUp { get; set; }

        public TaskViewModel(string taskDBConnectionString)
        {
            taskDB = new TasksDataContext(taskDBConnectionString);

            CategoryList = new ObservableCollection<Category>();

            AllRewards = new ObservableCollection<Reward>();

            IncompleteTasks = new ObservableCollection<Task>();

            CategoryTasks = new ObservableCollection<Task>();

            ToBeChanged = new ObservableCollection<object>();

            CategoriesLeveledUp = new List<String>();


            //Create App-wide BackgroundWorkers
            LoadCollectionsWorker = new BackgroundWorker();
            LoadCollectionsWorker.DoWork += LoadCollectionsWorker_DoWork;
            LoadCollectionsWorker.RunWorkerCompleted += LoadCollectionsWorker_Completed;

            AddPointsToCategoryWorker = new BackgroundWorker();
            AddPointsToCategoryWorker.DoWork += AddPointsToCategoryWorker_DoWork;
            AddPointsToCategoryWorker.RunWorkerCompleted += AddPointsToCategoryWorker_RunWorkerCompleted;
        }

        public void SaveChangesToDB()
        {
            taskDB.SubmitChanges();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify Silverlight that a property has changed.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private Category _all;
        public Category All
        {
            get { return _all; }
            set { _all = value; }
        }

        private ObservableCollection<Category> _CategoryList;
        public ObservableCollection<Category> CategoryList
        {
            get { return _CategoryList; }
            set
            {
                _CategoryList = value;
                NotifyPropertyChanged("CategoryList");
            }
        }

        private ObservableCollection<Reward> _allRewards;
        public ObservableCollection<Reward> AllRewards
        {
            get { return _allRewards; }
            set
            {
                _allRewards = value;
                NotifyPropertyChanged("AllRewards");
            }
        }

        private ObservableCollection<Task> _categoryTasks;
        public ObservableCollection<Task> CategoryTasks
        {
            get { return _categoryTasks; }
            set
            {
                _categoryTasks = value;
                NotifyPropertyChanged("CategoryTasks");
            }
        }

        private ObservableCollection<Task> _incompleteTasks;
        public ObservableCollection<Task> IncompleteTasks
        {
            get { return _incompleteTasks; }
            set
            {
                _incompleteTasks = value;
                NotifyPropertyChanged("IncompleteTasks");
            }
        }

        private ObservableCollection<object> _toBeChanged;
        public ObservableCollection<object> ToBeChanged
        {
            get { return _toBeChanged; }
            set
            {
                _toBeChanged = value;
                NotifyPropertyChanged("ToBeChanged");
            }
        }

        public void LoadCollectionsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Category> CategoryListAsync = new List<Category>();
            List<Reward> AllRewardsAsync = new List<Reward>();
            List<Task> IncompleteTasksAsync = new List<Task>();

            var CategorysInDB = from Category c in taskDB.Categories orderby c.DateModified descending select c;

            foreach (Category Category in CategorysInDB)
            {
                if (Category.Name == "All")
                {
                    All = Category;
                    CategoryListAsync.Insert(0, Category);
                }
                else
                {
                    CategoryListAsync.Add(Category);
                }

                foreach (Reward r in Category.Rewards)
                    AllRewardsAsync.Add(r);

                foreach (Task t in Category.Tasks)
                {
                    t.UpdateTask();
                }
            }

            var IncompleteTasksInDB = from Task t in taskDB.Tasks where t.IsComplete == false select t;

            foreach (Task t in IncompleteTasksInDB)
                IncompleteTasksAsync.Add(t);

            AddFromChangedTasks();

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.CategoryList = new ObservableCollection<Category>(CategoryListAsync);
                this.AllRewards = new ObservableCollection<Reward>(AllRewardsAsync);
                this.IncompleteTasks = new ObservableCollection<Task>(IncompleteTasksAsync);
            });
        }

        void LoadCollectionsWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                Debug.WriteLine("Error " + e.Error.Message);
        }

        public void LoadCollectionsFromDatabase()
        {
            CategoryList = new ObservableCollection<Category>();
            AllRewards = new ObservableCollection<Reward>();
            IncompleteTasks = new ObservableCollection<Task>();

            var CategorysInDB = from Category c in taskDB.Categories orderby c.DateModified descending select c;

            foreach (Category Category in CategorysInDB)
            {
                if (Category.Name == "All")
                {
                    All = Category;
                    CategoryList.Insert(0, Category);
                }
                else
                {
                    CategoryList.Add(Category);
                }

                foreach (Reward r in Category.Rewards)
                    AllRewards.Add(r);

                foreach (Task t in Category.Tasks)
                {
                    if (!t.IsComplete)
                        IncompleteTasks.Add(t);
                }
            }
        }

        public void LoadCategoryTasksFromDatebase(Category cat)
        {
            CategoryTasks = new ObservableCollection<Task>();

            var CategoryTasksInDB = from Task t in taskDB.Tasks where t.Category.Id == cat.Id orderby t.When descending select t;

            foreach (Task t in CategoryTasksInDB)
                CategoryTasks.Add(t);
        }

        public Task LoadTaskFromDatabase(int id)
        {
            var taskInDB = from Task t in taskDB.Tasks where t.Id == id select t;
            return taskInDB.First<Task>() as Task;
        }

        public void AddTask(Task newTask)
        {
            newTask.Category.Tasks.Add(newTask);

            if (newTask.IsComplete)
                this.AddPointsToCategoryWorker.RunWorkerAsync();

            taskDB.Tasks.InsertOnSubmit(newTask);

            if(!newTask.IsComplete)
                this.IncompleteTasks.Add(newTask);
            newTask.Category.UpdateDateModified();

            if (newTask.RecurringOption == Task.RecurringOptions.Never)
                newTask.Next = newTask.When;
            else
                newTask.Next = newTask.NextRecursionDate(newTask.When, newTask.RecurringAmount);

            taskDB.SubmitChanges();

            if (newTask.HasReminders)
                newTask.Schedule();
        }

        public void AddRecurringCopy(Task recurringTask)
        {
            Task newTask = new Task { 
                Name = recurringTask.Name, 
                IsComplete = true, 
                Reward = recurringTask.Reward, 
                Category = recurringTask.Category, 
                RecurringOption = Task.RecurringOptions.Never, 
                When = recurringTask.When, 
                Next = recurringTask.When,
                RecurringAmount = 0 ,
                Notes="", 
                HasReminders = false};

            this.ToBeChanged.Add(newTask);
        }

        public void CompleteHandler(Task t)
        {
            if (t.IsComplete)
            {
                t.Category.AddPoints(t.Reward);
                t.Unschedule();
                if(this.IncompleteTasks.Contains(t)) this.IncompleteTasks.Remove(t);
            }
            else
            {
                t.Category.RemovePoints(t.Reward);
                t.Schedule();
                if (!this.IncompleteTasks.Contains(t)) this.IncompleteTasks.Add(t);
            }
        }

        public void AddFromChangedTasks()
        {
            if (ToBeChanged.Count <= 0) return;
            List<Category> changedCategorys = new List<Category>();
            foreach (object obj in ToBeChanged)
            {
                Task t = obj as Task;
                if (!t.IsComplete)
                    this.IncompleteTasks.Add(t);
                taskDB.Tasks.InsertOnSubmit(t);
                t.Category.Tasks.Add(t);
                Debug.WriteLine(t.Name);
                changedCategorys.Add(t.Category);

            }
            taskDB.SubmitChanges();

            foreach (Category s in changedCategorys)
                s.Tasks.Load();
            changedCategorys.Clear();

            ToBeChanged = new ObservableCollection<object>();
        }

        public void RemoveTask(Task removingTask)
        {
            removingTask.Category.UpdateDateModified();

            if (removingTask.IsComplete)
                removingTask.Category.RemovePoints(removingTask.Reward);

            if (removingTask.HasReminders)
            {
                removingTask.Unschedule();
            }

            if (!removingTask.IsComplete) this.IncompleteTasks.Remove(removingTask);

            removingTask.Category.Tasks.Remove(removingTask);

            taskDB.Tasks.DeleteOnSubmit(removingTask);

            taskDB.SubmitChanges();
        }

        public void AddReward(Reward newReward)
        {
            newReward.Points = 0;
            newReward.Category.Rewards.Add(newReward);
            newReward.Category.UpdateDateModified();

            this.AllRewards.Add(newReward);

            taskDB.Rewards.InsertOnSubmit(newReward);

            taskDB.SubmitChanges();
        }

        public void RemoveReward(Reward oldReward)
        {
            oldReward.Category.Rewards.Remove(oldReward);
            oldReward.Category.UpdateDateModified();
            this.AllRewards.Remove(oldReward);

            taskDB.Rewards.DeleteOnSubmit(oldReward);

            taskDB.SubmitChanges();
        }

        public Category AddCategory(Category newCategory)
        {
            newCategory.DateModified = DateTime.Now;
            newCategory.NextLevelPoints = 70;
            newCategory.Level = 1;
            newCategory.Points = 0;

            if (this.All != null)
                this.CategoryList.Insert(1, newCategory);
            else
                this.CategoryList.Add(newCategory);

            taskDB.Categories.InsertOnSubmit(newCategory);

            taskDB.SubmitChanges();

            return newCategory;
        }

        public void RemoveCategory(Category removingCategory)
        {
            if (removingCategory.Tasks != null)
            {
                foreach (Task t in removingCategory.Tasks)
                {
                    if (t.IsComplete)
                        t.Category.RemovePoints(t.Reward);
                    if (t.HasReminders)
                        t.Unschedule();

                    if (!t.IsComplete)
                        IncompleteTasks.Remove(t);
                    taskDB.Tasks.DeleteOnSubmit(t);
                }
            }

            if (removingCategory.Rewards != null)
            {
                foreach (Reward r in removingCategory.Rewards)
                {
                    this.AllRewards.Remove(r);
                    taskDB.Rewards.DeleteOnSubmit(r);
                }
            }

            removingCategory.Parent.Children.Remove(removingCategory);


            foreach (Category child in removingCategory.Children)
            {
                child.Parent = removingCategory.Parent;
            }

            CategoryList.Remove(removingCategory);

            taskDB.Categories.DeleteOnSubmit(removingCategory);

            taskDB.SubmitChanges();

        }

        void AddPointsToCategoryWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (CategoryToAddTo != null && PointsToAdd > 0)
            {
                Debug.WriteLine("Adding points to " + CategoryToAddTo.Name);
                Category current = CategoryToAddTo;
                int currentPointsToAdd = PointsToAdd;
                Deployment.Current.Dispatcher.BeginInvoke(() => current.AddPoints(currentPointsToAdd));
            }
        }

        void AddPointsToCategoryWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Error != null)
                Debug.WriteLine("Error " + e.Error.Message);
            else
                taskDB.SubmitChanges();
            CategoryToAddTo = null;
            PointsToAdd = -1;

        }
    }
}
