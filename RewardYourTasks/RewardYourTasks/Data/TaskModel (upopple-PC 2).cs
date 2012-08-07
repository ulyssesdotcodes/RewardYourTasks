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

namespace RewardYourTasks.Model
{
    public class TasksDataContext : DataContext
    {
        public TasksDataContext(string connectionString) : base(connectionString) { }

        public Table<Task> Tasks;

        public Table<Skill> Skills;

        public Table<Reward> Rewards;
    }

    [Table]
    public class Skill : INotifyPropertyChanged, INotifyPropertyChanging
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

        private int _achievementLevel;
        [Column]
        public int AchievementLevel
        {
            get { return _achievementLevel; }
            set
            {
                if (_achievementLevel != value)
                {
                    NotifyPropertyChanging("AchievementLevel");
                    _achievementLevel = value;
                    NotifyPropertyChanged("AchievementLevel");
                }
            }
        }

        private int _currentPoints;
        [Column]
        public int CurrentPoints
        {
            get { return _currentPoints; }
            set
            {
                if (_currentPoints != value)
                {
                    NotifyPropertyChanging("CurrentPoints");
                    _currentPoints = value;
                    NotifyPropertyChanged("CurrentPoints");
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

        private int _parentId;

        [Column(CanBeNull = true)]
        public int ParentId
        {
            get { return _parentId; }
            set
            {
                if (_parentId != value)
                {
                    NotifyPropertyChanging("ParentId");
                    _parentId = value;
                    NotifyPropertyChanged("ParentId");
                }
            }
        }

        private String _parentName;

        [Column(CanBeNull = true)]
        public String ParentName
        {
            get { return _parentName; }
            set
            {
                if (_parentName != value)
                {
                    NotifyPropertyChanging("ParentName");
                    _parentName = value;
                    NotifyPropertyChanged("ParentName");
                }
            }
        }

        private int _taskCount;
        [Column]
        public int TaskCount
        {
            get { return _taskCount; }
            set
            {
                if (_taskCount != value)
                {
                    NotifyPropertyChanging("TaskCount");
                    _taskCount = value;
                    NotifyPropertyChanged("TaskCount");
                }
            }
        }

        private EntitySet<Task> _tasks;

        [Association(Storage = "_tasks", OtherKey = "_skillId", ThisKey = "Id")]
        public EntitySet<Task> Tasks
        {
            get { return this._tasks; }
            set { this._tasks.Assign(value); }
        }

        private EntitySet<Reward> _rewards;

        [Association(Storage = "_rewards", OtherKey = "_skillId", ThisKey = "Id")]
        public EntitySet<Reward> Rewards
        {
            get { return this._rewards; }
            set { this._rewards.Assign(value); }
        }

        public Skill()
        {
            _tasks = new EntitySet<Task>(new Action<Task>(this.attach_Task), new Action<Task>(this.detach_Task));
            _rewards = new EntitySet<Reward>(new Action<Reward>(this.attach_Reward), new Action<Reward>(this.detach_Reward));
        }

        #region Attaches and Detatches
        private void attach_Task(Task item)
        {
            NotifyPropertyChanging("Task");
            item.Skill = this;
        }


        private void detach_Task(Task item)
        {
            NotifyPropertyChanging("Task");
            item.Skill = null;
        }

        private void attach_Reward(Reward item)
        {
            NotifyPropertyChanging("Reward");
            item.Skill = this;
        }


        private void detach_Reward(Reward item)
        {
            NotifyPropertyChanging("Reward");
            item.Skill = null;
        }
        #endregion

        #region Point logic
        public void AddPoints(int points)
        {
            CurrentPoints += points;       
            while (CurrentPoints >= NextLevelPoints)
            {
                LevelUp();
            }
            AddPointsToRewards(points);
            UpdateLevelPercentComplete();
        }

        public void RemovePoints(int points)
        {
            CurrentPoints -= points;
            while (CurrentPoints < 0)
            {
                LevelDown();
            }
            RemovePointsFromRewards(points);
            UpdateLevelPercentComplete();
        }

        private int _levelPercentComplete;

        public int LevelPercentComplete
        {
            get { return this._levelPercentComplete; }
            set {
                NotifyPropertyChanging("LevelPercentComplete");
                _levelPercentComplete = value;
                NotifyPropertyChanged("LevelPercentComplete");
            }
        }

        public void UpdateLevelPercentComplete()
        {
            LevelPercentComplete = (int)(((double)CurrentPoints / (double)NextLevelPoints) * 100);
        }

        public void LevelUp()
        {
            CurrentPoints -= NextLevelPoints;
            AchievementLevel++;
            NextLevelPoints = 50 + AchievementLevel * 10;
        }

        public void LevelDown()
        {
            AchievementLevel--;
            NextLevelPoints = 50 + AchievementLevel * 10;
            CurrentPoints += NextLevelPoints;
        }
        #endregion

        #region Reward Logic
        public void AddPointsToRewards(int points){
            foreach (Reward r in Rewards)
            {
                r.AddPoints(points);
            }
        }

        public void RemovePointsFromRewards(int points)
        {
            foreach (Reward r in Rewards)
            {
                r.RemovePoints(points);
            }
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

        [Column(IsPrimaryKey=true, IsDbGenerated=true, DbType="INT NOT NULL Identity", CanBeNull=false, AutoSync=AutoSync.OnInsert)]
        public int Id
        {
            get{ return _id; }
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
            get{ return _name; }
            set 
            {
                if(_name != value)
                {
                    NotifyPropertyChanging("Name");
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private bool _isComplete;
        [Column]
        public bool IsComplete
        {
            get{ return _isComplete; }
            set 
            {
                if(_isComplete != value)
                {
                    NotifyPropertyChanging("IsComplete");
                    _isComplete = value;
                    NotifyPropertyChanged("IsComplete");
                }
            }
        }
        
        private int _reward;
        [Column]
        public int Reward
        {
            get{ return _reward; }
            set 
            {
                if(_reward != value)
                {
                    NotifyPropertyChanging("Reward");
                    _reward = value;
                    NotifyPropertyChanged("Reward");
                }
            }
        }

        private bool _isGoal;
        [Column]
        public bool IsGoal
        {
            get { return _isGoal; }
            set
            {
                if (_isGoal != value)
                {
                    NotifyPropertyChanging("IsGoal");
                    _isGoal = value;
                    NotifyPropertyChanged("IsGoal");
                }
            }
        }

        [Column]
        internal int _skillId;

        private EntityRef<Skill> _skill;

        [Association(Storage="_skill", ThisKey="_skillId", OtherKey="Id", IsForeignKey= true)]
        public Skill Skill
        {
            get{ return _skill.Entity; }
            set 
            {
                NotifyPropertyChanging("Skill");
                _skill.Entity = value;

                if (value != null)
                {
                    _skillId = value.Id;
                }

                NotifyPropertyChanged("Skill");
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
        internal int _skillId;

        private EntityRef<Skill> _skill;

        [Association(Storage = "_skill", ThisKey = "_skillId", OtherKey = "Id", IsForeignKey = true)]
        public Skill Skill
        {
            get { return _skill.Entity; }
            set
            {
                NotifyPropertyChanging("Skill");
                _skill.Entity = value;

                if (value != null)
                {
                    _skillId = value.Id;
                }

                NotifyPropertyChanged("Skill");
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
