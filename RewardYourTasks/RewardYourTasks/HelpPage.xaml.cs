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
using Microsoft.Phone.Tasks;

namespace RewardYourTasks
{
    public partial class HelpPage : PhoneApplicationPage
    {
        public HelpPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            String startItem;
            if (NavigationContext.QueryString.TryGetValue("startItem", out startItem))
            {
                this.HelpAboutPivot_Control.SelectedIndex = int.Parse(startItem);
            }
        }

        private void Email_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask ect = new EmailComposeTask();
            ect.To = "ulysses.popple+rewardyourtasks@gmail.com";
            ect.Subject = "Reward Your Tasks";
            ect.Body = "Hey Ulysses,\n\n\n\nThanks,\n";
            ect.Show();
        }

        private void LaunchReview(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask mrt = new MarketplaceReviewTask();
            mrt.Show();
        }
    }
}