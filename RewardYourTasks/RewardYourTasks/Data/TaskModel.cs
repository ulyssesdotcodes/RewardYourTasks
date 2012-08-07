using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Phone.Scheduler;
using System.Threading;

namespace RewardYourTasks.Model
{
    public class TasksDataContext : DataContext
    {
        public TasksDataContext(string connectionString) : base(connectionString) { }

        public Table<Task> Tasks;

        public Table<Category> Categories;

        public Table<Reward> Rewards;
    }

    [Table]
    public class Category : INotifyPropertyChanged, INotifyPropertyChanging
    {
        [Column(IsVersion = true)]
        private Binary _version;

        private int _id;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    NotifyPropertyChanging("Id");
                    _id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }


        private DateTime _dateModified;
        [Column]
        public DateTime DateModified
        {
            get { return _dateModified; }
            set
            {
                NotifyPropertyChanging("DateModified");
                _dateModified = value;
                NotifyPropertyChanged("DateModified");
            }
        }

        private string _name;
        [Column]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    NotifyPropertyChanging("Name");
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private int _points;

        [Column]
        public int Points
        {
            get { return _points; }
            set
            {
                if (_points != value)
                {
                    NotifyPropertyChanging("Points");
                    _points = value;
                    NotifyPropertyChanged("Points");
                    UpdateLevel();
                }
            }
        }


        private int _level;
        [Column]
        public int Level
        {
            get { return _level; }
            set
            {
                if (_level != value)
                {
                    NotifyPropertyChanging("Level");
                    _level = value;
                    NotifyPropertyChanged("Level");
                }
            }
        }

        private int _nextLevelPoints;
        [Column]
        public int NextLevelPoints
        {
            get { return _nextLevelPoints; }
            set
            {
                if (_nextLevelPoints != value)
                {
                    NotifyPropertyChanging("NextLevelPoints");
                    _nextLevelPoints = value;
                    NotifyPropertyChanged("NextLevelPoints");
                }
            }
        }

        private void UpdateLevel()
        {
            Boolean showLevelUp = false;
            if (Points >= NextLevelPoints && NextLevelPoints != 0)
            {
                NotifyPropertyChanging("Level");
                Level++;
                NotifyPropertyChanged("Level");
                showLevelUp = true;
            }
            else if (Points < 5 * (Level - 1) * (Level + 12))
            {
                do
                {
                    Level--;
                } while (Points < 5 * (Level - 1) * (Level + 12));
            }
            else return;

            NotifyPropertyChanging("PointsToNextLevel");
            NextLevelPoints = 5 * Level * (Level + 13);
            NotifyPropertyChanged("PointsToNextLevel");

            if (showLevelUp)
                MessageBox.Show("You are now level " + Level + " in " + this.Name + ". Keep up the good work, a reward is on the way!");
        }
        
        [Column]
        internal int _parentId;

        private EntityRef<Category> _parent;

        [Association(Storage = "_parent", ThisKey = "_parentId", OtherKey = "Id", IsForeignKey=false)]
        public Category Parent
        {
            get { return _parent.Entity; }
            set
            {
                NotifyPropertyChanging("Parent");
                _parent.Entity = value;

                if (value != null)
                {
                    _parentId = value.Id;
                }

                NotifyPropertyChanged("Parent");
            }
        }

#region sets
        private EntitySet<Category> _children;

        [Association(Storage = "_children", OtherKey = "_parentId", ThisKey = "Id")]
        public EntitySet<Category> Children
        {
            get { return this._children; }
            set { this._children.Assign(value); }
        }

        private EntitySet<Task> _tasks;

        [Association(Storage = "_tasks", OtherKey = "_categoryId", ThisKey = "Id")]
        public EntitySet<Task> Tasks
        {
            get { return this._tasks; }
            set { this._tasks.Assign(value); }
        }

        private EntitySet<Reward> _rewards;

        [Association(Storage = "_rewards", OtherKey = "_categoryId", ThisKey = "Id")]
        public EntitySet<Reward> Rewards
        {
            get { return this._rewards; }
            set { this._rewards.Assign(value); }
        }

        public Category()
        {
            _tasks = new EntitySet<Task>(
                new Action<Task>(this.attach_Task),
                new Action<Task>(this.detatch_Task));

            _children = new EntitySet<Category>(
                new Action<Category>(this.attach_Child),
                new Action<Category>(this.detatch_Child));

            _rewards = new EntitySet<Reward>(
                new Action<Reward>(this.attach_Reward),
                new Action<Reward>(this.detatch_Reward));
        }


        private void attach_Task(Task task)
        {
            NotifyPropertyChanging("Task");
            task.Category = this;
            NotifyPropertyChanged("Task");
        }

        private void detatch_Task(Task task)
        {
            NotifyPropertyChanging("Task");
            task.Category = null;
            NotifyPropertyChanged("Task");
        }

        private void attach_Child(Category child)
        {
            NotifyPropertyChanging("Child");
            child.Parent = this;
            NotifyPropertyChanged("Child");
        }

