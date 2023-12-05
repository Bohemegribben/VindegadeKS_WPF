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

            //Call the ListBoxFunction method which create the ListBoxItems for your ListBox
            ListBoxFunction();

            ComboBoxFunction();
        }


        //Input
        //Idea: Instead of when changed, in the save button get the values from the boxes
        //      Making these method unnecessary
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
            LockInputFields();
        }

        private void Les_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            Les_DisName_TextBlock.Text = "Modul Navn: " + Les_Name_TextBox.Text;
            Les_DisType_TextBlock.Text = "Kørekorts Type: " + Les_Type_ComboBox.Text;
            Les_DisDescription_TextBlock.Text = "Beskrivelse: " + Les_Description_TextBox.Text;
        }

        private void Les_Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            UnlockInputFields();
        }

        private void Les_Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            ClearInputFields();

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
                Les_DisName_TextBlock.Text = "Modul Navn: " + (Les_DisLes_ListBox.SelectedItem as LesListBoxItems).Name;
                Les_DisType_TextBlock.Text = "Kørekorts Type: " + (Les_DisLes_ListBox.SelectedItem as LesListBoxItems).Type;
                Les_DisDescription_TextBlock.Text = "Beskrivelse: " + (Les_DisLes_ListBox.SelectedItem as LesListBoxItems).Description;

            }
        }

        //Method to create, control and add items to the ListBox
        private void ListBoxFunction()
        {
            //Make a list with the Item Class from below called items (Name doesn't matter)
            //LesListBoxItems in my case
            List<LesListBoxItems> items = new List<LesListBoxItems>();

            //Add new items from the item class with specific attributes to the list
            //Will later be remade to automatically add items based on the database
            items.Add(new LesListBoxItems() { Name = "A", Type = "A", Description = "A" });
            items.Add(new LesListBoxItems() { Name = "B", Type = "B", Description = "B" });
            items.Add(new LesListBoxItems() { Name = "C", Type = "C", Description = "C" });
            items.Add(new LesListBoxItems() { Name = "D", Type = "D", Description = "D" });
            items.Add(new LesListBoxItems() { Name = "E", Type = "E", Description = "E" });
            items.Add(new LesListBoxItems() { Name = "F", Type = "F", Description = "F" });

            //Only necessary for multi-attribute ListBoxItem
            //Set up the attribute 'SetUp' which is used to determine the appearance of the ListBoxItem 
            //Forloop to go through all items in the items-list, to add and fill the 'SetUp' attribute
            for (int i = 0;items.Count> i;i++)
            {
                items[i].SetUp = $"{items[i].Name}\n{items[i].Type}\n{items[i].Description}";
            }
            
            //Set the ItemsSource to the list, so that the ListBox uses the list to make the ListBoxItems
            Les_DisLes_ListBox.ItemsSource = items; 
        }

        //Class to define the content of the ListBoxItems for the ListBox
        public class LesListBoxItems
        {
            //The attributes of the items for the ListBox
            public string Name { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
            //Extra attribute, used for visuals (Only needed for multi-attribute views)
            public string SetUp { get; set; }
        }
        #endregion

        #region ComboBox
        private void ComboBoxFunction()
        {
            //New list and datapoints for Combobox
            List<LesComboBoxType> types = new List<LesComboBoxType>();
            types.Add(new LesComboBoxType { Id = 1, DisplayValue = "One" });
            types.Add(new LesComboBoxType { Id = 2, DisplayValue = "Two" });
            types.Add(new LesComboBoxType { Id = 3, DisplayValue = "Three" });

            //Set the ItemsSource
            Les_Type_ComboBox.ItemsSource = types;
            //Sets which attribute is displayed
            Les_Type_ComboBox.DisplayMemberPath = "DisplayValue";
        }

        //Class which defines the ComboBox Data
        public class LesComboBoxType
        {
            public int Id { get; set; }
            public string DisplayValue { get; set; }
        }
        #endregion

        #region Quality of Life Methods
        //Locks all inputfields, so that they can't be edited
        private void LockInputFields()
        {
            Les_Name_TextBox.IsEnabled = false;
            Les_Type_ComboBox.IsEnabled = false;
            Les_Description_TextBox.IsEnabled = false;
        }

        //Unlocks all inputfields, so that they can be edited
        private void UnlockInputFields()
        {
            Les_Name_TextBox.IsEnabled = true;
            Les_Type_ComboBox.IsEnabled = true;
            Les_Description_TextBox.IsEnabled = true;
        }

        //Clears all inputfields
        private void ClearInputFields()
        {
            Les_Name_TextBox.Clear();
            Les_Type_ComboBox.SelectedItem = null;
            Les_Description_TextBox.Clear();
        }
        #endregion
    }
}
