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
    /// Interaction logic for AppointmentPage.xaml
    /// </summary>
    public partial class AppointmentPage : Page
    {
        public AppointmentPage()
        {
            InitializeComponent();
            ComboBoxFunction();
            ListBoxFunction();
        }

        //What happens when you select an item in the ListBox
        private void Apmt_DisApmt_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Safety check, to make sure that the selected item exists
            if (Apmt_DisApmt_ListBox.SelectedItem != null)
            {
                //Everything inside of the if-statement will likely have to be personalised 

                //Changes the text from the display window 
                //After the equal sign; (#ListBoxName.SelectedItem as #itemClass).#attribute;
                //The parts after a #, are the parts that needs to change based on your page
                Apmt_DisLesson_TextBlock.Text = "Lektion: " + (Apmt_DisApmt_ListBox.SelectedItem as ApmtListBoxItems).Lesson;
                Apmt_DisLessonType_TextBlock.Text = "Lektionstype: " + (Apmt_DisApmt_ListBox.SelectedItem as ApmtListBoxItems).LessonType;
                Apmt_DisClass_TextBlock.Text = "Hold: " + (Apmt_DisApmt_ListBox.SelectedItem as ApmtListBoxItems).Class;
                Apmt_DisStudent_TextBlock.Text = "Elev: " + (Apmt_DisApmt_ListBox.SelectedItem as ApmtListBoxItems).Student;
                Apmt_DisInstructor_TextBlock.Text = "Underviser: " + (Apmt_DisApmt_ListBox.SelectedItem as ApmtListBoxItems).Instructor;
                Apmt_DisDateTime_TextBlock.Text = "Underviser: " + (Apmt_DisApmt_ListBox.SelectedItem as ApmtListBoxItems).DateTime;

            }
        }

        //Method to create, control and add items to the ListBox
        private void ListBoxFunction()
        {
            //Make a list with the Item Class from below called items (Name doesn't matter)
            //LesListBoxItems in my case
            List<ApmtListBoxItems> items = new List<ApmtListBoxItems>();

            //Add new items from the item class with specific attributes to the list
            //Will later be remade to automatically add items based on the database
            items.Add(new ApmtListBoxItems() { Lesson = "A", LessonType = "A", Class = "A", Student = "A", Instructor = "A", DateTime = default });
            items.Add(new ApmtListBoxItems() { Lesson = "B", LessonType = "B", Class = "B", Student = "B", Instructor = "B", DateTime = default });
            items.Add(new ApmtListBoxItems() { Lesson = "C", LessonType = "C", Class = "C", Student = "C", Instructor = "C", DateTime = default });
            items.Add(new ApmtListBoxItems() { Lesson = "D", LessonType = "D", Class = "D", Student = "D", Instructor = "D", DateTime = default });
            items.Add(new ApmtListBoxItems() { Lesson = "E", LessonType = "E", Class = "E", Student = "E", Instructor = "E", DateTime = default });
            items.Add(new ApmtListBoxItems() { Lesson = "F", LessonType = "F", Class = "F", Student = "F", Instructor = "F", DateTime = default });

            //Only necessary for multi-attribute ListBoxItem
            //Set up the attribute 'SetUp' which is used to determine the appearance of the ListBoxItem 
            //Forloop to go through all items in the items-list, to add and fill the 'SetUp' attribute
            for (int i = 0; items.Count > i; i++)
            {
                items[i].SetUp = $"{items[i].Lesson} - {items[i].LessonType}\n{items[i].DateTime}";
            }

            //Set the ItemsSource to the list, so that the ListBox uses the list to make the ListBoxItems
            Apmt_DisApmt_ListBox.ItemsSource = items;
        }

        //Class to define the content of the ListBoxItems for the ListBox
        public class ApmtListBoxItems
        {
            //The attributes of the items for the ListBox
            public string Lesson { get; set; }
            public string LessonType { get; set; }
            public string Class { get; set; }
            public string Student { get; set; }
            public string Instructor { get; set; }
            public DateTime DateTime { get; set; }
            //Extra attribute, used for visuals (Only needed for multi-attribute views)
            public string SetUp { get; set; }
        }

        private void ComboBoxFunction()
        {
            //New list and datapoints for Combobox
            List<ApmtComboBoxLesson> lessons = new List<ApmtComboBoxLesson>();
            lessons.Add(new ApmtComboBoxLesson { Id = 1, DisplayValue = "One" });
            lessons.Add(new ApmtComboBoxLesson { Id = 2, DisplayValue = "Two" });
            lessons.Add(new ApmtComboBoxLesson { Id = 3, DisplayValue = "Three" });
            //Set the ItemsSource
            Apmt_PickLesson_ComboBox.ItemsSource = lessons;
            //Sets which attribute is displayed
            Apmt_PickLesson_ComboBox.DisplayMemberPath = "DisplayValue";

            List<ApmtComboBoxClass> classes = new List<ApmtComboBoxClass>();
            classes.Add(new ApmtComboBoxClass { Id = 1, DisplayValue = "One" });
            classes.Add(new ApmtComboBoxClass { Id = 2, DisplayValue = "Two" });
            classes.Add(new ApmtComboBoxClass { Id = 3, DisplayValue = "Three" });
            Apmt_PickClass_ComboBox.ItemsSource = classes;
            Apmt_PickClass_ComboBox.DisplayMemberPath = "DisplayValue";

            List<ApmtComboBoxStudent> students = new List<ApmtComboBoxStudent>();
            students.Add(new ApmtComboBoxStudent { Id = 1, DisplayValue = "One" });
            students.Add(new ApmtComboBoxStudent { Id = 2, DisplayValue = "Two" });
            students.Add(new ApmtComboBoxStudent { Id = 3, DisplayValue = "Three" });
            Apmt_PickStudent_ComboBox.ItemsSource = students;
            Apmt_PickStudent_ComboBox.DisplayMemberPath = "DisplayValue";

            List<ApmtComboBoxInstructor> instructors = new List<ApmtComboBoxInstructor>();
            instructors.Add(new ApmtComboBoxInstructor { Id = 1, DisplayValue = "One" });
            instructors.Add(new ApmtComboBoxInstructor { Id = 2, DisplayValue = "Two" });
            instructors.Add(new ApmtComboBoxInstructor { Id = 3, DisplayValue = "Three" });
            Apmt_PickInstructor_ComboBox.ItemsSource = instructors;
            Apmt_PickInstructor_ComboBox.DisplayMemberPath = "DisplayValue";
        }

        //Class which defines the ComboBox Data
        public class ApmtComboBoxLesson
        {
            public int Id { get; set; }
            public string DisplayValue { get; set; }
        }
        public class ApmtComboBoxClass
        {
            public int Id { get; set; }
            public string DisplayValue { get; set; }
        }
        public class ApmtComboBoxStudent
        {
            public int Id { get; set; }
            public string DisplayValue { get; set; }
        }
        public class ApmtComboBoxInstructor
        {
            public int Id { get; set; }
            public string DisplayValue { get; set; }
        }

        private void Apmt_ShowLessonType_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Apmt_PickLesson_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Apmt_PickClass_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Apmt_PickStudent_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Apmt_PickInstructor_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Apmt_Add_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Apmt_Save_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Apmt_Edit_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Apmt_Delete_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
