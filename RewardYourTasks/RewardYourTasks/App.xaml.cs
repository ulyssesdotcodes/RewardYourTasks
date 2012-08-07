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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using RewardYourTasks.ViewModels;
using RewardYourTasks.Model;
using System.Diagnostics;
using Microsoft.Phone.Data.Linq;
using System.Windows.Data;
using System.Globalization;
using System.ComponentModel;
using Microsoft.Phone.Tasks;

namespace RewardYourTasks
{
    public partial class App : Application
    {
        private static TaskViewModel viewModel = null;

        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static TaskViewModel ViewModel
        {
            get { return viewModel; }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        public static bool IsTrial { get; private set; }
        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {

                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            

            string DBConnectionString = "Data Source=isostore:/Tasks_v3.sdf";
            bool createDB = false;

            using (TasksDataContext db = new TasksDataContext(DBConnectionString))
            {


                if (db.DatabaseExists() == false)
                {
                    db.CreateDatabase();

                    createDB = true;

                    db.SubmitChanges();
                }
                else
                {
                    //Create the database schema updater
                    DatabaseSchemaUpdater dbUpdate = db.CreateDatabaseSchemaUpdater();

                    //Get database version
                    int dbVersion = dbUpdate.DatabaseSchemaVersion;

                    Debug.WriteLine("Database version " + dbVersion);

                    if (dbVersion == 2)
                    {
                        dbUpdate.DatabaseSchemaVersion = 3;
                        dbUpdate.Execute();
                    }
                }
            }

            App.viewModel = new TaskViewModel(DBConnectionString);

            if (createDB)
            {
                //TODO: Update this with the new database
                //Add default Categorys
                viewModel.All = viewModel.AddCategory(new Category { Name = "All", Parent=null });
                Category health = viewModel.AddCategory(new Category { Name = "Health", Parent = ViewModel.All});
                Category fitness = viewModel.AddCategory(new Category { Name = "Fitness", Parent = health});
                Category organization = viewModel.AddCategory(new Category { Name = "Organization", Parent = ViewModel.All});

                //Add default tasks
                viewModel.AddTask(new Task
                {
                    Name = "Eat 2 servings of fruit",
                    Notes = "",
                    Reward = 5,
                    Category = health,
                    RecurringOption = Task.RecurringOptions.Days,
                    RecurringAmount = 1,
                    When = DateTime.Now.Date.AddSeconds(5),
                    IsComplete = false,
                    HasReminders = false,
                });

                viewModel.AddTask(new Task { 
                    Name = "Take vitamins", 
                    Notes = "",
                    Reward = 5, 
                    Category = health, 
                    RecurringOption = Task.RecurringOptions.Days, 
                    RecurringAmount = 1,
                    When = DateTime.Now.Date.AddSeconds(5), 
                    IsComplete = false,
                    HasReminders = false, });

                viewModel.AddTask(new Task
                {
                    Name = "Cardio",
                    Notes = "",
                    Reward = 5,
                    Category = fitness,
                    RecurringOption = Task.RecurringOptions.Days,
                    RecurringAmount = 1,
                    When = DateTime.Now.Date.AddSeconds(5),
                    IsComplete = false,
                    HasReminders = false,
                });

                viewModel.AddTask(new Task
                {
                    Name = "Weights",
                    Notes = "",
                    Reward = 5,
                    Category = fitness,
                    RecurringOption = Task.RecurringOptions.DaysOfWeek,
                    RecurringAmount = 17,
                    When = DateTime.Now.Date.AddSeconds(5),
                    IsComplete = false,
                    HasReminders = false,
                });

                viewModel.AddTask(new Task
                {
                    Name = "Sport",
                    Notes = "",
                    Reward = 5,
                    Category = fitness,
                    RecurringOption = Task.RecurringOptions.DaysOfWeek,
                    RecurringAmount = 4,
                    When = DateTime.Now.Date.AddSeconds(5),
                    IsComplete = false,
                    HasReminders = false,
                });


                viewModel.AddTask(new Task
                {
                    Name = "Add something to Reward Your Tasks",
                    Notes = "",
                    Reward = 5,
                    Category = organization,
                    RecurringOption = Task.RecurringOptions.Days,
                    RecurringAmount = 1,
                    When = DateTime.Now.Date.AddHours(18),
                    IsComplete = false,
                    HasReminders = true,
                });

                viewModel.AddTask(new Task
                {
                    Name = "Explore Reward Your Tasks",
                    Notes = "",
                    Reward = 15,
                    Category = organization,
                    RecurringOption = Task.RecurringOptions.Never,
                    RecurringAmount = 0,
                    When = DateTime.Now.Date.AddSeconds(5),
                    IsComplete = false,
                    HasReminders = false,
                });

                //Add default rewards
                viewModel.AddReward(new Reward { Name = "Satisfy that sweet tooth", Category = health, PointsPerReward = 40 });
            }

            viewModel.LoadCollectionsWorker.RunWorkerAsync();

            /*
            foreach (Task t in viewModel.AllTasks)
            {
                t.UpdateTask();
            }

            viewModel.AddFromChangedTasks();
            */

        }

        private void DetermineIsTrial()
        {
            //Determine trial state
#if TRIAL
            IsTrial = true;
#else
            var license = new Microsoft.Phone.Marketplace.LicenseInformation();
            IsTrial = license.IsTrial();
#endif
        }

        public static bool CheckForTrialMessage()
        {
            if (App.IsTrial && viewModel.IncompleteTasks.Count >= 10)
            {
                MessageBoxResult mbr = MessageBox.Show("Whoops! You've reached the maximum number (10) of incomplete tasks which you can create on the trial version.\n\nBuy the full version now?", "Trial Version", MessageBoxButton.OKCancel);
                if (mbr == MessageBoxResult.OK)
                {
                    MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
                    marketplaceDetailTask.Show();
                }
                return false;
            }
            return true;
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            DetermineIsTrial();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            DetermineIsTrial();
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}