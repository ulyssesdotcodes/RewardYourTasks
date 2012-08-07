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
using System.IO.IsolatedStorage;

namespace RewardYourTasks
{
    public class Setting<T>
    {
        string name;
        private T value;
        private T defaultValue;
        private bool hasValue;

        public Setting(string name, T defaultValue)
        {
            this.name = name;
            this.defaultValue = defaultValue;
        }

        public T Value
        {
            get
            {
                if (!this.hasValue)
                {
                    if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue(this.name, out this.value))
                    {
                        this.value = this.defaultValue;
                        IsolatedStorageSettings.ApplicationSettings[this.name] = this.value;
                    }
                }
                return this.value;
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings[this.name] = value;
                this.value = value;
                this.hasValue = true;
            }
        }

        public T DefaultValue
        {
            get{ return this.defaultValue; }
        }

        public void ForceRefresh()
        {
            this.hasValue = false;
        }
    }
}