        private void detatch_Child(Category child)
        {
            NotifyPropertyChanging("Child");
            child.Parent = null;
            NotifyPropertyChanged("Child");
        }

        private void attach_Reward(Reward reward)
        {
            NotifyPropertyChanging("Reward");
            reward.Category = this;
            NotifyPropertyChanged("Reward");
        }

        private void detatch_Reward(Reward reward)
        {
            NotifyPropertyChanging("Reward");
            reward.Category = null;
            NotifyPropertyChanged("Reward");
        }
#endregion

        public void UpdateDateModified()
        {
            this.DateModified = DateTime.Now;

            if (App.ViewModel.CategoryList.Contains(this)) App.ViewModel.CategoryList.Remove(this);

            if (App.ViewModel.All != null)
            {
                App.ViewModel.CategoryList.Insert(1, this);
                App.ViewModel.All.DateModified = DateTime.Now.AddTicks(1);
            }
            else
            {
                App.ViewModel.CategoryList.Insert(0, this);
            }
        }

        #region functions
        public void AddPoints(int points)
        {
            this.Points += points;
            foreach (Reward r in this.Rewards)
                r.AddPoints(points);
            if (this.Parent != null)
                this.Parent.AddPoints(points);
        }

        public void RemovePoints(int points)
        {
            if (this.Points >= points)
                this.Points -= points;
            else
                this.Points = 0;
            foreach (Reward r in this.Rewards)
                r.RemovePoints(points);
            if (this.Parent != null)
                this.Parent.RemovePoints(points);
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region INotifyPropertyChanging Members
        public event PropertyChangingEventHandler PropertyChanging;

        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }
        #endregion


    }

    [Table]
    public class Task : INotifyPropertyChanged, INotifyPropertyChanging
    {
        [Column(IsVersion = true)]
        private Binary _version;

        private int _id;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    NotifyPropertyChanging("Id");
                    _id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        private string _name;
        [Column]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    NotifyPropertyChanging("Name");
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }



        private string _notes;
        [Column]
        public string Notes
        {
            get { return _notes; }
            set
            {
                if (_notes != value)
                {
                    NotifyPropertyChanging("Notes");
                    _notes = value;
                    NotifyPropertyChanged("Notes");
                }
            }
        }

        [Column]
        internal int _categoryId;

        private EntityRef<Category> _category;

        [Association(Storage = "_category", ThisKey = "_categoryId", OtherKey = "Id", IsForeignKey = true)]
        public Category Category
        {
            get { return _category.Entity; }
            set
            {
                NotifyPropertyChanging("Category");
                _category.Entity = value;

                if (value != null)
                {
                    _categoryId = value.Id;
                }

                NotifyPropertyChanged("Category");
            }
        }

        private bool _isComplete;
        [Column]
        public bool IsComplete
        {
            get { return _isComplete; }
            set
            {
                if (_isComplete != value)
                {
                    NotifyPropertyChanging("IsComplete");
                    _isComplete = value;
                    NotifyPropertyChanged("IsComplete");
                }
            }
        }

        private bool _hasReminders;
        [Column]
        public bool HasReminders
        {
            get { return _hasReminders; }
            set
            {
                if (_hasReminders != value)
                {
                    NotifyPropertyChanging("HasReminders");
                    _hasReminders = value;
                    NotifyPropertyChanged("HasReminders");
                }
            }
        }

        private int _reward;
        [Column]
        public int Reward
        {
            get { return _reward; }
            set
            {
                if (_reward != value)
                {
                    NotifyPropertyChanging("Reward");
                    _reward = value;
                    NotifyPropertyChanged("Reward");
                }
            }
        }

        public enum RecurringOptions { Never, Days, Weeks, Months, DaysOfWeek }

        private RecurringOptions _recurringOption;
        [Column]
        public RecurringOptions RecurringOption
        {
            get { return _recurringOption; }
            set
            {
                NotifyPropertyChanging("RecurringOption");
                _recurringOption = value;
                NotifyPropertyChanged("RecurringOption");
            }
        }

        private int _recurringAmount;
        [Column]
        public int RecurringAmount
        {
            get { return _recurringAmount; }
            set
            {
                NotifyPropertyChanging("RecurringAmount");
                _recurringAmount = value;
                NotifyPropertyChanged("RecurringAmount");
            }
        }

        private DateTime _when;
        [Column]
        public DateTime When
        {
            get { return _when; }
            set
            {
                NotifyPropertyChanging("When");
                _when = value;
                NotifyPropertyChanged("When");
            }
        }

        private DateTime _next;
        [Column]
        public DateTime Next
        {
            get { return _next; }
            set
            {
                NotifyPropertyChanging("Next");
                _next = value;
                NotifyPropertyChanged("Next");
            }
        }

        public void UpdateTask()
        {
            Debug.WriteLine(this.When.Ticks < DateTime.Now.Ticks);
            if (this.RecurringOption == RecurringOptions.Never || this.RecurringAmount <= 0 || this.Next >= DateTime.Now && !(this.IsComplete == true && this.Next.Date <= DateTime.Now.Date)) return;

            Debug.WriteLine("Finding new date for " + this.Name);

            if (this.IsComplete)
                App.ViewModel.AddRecurringCopy(this);

            while (this.Next <= DateTime.Now || (this.IsComplete && this.Next.Date <= DateTime.Now.Date))
            {
                this.When = this.Next;
                this.Next = NextRecursionDate(this.Next, this.RecurringAmount);
            }

            if (this.IsComplete)
            {
                this.IsComplete = false;
                if (this.HasReminders)
                    this.Schedule();
            }

            App.ViewModel.SaveChangesToDB();
        }

        public DateTime NextRecursionDate(DateTime from, int amount)
        {
            switch (this.RecurringOption)
            {
                case RecurringOptions.Days:
                    return from.AddDays(amount);
                case RecurringOptions.Weeks:
                    return from.AddDays(amount * 7);
                case RecurringOptions.Months:
                    return from.AddMonths(amount);
                case RecurringOptions.DaysOfWeek:
                    int i = 0;
                    do
                    {
                        from = new DateTime(from.Ticks + TimeSpan.TicksPerDay);
                        i++;
                    } while ((amount & (int)Math.Pow(2, (int)from.DayOfWeek - 1)) == 0 && i < 8);
                    return from;
                default: return DateTime.Now;
            }
        }

        public void Schedule()
        {
            if (this.HasReminders && this.When > DateTime.Now && ScheduledActionService.Find(this.Id.ToString() + this.When.Ticks) == null)
            {
                Reminder r = new Reminder(this.Id.ToString() + this.When.Ticks);
                r.BeginTime = this.When;
                r.Title = this.Name.Length > 63 ? this.Name.Substring(0,60) + "..." : this.Name;
                r.Content = this.Notes;
                r.NavigationUri = new Uri("/ViewTask.xaml?taskId=" + this.Id, UriKind.Relative);
                ScheduledActionService.Add(r);
            }
        }

        public void Unschedule()
        {
            if (this.HasReminders && ScheduledActionService.Find(this.Id.ToString() + this.When.Ticks) != null)
            {
                ScheduledActionService.Remove(this.Id.ToString() + this.When.Ticks);
            }
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region INotifyPropertyChanging Members
        public event PropertyChangingEventHandler PropertyChanging;

        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }
        #endregion
    }



    [Table]
    public class Reward : INotifyPropertyChanged, INotifyPropertyChanging
    {
        [Column(IsVersion = true)]
        private Binary _version;

        private int _id;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    NotifyPropertyChanging("Id");
                    _id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        private string _name;
        [Column]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    NotifyPropertyChanging("Name");
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private int _points;
        [Column]
        public int Points
        {
            get { return _points; }
            set
            {
                if (_points != value)
                {
                    NotifyPropertyChanging("Points");
                    _points = value;
                    NotifyPropertyChanged("Points");
                }
            }
        }

        private int _pointsPerReward;
        [Column]
        public int PointsPerReward
        {
            get { return _pointsPerReward; }
            set
            {
                if (_pointsPerReward != value)
                {
                    NotifyPropertyChanging("PointsPerReward");
                    _pointsPerReward = value;
                    NotifyPropertyChanged("PointsPerReward");
                }
            }
        }

        private int _count;

        [Column]
        public int Count
        {
            get { return _count; }
            set
            {
                if (_count != value)
                {
                    NotifyPropertyChanging("Count");
                    _count = value;
                    NotifyPropertyChanged("Count");
                }
            }
        }

        [Column]
        internal int _categoryId;

        private EntityRef<Category> _category;

        [Association(Storage = "_category", ThisKey = "_categoryId", OtherKey = "Id", IsForeignKey = true)]
        public Category Category
        {
            get { return _category.Entity; }
            set
            {
                NotifyPropertyChanging("Category");
                _category.Entity = value;

                if (value != null)
                {
                    _categoryId = value.Id;
                }

                NotifyPropertyChanged("Category");
            }
        }


        public bool AddPoints(int points)
        {
            this.Points += points;
            if (this.Points >= this.PointsPerReward)
            {
                this.Count += this.Points / this.PointsPerReward;
                this.Points = this.Points % this.PointsPerReward;
                return true;
            }
            return false;
        }

        public void RemovePoints(int points)
        {
            this.Points -= points;
            if (this.Points < 0)
            {
                int change = 1 - this.Points / this.PointsPerReward;
                this.Count -= change;
                this.AddPoints(change * this.PointsPerReward);
            }
            if (this.Count < 0)
            {
                this.Count = 0;
                this.Points = 0;
            }
        }

        public bool UseReward()
        {
            if (this.Count > 0)
            {
                this.Count--;
                App.ViewModel.SaveChangesToDB();
                return true;
            }
            return false;
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region INotifyPropertyChanging Members
        public event PropertyChangingEventHandler PropertyChanging;

        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }
        #endregion
    }
}