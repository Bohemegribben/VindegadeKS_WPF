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
using System.Data;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;

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
            AddLessonComboBoxFunction();
            AddInstructorComboBoxFunction();
            AddStudentComboBoxFunction();
            AddClassComboBoxFunction();
            LockInputFields();
            ListBoxFunction();
        }
        public Appointment CurrentAppointment = new Appointment();
        AppointmentListBox appointmentDetailsToBeRetrieved;
        public Lesson CurrentLesson = new Lesson();
        Lesson lessonToBeRetrieved;
        public Instructor CurrentInstructor = new Instructor();
        Instructor instructorToBeRetrieved;
        public Student CurrentStudent = new Student();
        Student studentToBeRetrieved;
        public Class CurrentClass = new Class();
        Class classToBeRetrieved;
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
                
                //Make a list of 5-tuples containing instanses of each class necessary in appointments

                List<AppointmentListBox> appointments = new List<AppointmentListBox>();

                //Forloop which adds intCount number of new items to items-list
                for (int i = 0; i < intCount; i++)
                {
                    //Calls RetrieveLessonData method, sending i as index
                    RetrieveAppointmentData(i);;

                    //Add new items from the item class with specific attributes to the list
                    //Will later be remade to automatically add items based on the database

                    appointments.Add(appointmentDetailsToBeRetrieved);

                    //Only necessary for multi-attribute ListBoxItem
                    //Set up the attribute 'SetUp' which is used to determine the appearance of the ListBoxItem 
                    //Forloop to go through all items in the items-list, to add and fill the 'SetUp' attribute

                    appointments[i].Setup = $"{appointments[i].ListBoxLesName} - {appointments[i].ListBoxLesType} \n{appointments[i].ListBoxApmtDate}";
                }

                //Set the ItemsSource to the list, so that the ListBox uses the list to make the ListBoxItems
                Apmt_DisApmt_ListBox.ItemsSource = appointments;
            }
        }

        private void AddLessonComboBoxFunction()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();

                //Make lessons
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_LesID) from VK_Lessons", con);
                int intCount = (int)count.ExecuteScalar();

                List<Lesson> lessons = new List<Lesson>();

                for (int i = 0; i < intCount; i++)
                {
                    RetrieveLessonData(i);

                    lessons.Add(lessonToBeRetrieved);

                    lessonToBeRetrieved.Setup = $"{lessonToBeRetrieved.LesName}, {lessonToBeRetrieved.LesType}";
                    
                    Apmt_PickLesson_ComboBox.Items.Add(lessonToBeRetrieved.Setup);
                }
            }
        }
        private void AddInstructorComboBoxFunction()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();

                //Make classes
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_InstID) from VK_Instructors", con);
                int intCount = (int)count.ExecuteScalar();

                List<Instructor> instructors = new List<Instructor>();

                for (int i = 0; i < intCount; i++)
                {
                    RetrieveInstructorData(i);

                    instructors.Add(instructorToBeRetrieved);

                    instructorToBeRetrieved.Setup = $"{instructorToBeRetrieved.InstFirstName} {instructorToBeRetrieved.InstLastName}";

                    Apmt_PickInstructor_ComboBox.Items.Add(instructorToBeRetrieved.Setup);
                }
            }
        }
        
        private void AddStudentComboBoxFunction()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();

                //Make students
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_StuCPR) from VK_Students", con);
                int intCount = (int)count.ExecuteScalar();

                List<Student> students = new List<Student>();

                for (int i = 0; i < intCount; i++)
                {
                    RetrieveStudentData(i);

                    students.Add(studentToBeRetrieved);

                    studentToBeRetrieved.Setup = $"{studentToBeRetrieved.StuFirstName} {studentToBeRetrieved.StuLastName}";

                    Apmt_PickStudent_ComboBox.Items.Add(studentToBeRetrieved.Setup);
                }
            }
        }

        private void AddClassComboBoxFunction()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();

                //Make classes
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_ClassName) from VK_Classes", con);
                int intCount = (int)count.ExecuteScalar();

                List<Class> classes = new List<Class>();

                for (int i = 0; i < intCount; i++)
                {
                    RetrieveClassData(i);

                    classes.Add(classToBeRetrieved);

                    classToBeRetrieved.Setup = $"{classToBeRetrieved.ClassName}, {classToBeRetrieved.ClassLicenseType}";

                    Apmt_PickClass_ComboBox.Items.Add(classToBeRetrieved.Setup);
                }
            }
        }

        public void RetrieveLessonData(int dbRowNumber)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();
                //Creates a four cmd SqlCommand, which SELECTs specific rows from each table in the DB 
                SqlCommand cmdLes = new SqlCommand("SELECT PK_LesID, LesName, LesType, LesDescription FROM VK_Lessons ORDER BY PK_LesID ASC OFFSET @dbRowNumber ROWS FETCH NEXT 1 ROW ONLY", con);

                //Set dbRowNumber to 0 if under 0
                if (dbRowNumber < 0)
                {
                    dbRowNumber = 0;
                }
                //Gives @dbRowNumber the value of dbRowNumber
                cmdLes.Parameters.AddWithValue("@dbRowNumber", dbRowNumber);

                //Set up a data reader called dr, which reads the data from cmd (the previous sql command)
                using (SqlDataReader dr = cmdLes.ExecuteReader())
                {
                    //While-loop running while dr is reading 
                    while (dr.Read())
                    {
                        //Sets lesToBeRetrieve a new empty Lesson, which is then filled
                        lessonToBeRetrieved = new Lesson(int.Parse(dr["PK_LesID"].ToString()), dr["LesName"].ToString(), dr["LesType"].ToString(), dr["LesDescription"].ToString());
                    }
                }
            }
        }

        //Retrieves the data of a specific row in the database where the row number is equal to dbRowNum + 1
        public void RetrieveInstructorData(int dbRowNumber)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();
                //Creates a four cmd SqlCommand, which SELECTs specific rows from each table in the DB 
                SqlCommand cmdInst = new SqlCommand("SELECT PK_InstID, InstFirstName, InstLastName, InstPhone, InstEmail FROM VK_Instructors ORDER BY PK_InstID ASC OFFSET @dBRowNumber ROWS FETCH NEXT 1 ROW ONLY", con);

                //Set dbRowNumber to 0 if under 0
                if (dbRowNumber < 0)
                {
                    dbRowNumber = 0;
                }
                //Gives @dbRowNumber the value of dbRowNumber
                cmdInst.Parameters.AddWithValue("@dbRowNumber", dbRowNumber);

                using (SqlDataReader dr = cmdInst.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        instructorToBeRetrieved = new Instructor(int.Parse(dr["PK_InstID"].ToString()), dr["InstFirstName"].ToString(), dr["InstLastName"].ToString(), dr["InstPhone"].ToString(), dr["InstEmail"].ToString());
                    }
                }
            }
        }
        
        //Retrieves the data of a specific row in the database where the row number is equal to dbRowNumber + 1
        public void RetrieveStudentData(int dbRowNumber)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();
                //Creates a four cmd SqlCommand, which SELECTs specific rows from each table in the DB 
                SqlCommand cmdStu = new SqlCommand("SELECT PK_StuCPR, StuFirstName, StuLastName, StuPhone, StuEmail FROM VK_Students ORDER BY PK_StuCPR ASC OFFSET @dBRowNumber ROWS FETCH NEXT 1 ROW ONLY", con);

                //Set dbRowNumber to 0 if under 0
                if (dbRowNumber < 0)
                {
                    dbRowNumber = 0;
                }
                //Gives @dbRowNumber the value of dbRowNumber
                cmdStu.Parameters.AddWithValue("@dbRowNumber", dbRowNumber);

                using (SqlDataReader dr = cmdStu.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        studentToBeRetrieved = new Student(dr["PK_StuCPR"].ToString(), dr["StuFirstName"].ToString(), dr["StuLastName"].ToString(), dr["StuPhone"].ToString(), dr["StuEmail"].ToString());
                    }
                }
            }
        }
        
        //Retrieves the data of a specific row in the database where the row number is equal to dbRowNum + 1
        public void RetrieveClassData(int dbRowNumber)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();
                //Creates a four cmd SqlCommand, which SELECTs specific rows from each table in the DB 
                SqlCommand cmdClass = new SqlCommand("SELECT PK_ClassName, ClassYear, ClassNumber, ClassQuarter, ClassLicenseType FROM VK_Classes ORDER BY PK_ClassName ASC OFFSET @dbRowNumber ROWS FETCH NEXT 1 ROW ONLY", con);

                //Set dbRowNumber to 0 if under 0
                if (dbRowNumber < 0)
                {
                    dbRowNumber = 0;
                }
                //Gives @dbRowNumber the value of dbRowNumber
                cmdClass.Parameters.AddWithValue("@dbRowNumber", dbRowNumber);

                using (SqlDataReader dr = cmdClass.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        classToBeRetrieved = new Class((Quarter)Enum.Parse(typeof(Quarter), dr["ClassQuarter"].ToString()), dr["ClassYear"].ToString(), dr["ClassNumber"].ToString(), (LicenseType)Enum.Parse(typeof(LicenseType), dr["ClassLicenseType"].ToString()), dr["PK_ClassName"].ToString());
                    }
                }
            }
        }

        //Retrieves the data of a specific row in the database where the row number is equal to dbRowNum + 1
        public void RetrieveAppointmentData(int dbRowNumber)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();
                //Creates a four cmd SqlCommand, which SELECTs specific rows from each table in the DB 
                //SqlCommand cmdApmt = new SqlCommand("SELECT PK_ApmtID, ApmtDate, FK_InstID, FK_LesID, FK_ClassName FROM VK_Appointments ORDER BY PK_ApmtID ASC OFFSET @dbRowNumber ROWS FETCH NEXT 1 ROW ONLY", con);

                SqlCommand cmdApmt = new SqlCommand("SELECT " +
                                                    "VK_Lessons.LesName, " +
                                                    "VK_Lessons.LesType, " +
                                                    "VK_Classes.PK_ClassName, " +
                                                    "VK_Classes.ClassLicenseType, " +
                                                    "VK_Students.StuFirstName, " +
                                                    "VK_Students.StuLastName, " +
                                                    "VK_Instructors.InstFirstName, " +
                                                    "VK_Instructors.InstLastName, " +
                                                    "VK_Appointments.ApmtDate, " +
                                                    "VK_Appointments.PK_ApmtID " +
                                                    "FROM VK_Lessons " +
                                                    "JOIN VK_Appointments ON VK_Lessons.PK_LesID = VK_Appointments.FK_LesID " +
                                                    "JOIN VK_Classes ON VK_Appointments.FK_ClassName = VK_Classes.PK_ClassName " +
                                                    "JOIN VK_Class_Student ON VK_Classes.PK_ClassName = VK_Class_Student.CK_ClassName " +
                                                    "JOIN VK_Students ON VK_Class_Student.CK_StuCPR = VK_Students.PK_StuCPR " +
                                                    "JOIN VK_Instructors ON VK_Appointments.FK_InstID = VK_Instructors.PK_InstID " +
                                                    "GROUP BY " +
                                                    "VK_Lessons.LesName, " +
                                                    "VK_Lessons.LesType, " +
                                                    "VK_Classes.PK_ClassName, " +
                                                    "VK_Classes.ClassLicenseType, " +
                                                    "VK_Students.StuFirstName, " +
                                                    "VK_Students.StuLastName, " +
                                                    "VK_Instructors.InstFirstName, " +
                                                    "VK_Instructors.InstLastName, " +
                                                    "VK_Appointments.ApmtDate, " +
                                                    "VK_Appointments.PK_ApmtID " +
                                                    "ORDER BY " + 
                                                    "PK_ApmtID " +
                                                    "ASC OFFSET @dbRowNumber ROWS " +
                                                    "FETCH NEXT 1 ROW ONLY", con);


                //Set dbRowNumber to 0 if under 0
                if (dbRowNumber < 0)
                {
                    dbRowNumber = 0;
                }
                //Gives @dbRowNumber the value of dbRowNumber
                cmdApmt.Parameters.AddWithValue("@dbRowNumber", dbRowNumber);

                using (SqlDataReader dr = cmdApmt.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        appointmentDetailsToBeRetrieved = new AppointmentListBox(dr["LesName"].ToString(),
                                                                                 dr["LesType"].ToString(),
                                                                                 dr["PK_ClassName"].ToString(),
                                                                                 dr["StuFirstName"].ToString() + dr["StuLastName"].ToString(),
                                                                                 dr["InstFirstName"].ToString() + dr["InstLastName"].ToString(),
                                                                                 (DateTime)dr["ApmtDate"],
                                                                                 "");
                    }
                }
            }
        }

        public class AppointmentListBox
        {
            public string ListBoxLesName { get; set; }
            public string ListBoxLesType { get; set; }
            public string ListBoxClassName { get; set; }
            public string ListBoxStuName { get; set; }
            public string ListBoxInstName { get; set; }
            public DateTime ListBoxApmtDate { get; set; }

            public string Setup { get; set; }
            
            public AppointmentListBox(string _listBoxLesName, 
                                      string _listBoxLesType,
                                      string _listBoxClassName,
                                      string _listBoxStuName,
                                      string _listBoxInstName,
                                      DateTime _listBoxApmtDate,
                                      string _setup)
            {
                ListBoxLesName = _listBoxLesName;
                ListBoxLesType = _listBoxLesType;
                ListBoxClassName = _listBoxClassName;
                ListBoxStuName = _listBoxStuName;
                ListBoxInstName = _listBoxInstName;
                ListBoxApmtDate = _listBoxApmtDate;
                Setup = _setup;
            }

            public AppointmentListBox() : this("", "", "", "", "", default, "")
            { }
        }

        public void SaveAppointment(Appointment appointmentToBeCreated, Instructor instructorToBeCreated, Lesson lessonToBeCreated, Class classToBeCreated, Student studentToBeCreated)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmdApmt = new SqlCommand("INSERT INTO VK_Appointments (ApmtDate, FK_InstID, FK_LesID, FK_ClassName) " +
                                                    "VALUES(@ApmtDate, @FK_InstID, @FK_LesID, @FK_ClassName) " +
                                                    "SELECT @@IDENTITY", con);
                cmdApmt.Parameters.Add("@ApmtDate", SqlDbType.DateTime2).Value = appointmentToBeCreated.ApmtDate;
                cmdApmt.Parameters.Add("@FK_InstID", SqlDbType.Int).Value = instructorToBeCreated.InstId;
                cmdApmt.Parameters.Add("@FK_LesID", SqlDbType.Int).Value = lessonToBeCreated.LesId;
                cmdApmt.Parameters.Add("@FK_ClassName", SqlDbType.NVarChar).Value = classToBeCreated.ClassName;
                appointmentToBeCreated.ApmtId = Convert.ToInt32(cmdApmt.ExecuteScalar());

                SqlCommand cmdStu = new SqlCommand("INSERT INTO VK_STUDENT_APPOINTMENT (CK_StuCPR, CK_ApmtID) " +
                                                   "VALUES(@CK_StuCPR, @CK_ApmtID) " +
                                                   "SELECT @@IDENTITY", con);
                cmdStu.Parameters.Add("@CK_StuCPR", SqlDbType.NVarChar).Value = studentToBeCreated.StuCPR;
                cmdStu.Parameters.Add("@CK_ApmtID", SqlDbType.Int).Value = appointmentToBeCreated.ApmtId;
                cmdStu.ExecuteScalar();
            }
        }
        
        private void Apmt_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            UnlockInputFields();
        }

        private void Apmt_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveAppointment(CurrentAppointment, CurrentInstructor, CurrentLesson, CurrentClass, CurrentStudent);
            ClearInputFields();
            LockInputFields();
            ListBoxFunction();
        }

        private void Apmt_Edit_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Apmt_Delete_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearInputFields()
        {
            Apmt_PickLesson_ComboBox.SelectedItem = null;
            Apmt_PickClass_ComboBox.SelectedItem = null;
            Apmt_PickStudent_ComboBox.SelectedItem = null;
            Apmt_PickInstructor_ComboBox.SelectedItem = null;
            Apmt_PickDateTime_DateTimePicker.Value = DateTime.Now;
        }

        private void LockInputFields()
        {
            Apmt_PickLesson_ComboBox.IsEnabled = false;
            Apmt_PickClass_ComboBox.IsEnabled = false;
            Apmt_PickStudent_ComboBox.IsEnabled = false;
            Apmt_PickInstructor_ComboBox.IsEnabled = false;
        }

        private void UnlockInputFields()
        {
            Apmt_PickLesson_ComboBox.IsEnabled = true;
            Apmt_PickClass_ComboBox.IsEnabled = true;
            Apmt_PickStudent_ComboBox.IsEnabled = true;
            Apmt_PickInstructor_ComboBox.IsEnabled = true;
        }

        private void Apmt_ShowLessonType_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Apmt_PickLesson_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RetrieveLessonData(Apmt_PickLesson_ComboBox.SelectedIndex);
            CurrentLesson.LesId = lessonToBeRetrieved.LesId;
        }

        private void Apmt_PickClass_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RetrieveClassData(Apmt_PickClass_ComboBox.SelectedIndex);
            CurrentClass.ClassQuarter = classToBeRetrieved.ClassQuarter;
            CurrentClass.ClassYear = classToBeRetrieved.ClassYear;
            CurrentClass.ClassNumber = classToBeRetrieved.ClassNumber;
            CurrentClass.ClassName = classToBeRetrieved.ClassName;
        }

        private void Apmt_PickStudent_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RetrieveStudentData(Apmt_PickStudent_ComboBox.SelectedIndex);
            CurrentStudent.StuCPR = studentToBeRetrieved.StuCPR;
        }

        private void Apmt_PickInstructor_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RetrieveInstructorData(Apmt_PickInstructor_ComboBox.SelectedIndex);
            CurrentInstructor.InstId = instructorToBeRetrieved.InstId;
            //??? ListBoxFunction();
        }
        
        private void Apmt_PickDateTime_DateTimePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            CurrentAppointment.ApmtDate = (DateTime)Apmt_PickDateTime_DateTimePicker.Value;
        }
    }
}
