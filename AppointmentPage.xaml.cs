using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static VindegadeKS_WPF.Class_Sub_ShowClass_Page;
using VindegadeKS_WPF;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Microsoft.Identity.Client;

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
        public Appointment CurrentAppointment = new Appointment();
        Appointment appointmentToBeRetrieved;
        public Instructor CurrentInstructor = new Instructor();
        Instructor instructorToBeRetrieved;
        public Student CurrentStudent = new Student();
        Student studentToBeRetrieved;
        public Class CurrentClass = new Class();
        Class classToBeRetrieved;
        public Lesson CurrentLesson = new Lesson();
        Lesson lessonToBeRetrieved;
        int currentItem;
        bool edit = false;

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
                Apmt_DisLesson_TextBlock.Text = "Lektion: " + (Apmt_DisApmt_ListBox.SelectedItem as Lesson).LesName;
                Apmt_DisLessonType_TextBlock.Text = "Lektionstype: " + (Apmt_DisApmt_ListBox.SelectedItem as Lesson).LesType;
                Apmt_DisClass_TextBlock.Text = "Hold: " + (Apmt_DisApmt_ListBox.SelectedItem as Class).ClassName;
                Apmt_DisStudent_TextBlock.Text = "Elev: " + (Apmt_DisApmt_ListBox.SelectedItem as Student).StuFirstName + (Apmt_DisApmt_ListBox.SelectedItem as Student).StuLastName;
                Apmt_DisInstructor_TextBlock.Text = "Underviser: " + (Apmt_DisApmt_ListBox.SelectedItem as Instructor).InstFirstName + (Apmt_DisApmt_ListBox.SelectedItem as Instructor).InstLastName;
                Apmt_DisDateTime_TextBlock.Text = "Aftale: " + (Apmt_DisApmt_ListBox.SelectedItem as Appointment).ApmtDate;

                //Sets currentItem to equal the ID of selected item
                currentItem = (Apmt_DisApmt_ListBox.SelectedItem as Appointment).ApmtId;

                //Sets edit to false, as it is impossible for it to be true currently
                edit = false;

                //Controls which button the user can interact with - User needs able to edit and delete, but not save
                Apmt_Save_Button.IsEnabled = false;
                Apmt_Edit_Button.IsEnabled = true;
                Apmt_Delete_Button.IsEnabled = true;
            }
        }

        //Method to create, control and add items to the ListBox
        private void ListBoxFunction()
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates a count SqlCommand, which gets the number of rows in the table 
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_ApmtID) from VK_Appointments", con);
                //Saves count command result to int
                int intCount = (int)count.ExecuteScalar();


                //Make a list of 5-tuples containing instanses of each class necessary in apointments
                List<(Lesson, Class, Student, Instructor, Appointment)> items = new List<(Lesson, Class, Student, Instructor, Appointment)>();

                //Forloop which adds intCount number of new items to items-list
                for (int i = 0; i < intCount; i++)
                {
                    //Calls RetrieveLessonData method, sending i as index
                    RetrieveData(i);

                    //Add new items from the item class with specific attributes to the list
                    //Will later be remade to automatically add items based on the database

                    items.Add((new Lesson() { LesName = lessonToBeRetrieved.LesName, LesType = lessonToBeRetrieved.LesType },
                            new Class() { ClassName = classToBeRetrieved.ClassName },
                            new Student() { StuFirstName = studentToBeRetrieved.StuFirstName, StuLastName = studentToBeRetrieved.StuLastName }, //kan det konkatineres?
                            new Instructor() { InstFirstName = instructorToBeRetrieved.InstFirstName, InstLastName = instructorToBeRetrieved.InstLastName }, //kan det konkatineres? 
                            new Appointment() { ApmtDate = appointmentToBeRetrieved.ApmtDate, SetUp = "" }));

                    //Only necessary for multi-attribute ListBoxItem
                    //Set up the attribute 'SetUp' which is used to determine the appearance of the ListBoxItem 
                    //Forloop to go through all items in the items-list, to add and fill the 'SetUp' attribute

                    items[i].Item5.SetUp = $"{items[i].Item1.LesName} - {items[i].Item1.LesType}\n{items[i].Item5.ApmtDate}";
                }

                //Set the ItemsSource to the list, so that the ListBox uses the list to make the ListBoxItems
                Apmt_DisApmt_ListBox.ItemsSource = items;
            }
        }

        /*
        //Class to define the content of the ListBoxItems for the ListBox
        public class ApmtListBoxItems
        {
            //The attributes of the items for the ListBox
            public string LesName { get; set; }
            public string LesType { get; set; }
            public string Class { get; set; }
            public string Student { get; set; }
            public string Instructor { get; set; }
            public DateTime DateTime { get; set; }
            //Extra attribute, used for visuals (Only needed for multi-attribute views)
            public string SetUp { get; set; }
        }
        */

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


        //Retrieves the data of a specific row in the database where the row number is equal to dbRowNum + 1
        public void RetrieveData(int dbRowNumber)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();
                //Creates a four cmd SqlCommand, which SELECTs specific rows from each table in the DB 
                SqlCommand cmdLes = new SqlCommand("SELECT PK_LesID, LesName, LesType, LesDescription FROM VK_Lessons ORDER BY PK_LesID ASC OFFSET @dbRowNum ROWS FETCH NEXT 1 ROW ONLY", con);
                SqlCommand cmdInst = new SqlCommand("SELECT PK_InstID, InstFirstName, InstLastName, InstPhone, InstEmail FROM VK_Instructors ORDER BY PK_InstID ASC OFFSET @dBRowNumber ROWS FETCH NEXT 1 ROW ONLY", con);
                SqlCommand cmdStu = new SqlCommand("SELECT PK_StuCPR, StuFirstName, StuLastName, StuPhone, StuEmail FROM VK_Students ORDER BY PK_StuCPR ASC OFFSET @dBRowNumber ROWS FETCH NEXT 1 ROW ONLY", con);
                SqlCommand cmdClass = new SqlCommand("SELECT PK_ClassName, ClassYear, ClassNumber, ClassQuarter, ClassLicenseType FROM VK_Classes ORDER BY PK_ClassName ASC OFFSET @dbRowNum ROWS FETCH NEXT 1 ROW ONLY", con);

                //Set dbRowNumber to 0 if under 0
                if (dbRowNumber < 0)
                {
                    dbRowNumber = 0;
                }
                //Gives @dbRowNumber the value of dbRowNumber
                cmdLes.Parameters.AddWithValue("@dbRowNum", dbRowNumber);
                cmdInst.Parameters.AddWithValue("@dbRowNum", dbRowNumber);
                cmdStu.Parameters.AddWithValue("@dbRowNum", dbRowNumber);
                cmdClass.Parameters.AddWithValue("@dbRowNum", dbRowNumber);


                //Set up a data reader called dr, which reads the data from cmd (the previous sql command)
                using (SqlDataReader dr = cmdLes.ExecuteReader())
                {
                    //While-loop running while dr is reading 
                    while (dr.Read())
                    {
                        //Sets lesToBeRetrieve a new empty Lesson, which is then filled
                        lessonToBeRetrieved = new Lesson(0, "", "", "")
                        {
                            //Sets the attributes of lesToBeRetrieved equal to the data from the current row of the database
                            LesName = dr["LesName"].ToString(),
                            LesType = dr["LesType"].ToString(),
                        };
                    }
                }
                using (SqlDataReader dr = cmdInst.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        instructorToBeRetrieved = new Instructor(0, "", "", "", "")
                        {
                            InstFirstName = dr["InstFirstName"].ToString(),
                            InstLastName = dr["InstLastName"].ToString(),
                        };
                    }
                }
                using (SqlDataReader dr = cmdStu.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        studentToBeRetrieved = new Student("", "", "", "", "")
                        {
                            StuFirstName = dr["StuFirstName"].ToString(),
                            StuLastName = dr["StuLastName"].ToString(),
                        };
                    }
                }
                using (SqlDataReader dr = cmdClass.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        classToBeRetrieved = new Class(default, "", "", default, "")
                        {
                            ClassName = dr["PK_ClassName"].ToString(),
                            ClassYear = dr["ClassYear"].ToString(),
                            ClassNumber = dr["ClassNumber"].ToString(),
                            ClassQuarter = (Quarter)Enum.Parse(typeof(Quarter), dr["ClassQuarter"].ToString()),
                        };
                    }
                }
            }
        }
    }
}