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
using System.Security.Policy;

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
            ListBoxFunction();
            LockInputFields();

            //Controls which button the user can interact with - User needs able to edit and delete, but not save
            Apmt_Save_Button.IsEnabled = false;
            Apmt_Edit_Button.IsEnabled = false;
            Apmt_Delete_Button.IsEnabled = false;
        }

        List<Lesson> lessons;
        List<Instructor> instructors;
        List<Student> students;
        List<Class> classes;

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();

                SqlCommand countLessons = new SqlCommand("SELECT COUNT(PK_LesID) from VK_Lessons", con);
                int intCount = (int)countLessons.ExecuteScalar();

                if (intCount != lessons.Count())
                {
                    Apmt_PickLesson_ComboBox.Items.Clear();
                    AddLessonComboBoxFunction();
                }

                SqlCommand countInstructors = new SqlCommand("SELECT COUNT(PK_InstID) from VK_Instructors", con);
                intCount = (int)countInstructors.ExecuteScalar();
                
                if (intCount != instructors.Count())
                {
                    Apmt_PickInstructor_ComboBox.Items.Clear();
                    AddInstructorComboBoxFunction();
                }

                SqlCommand countStudents = new SqlCommand("SELECT COUNT(PK_StuCPR) from VK_Students", con);
                intCount = (int)countStudents.ExecuteScalar();

                if (intCount != students.Count())
                {
                    Apmt_PickStudent_ComboBox.Items.Clear();
                    AddStudentComboBoxFunction();
                }

                SqlCommand countClasses = new SqlCommand("SELECT COUNT(PK_ClassName) from VK_Classes", con);
                intCount = (int)countClasses.ExecuteScalar();

                if (intCount != classes.Count())
                {
                    Apmt_PickClass_ComboBox.Items.Clear();
                    AddClassComboBoxFunction();
                }
            }
        }

        public Appointment CurrentAppointment = new Appointment();
        public Lesson CurrentLesson = new Lesson();
        public Instructor CurrentInstructor = new Instructor();
        public Student CurrentStudent = new Student();
        public Class CurrentClass = new Class();
        public AppointmentListBox appointmentDetailsToBeRetrieved;
        public Lesson lessonToBeRetrieved;
        public Instructor instructorToBeRetrieved;
        public Student studentToBeRetrieved;
        public Class classToBeRetrieved;
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
                Apmt_DisLesson_TextBlock.Text = "Lektion: " + (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxLesName;
                Apmt_DisLessonType_TextBlock.Text = "Lektionstype: " + (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxLesType;
                Apmt_DisClass_TextBlock.Text = "Hold: " + (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxClassName;
                Apmt_DisClassLicenseType_TextBlock.Text = "Kørekorttype: " + (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxClassLicenseType;
                Apmt_DisStudent_TextBlock.Text = "Elev: " + (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxStuName;
                Apmt_DisInstructor_TextBlock.Text = "Underviser: " + (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxInstName;
                Apmt_DisDateTime_TextBlock.Text = "Aftale: " + (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxApmtDate;

                //Sets currentItem to equal the ID of selected item
                currentItem = (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxApmtId;

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
                    RetrieveAppointmentData(i);

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

                lessons = new List<Lesson>();

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

                instructors = new List<Instructor>();

                for (int i = 0; i < intCount; i++)
                {
                    RetrieveInstructorData(i);

                    instructors.Add(instructorToBeRetrieved);

                    instructorToBeRetrieved.Setup = $"{instructorToBeRetrieved.InstLastName}, {instructorToBeRetrieved.InstFirstName}";

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

                students = new List<Student>();

                for (int i = 0; i < intCount; i++)
                {
                    RetrieveStudentData(i);

                    students.Add(studentToBeRetrieved);

                    studentToBeRetrieved.Setup = $"{studentToBeRetrieved.StuLastName}, {studentToBeRetrieved.StuFirstName}";

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

                classes = new List<Class>();

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

                //Creates a SqlCommand, which SELECTs specific rows from all five tables in the DB and joins them 
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
                                                    "JOIN VK_Student_Appointment ON VK_Appointments.PK_ApmtID = VK_Student_Appointment.CK_ApmtID " +
                                                    "JOIN VK_Students ON VK_Student_Appointment.CK_StuCPR = VK_Students.PK_StuCPR " +
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
                        appointmentDetailsToBeRetrieved = new AppointmentListBox(Convert.ToInt32(dr["PK_ApmtID"]),
                                                                                 dr["LesName"].ToString(),
                                                                                 dr["LesType"].ToString(),
                                                                                 dr["PK_ClassName"].ToString(),
                                                                                 dr["ClassLicenseType"].ToString(),
                                                                                 dr["StuLastName"].ToString() + ", " + dr["StuFirstName"].ToString(),
                                                                                 dr["InstLastName"].ToString() + ", " + dr["InstFirstName"].ToString(),
                                                                                 (DateTime)dr["ApmtDate"],
                                                                                 "");
                    }
                }
            }
        }

        public class AppointmentListBox
        {
            public int ListBoxApmtId { get; set; }
            public string ListBoxLesName { get; set; }
            public string ListBoxLesType { get; set; }
            public string ListBoxClassName { get; set; }
            public string ListBoxClassLicenseType { get; set; }
            public string ListBoxStuName { get; set; }
            public string ListBoxInstName { get; set; }
            public DateTime ListBoxApmtDate { get; set; }
            public string Setup { get; set; }
            
            public AppointmentListBox(int _listBoxApmtId,
                                      string _listBoxLesName, 
                                      string _listBoxLesType,
                                      string _listBoxClassName,
                                      string _listBoxClassLicenseType,
                                      string _listBoxStuName,
                                      string _listBoxInstName,
                                      DateTime _listBoxApmtDate,
                                      string _setup)
            {
                ListBoxApmtId = _listBoxApmtId;
                ListBoxLesName = _listBoxLesName;
                ListBoxLesType = _listBoxLesType;
                ListBoxClassName = _listBoxClassName;
                ListBoxClassLicenseType = _listBoxClassLicenseType;
                ListBoxStuName = _listBoxStuName;
                ListBoxInstName = _listBoxInstName;
                ListBoxApmtDate = _listBoxApmtDate;
                Setup = _setup;
            }

            public AppointmentListBox() : this(0 ,"", "", "", "", "", "", DateTime.Now, "")
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

                SqlCommand cmdStu = new SqlCommand("INSERT INTO VK_Student_Appointment (CK_StuCPR, CK_ApmtID) " +
                                                   "VALUES(@CK_StuCPR, @CK_ApmtID) " +
                                                   "SELECT @@IDENTITY", con);
                cmdStu.Parameters.Add("@CK_StuCPR", SqlDbType.NVarChar).Value = studentToBeCreated.StuCPR;
                cmdStu.Parameters.Add("@CK_ApmtID", SqlDbType.Int).Value = appointmentToBeCreated.ApmtId;
                cmdStu.ExecuteScalar();
            }
        }
        
        //Edits the data of a previously existing Appointment
        public void EditAppointment(Appointment appointmentToBeCreated, Instructor instructorToBeCreated, Lesson lessonToBeCreated, Class classToBeCreated, Student studentToBeCreated)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates a cmd SqlCommand, which UPDATEs the attributes of a specific row in the table, based on the LesId
                SqlCommand cmdApmt = new SqlCommand("UPDATE VK_Appointments SET ApmtDate = @ApmtDate, FK_InstID = @InstID, FK_LesID = @LesID, FK_ClassName = @ClassName " +
                                                "WHERE PK_ApmtID = @ApmtID", con);

                //Gives @attribute the value of attribute
                cmdApmt.Parameters.AddWithValue("@ApmtDate", appointmentToBeCreated.ApmtDate);
                cmdApmt.Parameters.AddWithValue("@InstID", instructorToBeCreated.InstId);
                cmdApmt.Parameters.AddWithValue("@LesID", lessonToBeCreated.LesId);
                cmdApmt.Parameters.AddWithValue("@ClassName", classToBeCreated.ClassName);
                cmdApmt.Parameters.AddWithValue("@ApmtID", appointmentToBeCreated.ApmtId);
                cmdApmt.ExecuteNonQuery();

                SqlCommand cmdStu = new SqlCommand("UPDATE VK_Student_Appointment SET CK_StuCPR = @StuCPR " +
                                                   "WHERE CK_ApmtID = @ApmtID", con);
                cmdStu.Parameters.AddWithValue("@StuCPR", studentToBeCreated.StuCPR);
                cmdStu.Parameters.AddWithValue("@ApmtID", appointmentToBeCreated.ApmtId);
                cmdStu.ExecuteNonQuery();

                //Tells the database to execute the cmd sql command
            }
        }

        public void DeleteAppointment(int appointmentIdToBeDeleted) // DeleteAppointment-metoden defineres med parameteren int appointmentIdToBeDeleted,
                                                                    // Metoden tager CurrentAppointment.ListBoxApmtId (som har referencesemantisk lighed med currentItem)
                                                                    // som argument, når den kaldes
        {
            // Sql-Connection definerer forbindelsen 'con' til databasen
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open(); // 'Open' åbner forbindelsen 'con' til databasen
                SqlCommand cmd = new SqlCommand("DELETE FROM VK_Appointments WHERE PK_ApmtID = @PK_ApmtId", con); // SqlCommand definerer Sql-query-indholdet
                                                                                                                  // (en DELETE-kommando rettet mod en specifik
                                                                                                                  // tabel i databasen) af 'cmd', som skal
                                                                                                                  // sendes via forbindelsen 'con'

                cmd.Parameters.AddWithValue("@PK_ApmtId", appointmentIdToBeDeleted); // cmd.Parameters.AddWithValue sætter en SQL-variabel (@PK_InstId) lig
                                                                                    // med parameteren 'instructorIdToBeDeleted', der får sit argument, når
                                                                                    // metoden bliver kaldt
                cmd.ExecuteScalar(); // ExecuteScalar-metoden kører kommandoen cmd
            }

            ClearInputFields(); // Input-felterne cleares for at indikere, at sletningen er gennemført
        }

        private void Apmt_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            UnlockInputFields();
            Apmt_Save_Button.IsEnabled = true;
            Apmt_Edit_Button.IsEnabled = false;
        }

        private void Apmt_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            if (edit == false)
            { SaveAppointment(CurrentAppointment, CurrentInstructor, CurrentLesson, CurrentClass, CurrentStudent); }
            else
            {
                /*
                char delimitor = ',';
                int delimitorIndex = Apmt_PickLesson_ComboBox.Text.IndexOf(delimitor);
                CurrentLesson.LesName = Apmt_PickLesson_ComboBox.Text.Substring(0, delimitorIndex);
                CurrentLesson.LesType = Apmt_PickLesson_ComboBox.Text.Substring(delimitorIndex +2);
                delimitorIndex = Apmt_PickClass_ComboBox.Text.IndexOf(delimitor);
                CurrentClass.ClassName = Apmt_PickClass_ComboBox.Text.Substring(0, delimitorIndex);
                CurrentClass.ClassLicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), Apmt_PickClass_ComboBox.Text.Substring(delimitorIndex + 2));
                delimitorIndex = Apmt_PickStudent_ComboBox.Text.IndexOf(delimitor);
                CurrentStudent.StuFirstName = Apmt_PickStudent_ComboBox.Text.Substring(0, delimitorIndex);
                CurrentStudent.StuLastName = Apmt_PickStudent_ComboBox.Text.Substring(delimitorIndex +2);
                delimitorIndex = Apmt_PickInstructor_ComboBox.Text.IndexOf(delimitor);
                CurrentInstructor.InstFirstName = Apmt_PickInstructor_ComboBox.Text.Substring(0, delimitorIndex);
                CurrentInstructor.InstLastName = Apmt_PickInstructor_ComboBox.Text.Substring(delimitorIndex + 2);
                CurrentAppointment.ApmtDate = (DateTime)Apmt_PickDateTime_DateTimePicker.Value;
                */

                EditAppointment(CurrentAppointment, CurrentInstructor, CurrentLesson, CurrentClass, CurrentStudent);
            }

            Apmt_DisLesson_TextBlock.Text = "Lektion: ";
            Apmt_DisLessonType_TextBlock.Text = "Lektionstype: ";
            Apmt_DisClass_TextBlock.Text = "Hold: ";
            Apmt_DisClassLicenseType_TextBlock.Text = "Kørekorttype: ";
            Apmt_DisStudent_TextBlock.Text = "Elev: ";
            Apmt_DisInstructor_TextBlock.Text = "Underviser: ";
            Apmt_DisDateTime_TextBlock.Text = "Aftale: ";

            ClearInputFields();
            LockInputFields();
            ListBoxFunction();
            edit = false;
            Apmt_Edit_Button.IsEnabled = false;
            Apmt_Save_Button.IsEnabled = false;
            Apmt_Delete_Button.IsEnabled = false;
        }

        private void Apmt_Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            edit = true;
            Apmt_Save_Button.IsEnabled = true;
            UnlockInputFields();

            Apmt_PickLesson_ComboBox.Text = $"{(Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxLesName}, {(Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxLesType}";
            Apmt_PickClass_ComboBox.Text = $"{(Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxClassName}, {(Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxClassLicenseType}";
            Apmt_PickStudent_ComboBox.Text = $"{(Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxStuName}";
            Apmt_PickInstructor_ComboBox.Text = $"{(Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxInstName}";
            Apmt_PickDateTime_DateTimePicker.Text = $"{(Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxApmtDate}";
        }

        private void Apmt_Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            DeleteAppointment(currentItem);
            ListBoxFunction();

            Apmt_DisLesson_TextBlock.Text = "Lektion: ";
            Apmt_DisLessonType_TextBlock.Text = "Lektionstype: ";
            Apmt_DisClass_TextBlock.Text = "Hold: ";
            Apmt_DisClassLicenseType_TextBlock.Text = "Kørekorttype: ";
            Apmt_DisStudent_TextBlock.Text = "Elev: ";
            Apmt_DisInstructor_TextBlock.Text = "Underviser: ";
            Apmt_DisDateTime_TextBlock.Text = "Aftale: ";

            Apmt_Edit_Button.IsEnabled = false;
            Apmt_Save_Button.IsEnabled = false;
            Apmt_Delete_Button.IsEnabled = false;
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
        }
        
        private void Apmt_PickDateTime_DateTimePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            CurrentAppointment.ApmtDate = (DateTime)Apmt_PickDateTime_DateTimePicker.Value;
        }
    }
}
