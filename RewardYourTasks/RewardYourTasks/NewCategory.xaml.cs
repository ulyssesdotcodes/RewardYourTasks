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
using System.Text.RegularExpressions;
using System.Diagnostics;
using RewardYourTasks.ViewModels;

namespace RewardYourTasks
{
    public partial class NewCategoryPage : PhoneApplicationPage
    {
        int CategoryCount;
        Category updatingCategory;
        TaskViewModel viewModel;

        public NewCategoryPage()
        {
            InitializeComponent();

            this.viewModel = App.ViewModel;

            this.CategoryListPicker.DataContext = viewModel.CategoryList;

            CategoryCount = viewModel.CategoryList.Count;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


            String CategoryId;
            if (updatingCategory == null && NavigationContext.QueryString.TryGetValue("CategoryId", out CategoryId))
            {
                updatingCategory = this.viewModel.CategoryList.Single<Category>(s => s.Id == int.Parse(CategoryId));
                this.newName_TextBox.Text = updatingCategory.Name;
                this.CategoryListPicker.SelectedIndex = this.viewModel.CategoryList.IndexOf(updatingCategory.Parent);
            }
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void DoneButton_Click(object sender, System.EventArgs e)
        {
            Category NewParent = this.CategoryListPicker.SelectedItem as Category;
            if (this.newName_TextBox.Text == "")
            {
                MessageBox.Show("You can't have a category without a name!");
                return;
            }
            if (updatingCategory == null)
            {
                App.ViewModel.AddCategory(new Category
                {
                    Name = this.newName_TextBox.Text,
                    Parent = NewParent
                });
            }
            else if (NewParent.Id == updatingCategory.Id)
            {
                MessageBox.Show("You can't make a Category a parent of itself!");
                return;
            }
            else if (NewParent.Parent.Id == updatingCategory.Id)
            {
                MessageBox.Show("You can't make a Category a parent of it's parent!");
                return;
            } 
            else
            {
                if (NewParent != updatingCategory.Parent)
                {
                    int OldPoints = updatingCategory.Points;
                    updatingCategory.RemovePoints(OldPoints);
                    updatingCategory.Parent = NewParent;
                    updatingCategory.AddPoints(OldPoints);
                }
                updatingCategory.Name = this.newName_TextBox.Text;
                this.viewModel.SaveChangesToDB();
            }
            this.NavigationService.GoBack();
        }
    }
}