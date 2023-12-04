using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VindegadeKS_WPF
{
    /// <summary>
    /// Interaction logic for LessonPage.xaml
    /// </summary>
    public partial class LessonPage : Page
    {
        public LessonPage()
        {
            InitializeComponent();

            //Call the addItems method which create the ListBoxItems for your ListBox
            addItems();
        }


        //Input
        private void Les_Name_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Les_Type_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Les_Description_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        //Buttons
        private void Les_Add_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Les_Save_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Les_Edit_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Les_Delete_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        #region ListBox
        //What happens when you select an item in the ListBox
        private void Les_DisLes_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Safety check, to make sure that the selected item exists
            if(Les_DisLes_ListBox.SelectedItem != null)
            {
                //Everything inside of the if-statement will likely have to be personalised 

                //Changes the text from the display window 
                //After the equal sign; (#ListBoxName.SelectedItem as #itemClass).#attribute;
                //The parts after a #, are the parts that needs to change based on your page
                Les_DisName_TextBlock.Text = (Les_DisLes_ListBox.SelectedItem as LesListBoxItems).Name;
                Les_DisType_TextBlock.Text = (Les_DisLes_ListBox.SelectedItem as LesListBoxItems).Type;
                Les_DisDescription_TextBlock.Text = (Les_DisLes_ListBox.SelectedItem as LesListBoxItems).Description;

            }
        }

        //Method to create, control and add items to the ListBox
        private void addItems()
        {
            //Make a list with the Item Class from below called items (Name doesn't matter)
            //LesListBoxItems in my case
            List<LesListBoxItems> items = new List<LesListBoxItems>();

            //Add new items from the item class with specific attributes to the list
            //Will later be remade to automatically add items based on the database
            items.Add(new LesListBoxItems() { Name = "A", Type = "A", Description = "A" });
            items.Add(new LesListBoxItems() { Name = "B", Type = "B", Description = "B" });
            items.Add(new LesListBoxItems() { Name = "C", Type = "C", Description = "C" });

            //Set the ItemsSource to the list, so that the ListBox uses the list to make the ListBoxItems
            Les_DisLes_ListBox.ItemsSource = items; 
        }

        //Class to define the content of the ListBoxItems for the ListBox
        public class LesListBoxItems
        {
            //The attributes of the items for the ListBox
            public string Name { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }
        }
        #endregion
    }
}
