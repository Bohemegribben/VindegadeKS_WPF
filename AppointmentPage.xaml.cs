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
using System.Windows.Media.Animation;

namespace VindegadeKS_WPF
{
    /// <summary>
    /// Interaction logic for AppointmentPage.xaml
    /// </summary>
    public partial class AppointmentPage : Page
    {
        //The AppointmentPage constructor
        public AppointmentPage()
        {
            InitializeComponent();

            ComboBoxStartUp(); //Runs ComboBoxStartUp with all of the set up
                               //for the ComboBoxes to minimize clutter

            ListBoxFunction(); // Runs ListBoxFunction when the page is opened

            LockInputFields(); // Runs LockInputFields on startup so nothing
                               // can be entered before the user presses the Add/Tilføj-button

            //Controls which button the user can interact with.
            //Upon startup only the Add/Tilføj-Button is enabled
            Apmt_Add_Button.IsEnabled = true;
            Apmt_Save_Button.IsEnabled = false;
            Apmt_Edit_Button.IsEnabled = false;
            Apmt_Delete_Button.IsEnabled = false;
        }

        #region Variables and AppointmentListBox-class
        // The lists and 'ToBeRetrieved'-variables are declared outside the class methods
        // in order for all the methods to be able to access and use their alotted memory
        // on the heap 
        List<Lesson> lessons;
        List<Instructor> instructors;
        List<Student> students;
        List<Class> classes;

        AppointmentListBox appointmentDetailsToBeRetrieved;
        Lesson lessonToBeRetrieved;
        Instructor instructorToBeRetrieved;
        Student studentToBeRetrieved;
        Class classToBeRetrieved;

        // The Current-instances are declared AND initialised outside
        // of the methods to preserve their integrety (keep them the same)
        // whereever they are used. They are used to hold the
        // respective instanse currently selected in the ComboBox before it is
        // saved or edited.
        public Appointment CurrentAppointment = new Appointment(); 
        public Lesson CurrentLesson = new Lesson();
        public Instructor CurrentInstructor = new Instructor();
        public Student CurrentStudent = new Student();
        public Class CurrentClass = new Class();

        // The idOfSelectedListBoxItem is used to hold the ListBoxApmtId
        // selected in the ListBox. It is an integer used as a parameter
        // in the DeleteAppointment-method.
        int idOfSelectedListBoxItem;

        //The boolian edit-variable is a switch, used to control when the
        //save/gem-button is saving a new dataset to the database vs when
        //it is editing an existing one. It is set to true, when the
        //edit/rediger-button is pressed whereafter the save/gem-button
        //edits instead of saves.
        bool edit = false;

        //A page-specific class (with constructors) used by the Appointment-page
        //to store data from the tables VK_Appointments, VK_Lessons, VK_Classes,
        //VK_Students, VK_Instructors to be displayed in the AppointmentListBox
        //as well as an extra attribute, Setup, used to hold the strings to be
        //displayed (we could have overwritten the ToString-Method of the Object-
        //class instead)
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
        #endregion

        #region Buttons
        //Enables the user to add new Appointment
        private void Apmt_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            UnlockInputFields(); //Unlocks the input fields

            edit = false; //Sets edit to false, as it is impossible
                          //for it to be true currently

            //Controls which button the user can interact with.
            //User needs to save, but shouldn't interact with Edit/Delete
            //as Add is adding a new Lesson
            Apmt_Save_Button.IsEnabled = true;
            Apmt_Edit_Button.IsEnabled = false;
            Apmt_Delete_Button.IsEnabled = false;
        }

        //Saves the Appointment from the input fields
        private void Apmt_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            //If-statement checking wether the edit-bool is true or false. Only
            //if the edit-button has been pressed is the edit-bool true. 
            //If it's false edited the selected data is to be saved as a new
            //entity in the database - run SaveNewLesson(CurrentAppointment).
            //If it is true an old entity in the database is to be edited -
            //run UpdateLesson(CurrentAppointment)
            if (edit == false)
            { SaveAppointment(CurrentAppointment, CurrentInstructor, CurrentLesson, CurrentClass, CurrentStudent); }
            else
            { EditAppointment(CurrentAppointment, CurrentInstructor, CurrentLesson, CurrentClass, CurrentStudent); }

            //Removes the displayed (saved or edited) data in the stackpanel
            //textblocks and sets the default text to category+blank to
            //indicate to the user that the save or edit-method have been run  
            Apmt_DisLesson_TextBlock.Text = "Lektion: ";
            Apmt_DisLessonType_TextBlock.Text = "Lektionstype: ";
            Apmt_DisClass_TextBlock.Text = "Hold: ";
            Apmt_DisClassLicenseType_TextBlock.Text = "Kørekorttype: ";
            Apmt_DisStudent_TextBlock.Text = "Elev: ";
            Apmt_DisInstructor_TextBlock.Text = "Underviser: ";
            Apmt_DisDateTime_TextBlock.Text = "Aftale: ";

            //Clears and locks the input fields and reruns the ListBoxFunction
            //to make sure it displays the updated list of appointments
            //currently in the database
            ClearInputFields();
            LockInputFields();
            ListBoxFunction();

            //Sets edit to false, as it is impossible
            //for it to be true currently
            edit = false;

            //Controls which button the user can interact with.
            //User needs to be able to Add more Lessons, but nothing else
            Apmt_Add_Button.IsEnabled = true;
            Apmt_Edit_Button.IsEnabled = false;
            Apmt_Save_Button.IsEnabled = false;
            Apmt_Delete_Button.IsEnabled = false;
        }

        //Lets the user edit previously created Appointment
        private void Apmt_Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            //Unlocks the input fields
            UnlockInputFields();

            //Sets edit to true, as the user is currently editing
            //the Appointment
            edit = true;

            //Controls which button the user can interact with.
            //User needs able to save, but nothing else
            Apmt_Add_Button.IsEnabled = false;
            Apmt_Save_Button.IsEnabled = true;
            Apmt_Edit_Button.IsEnabled = false;
            Apmt_Delete_Button.IsEnabled = false;

            //Sets the ComboBox input fields to be equal to the
            //data from the AppointmentListBox
            Apmt_PickLesson_ComboBox.Text = $"{(Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxLesName}, {(Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxLesType}";
            Apmt_PickClass_ComboBox.Text = $"{(Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxClassName}, {(Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxClassLicenseType}";
            Apmt_PickStudent_ComboBox.Text = $"{(Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxStuName}";
            Apmt_PickInstructor_ComboBox.Text = $"{(Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxInstName}";
            Apmt_PickDateTime_DateTimePicker.Text = $"{(Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxApmtDate}";
        }

        //Lets the user delete previously created Appointments
        private void Apmt_Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            //Runs DeleteApppointment while feeding it idOfSelectedListBoxItem
            DeleteAppointment(idOfSelectedListBoxItem);

            //Clears and locks the input fields and reruns the ListBoxFunction
            //to make sure it displays the updated list of appointments
            //currently in the database
            ClearInputFields();
            LockInputFields();
            ListBoxFunction();

            //Removes the displayed (deleted) data from the stackpanel
            //textblocks and sets the default text to category+blank to
            //indicate to the user that the delete-method have been run  
            Apmt_DisLesson_TextBlock.Text = "Lektion: ";
            Apmt_DisLessonType_TextBlock.Text = "Lektionstype: ";
            Apmt_DisClass_TextBlock.Text = "Hold: ";
            Apmt_DisClassLicenseType_TextBlock.Text = "Kørekorttype: ";
            Apmt_DisStudent_TextBlock.Text = "Elev: ";
            Apmt_DisInstructor_TextBlock.Text = "Underviser: ";
            Apmt_DisDateTime_TextBlock.Text = "Aftale: ";

            //Controls which button the user can interact with.
            //User needs able to add, but nothing else
            Apmt_Add_Button.IsEnabled = true;
            Apmt_Edit_Button.IsEnabled = false;
            Apmt_Save_Button.IsEnabled = false;
            Apmt_Delete_Button.IsEnabled = false;
        }
        #endregion

        #region ListBox
        //What happens when you select an item in the ListBox
        private void Apmt_DisApmt_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Safety check, to make sure that the selected item exists
            if (Apmt_DisApmt_ListBox.SelectedItem != null)
            {
                //Changes the data displayed in the textblocks ín the
                //stackpanel display window, by setting them equal to the
                //properties in the selected AppointmentListBox-item.
                Apmt_DisLesson_TextBlock.Text = "Lektion: " + (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxLesName;
                Apmt_DisLessonType_TextBlock.Text = "Lektionstype: " + (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxLesType;
                Apmt_DisClass_TextBlock.Text = "Hold: " + (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxClassName;
                Apmt_DisClassLicenseType_TextBlock.Text = "Kørekorttype: " + (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxClassLicenseType;
                Apmt_DisStudent_TextBlock.Text = "Elev: " + (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxStuName;
                Apmt_DisInstructor_TextBlock.Text = "Underviser: " + (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxInstName;
                Apmt_DisDateTime_TextBlock.Text = "Aftale: " + (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxApmtDate;

                //Sets idOfSelectedListBoxItem to equal the ID of selected AppointmentListBox-item
                idOfSelectedListBoxItem = (Apmt_DisApmt_ListBox.SelectedItem as AppointmentListBox).ListBoxApmtId;

                //Runs AppointmentDataToBeEdited to fill CurrentAppointment with data from the selected AppointmentListBox-item based on the idOfSelectedListBoxItem. 
                AppointmentDataToBeEdited(idOfSelectedListBoxItem);

                //Sets edit to false, as it is impossible for it to be true currently
                edit = false;

                //Controls which button the user can interact with.
                //User needs able to edit and delete (and add), but not save.
                Apmt_Add_Button.IsEnabled = true;
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

                //Creates a count SqlCommand, which gets the number of rows in the specified table 
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_ApmtID) from VK_Appointments", con);

                //Saves count command result to int
                int intCount = (int)count.ExecuteScalar();

                //Instansiate a new list AppointmentListBox-items
                List<AppointmentListBox> appointments = new List<AppointmentListBox>();

                //For-loop which adds intCount number of new items to items-list
                for (int i = 0; i < intCount; i++)
                {
                    //Calls RetrieveAppointmentData-method, sending i as index
                    RetrieveAppointmentData(i);

                    //Add new AppointmentListBox-items from the AppointmentListBox-class with specific attributes to the list
                    appointments.Add(appointmentDetailsToBeRetrieved);


                    //Only necessary for multi-attribute ListBoxItem
                    //Set up the attribute 'SetUp' which is used to determine the appearance of the ListBoxItem 
                    //Forloop to go through all items in the items-list, to add and fill the 'SetUp' attribute


                    //Set up the attribute 'Setup' which is used to determine
                    //the appearance of the AppointmentListBox-item
                    //For-loop to go through all appointments in the
                    //appointments-list, to add and fill the Setup-attribute

                    appointments[i].Setup = $"{appointments[i].ListBoxLesName} - {appointments[i].ListBoxLesType} \n{appointments[i].ListBoxApmtDate}";
                }

                //Set the ItemsSource to the appointments-list, so that the
                //ListBox uses the list to make the AppointmentListBox-items
                Apmt_DisApmt_ListBox.ItemsSource = appointments;
            }
        }
        #endregion

        #region ComboBox Setup Methods
        //Run by the AppointmentPage-constructor to minimize the clutter in the constructor
        private void ComboBoxStartUp()
        {
            //Runs the methods defined below
            AddLessonComboBoxFunction();
            AddInstructorComboBoxFunction();
            AddStudentComboBoxFunction();
            AddClassComboBoxFunction();
        }

        //Set up the AddLessonComboBoxFunction
        private void AddLessonComboBoxFunction()
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Make a count command to find how many lassons exist in VK_Lessons
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_LesID) from VK_Lessons", con);

                //Run the command and saves the resultat in intCount
                int intCount = (int)count.ExecuteScalar();

                //Initialize the lessons-list previously declared 
                lessons = new List<Lesson>();

                //Forloop which runs intCount amount of times
                for (int i = 0; i < intCount; i++)
                {
                    //Retrives the data from VK_Lessons at index i
                    RetrieveLessonData(i);

                    //Adds the lesson retrieved by RetrieveLessonData to the list
                    lessons.Add(lessonToBeRetrieved);

                    //Sets Setup to what should be displayed in the ComboBox
                    lessonToBeRetrieved.Setup = $"{lessonToBeRetrieved.LesName}, {lessonToBeRetrieved.LesType}";

                    //Displayes the string in setup as an item in the ComboBox 
                    Apmt_PickLesson_ComboBox.Items.Add(lessonToBeRetrieved.Setup);
                }
            }
        }

        //Set up the AddClassComboBoxFunction
        private void AddClassComboBoxFunction()
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Make a count command to find how many classes exist in VK_Classes
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_ClassName) from VK_Classes", con);

                //Run the command and saves the resultat in intCount
                int intCount = (int)count.ExecuteScalar();

                //Initialize the classes-list previously declared 
                classes = new List<Class>();

                //Forloop which runs intCount amount of times
                for (int i = 0; i < intCount; i++)
                {
                    //Retrives the data from VK_Classes at index i
                    RetrieveClassData(i);

                    //Adds the class retrieved by RetrieveClassData to the list
                    classes.Add(classToBeRetrieved);

                    //Sets Setup to what should be displayed in the ComboBox
                    classToBeRetrieved.Setup = $"{classToBeRetrieved.ClassName}, {classToBeRetrieved.ClassLicenseType}";

                    //Displayes the string in setup as an item in the ComboBox 
                    Apmt_PickClass_ComboBox.Items.Add(classToBeRetrieved.Setup);
                }
            }
        }

        //Set up the AddStudentComboBoxFunction
        private void AddStudentComboBoxFunction()
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Make a count command to find how many students exist in VK_Students
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_StuCPR) from VK_Students", con);

                //Run the command and saves the resultat in intCount
                int intCount = (int)count.ExecuteScalar();

                //Initialize the students-list previously declared 
                students = new List<Student>();

                //Forloop which runs intCount amount of times
                for (int i = 0; i < intCount; i++)
                {
                    //Retrives the data from VK_Students at index i
                    RetrieveStudentData(i);

                    //Adds the student retrieved by RetrieveStudentData to the list
                    students.Add(studentToBeRetrieved);

                    //Sets Setup to what should be displayed in the ComboBox
                    studentToBeRetrieved.Setup = $"{studentToBeRetrieved.StuLastName}, {studentToBeRetrieved.StuFirstName}";

                    //Displayes the string in setup as an item in the Combobox 
                    Apmt_PickStudent_ComboBox.Items.Add(studentToBeRetrieved.Setup);
                }
            }
        }

        //Set up the AddInstructorComboBoxFunction
        private void AddInstructorComboBoxFunction()
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Make a count command to find how many instructors exist in VK_Instructors
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_InstID) from VK_Instructors", con);

                //Run the command and saves the resultat in intCount
                int intCount = (int)count.ExecuteScalar();

                //Initialize the instructors-list previously declared 
                instructors = new List<Instructor>();

                //Forloop which runs intCount amount of times
                for (int i = 0; i < intCount; i++)
                {
                    //Retrives the data from VK_Instructors at index i
                    RetrieveInstructorData(i);

                    //Adds the instructor retrieved by RetrieveInstructorData to the list
                    instructors.Add(instructorToBeRetrieved);

                    //Sets Setup to what should be displayed in the ComboBox
                    instructorToBeRetrieved.Setup = $"{instructorToBeRetrieved.InstLastName}, {instructorToBeRetrieved.InstFirstName}";

                    //Displayes the string in setup as an item in the ComboBox 
                    Apmt_PickInstructor_ComboBox.Items.Add(instructorToBeRetrieved.Setup);
                }
            }
        }
        #endregion

        #region ComboBox controls (and Datetimepicker control)
        //Event for when a lesson is selected in Apmt_PickLesson_ComboBox
        private void Apmt_PickLesson_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Retrives the data from VK_Lessons at the index of the selected ComboBox-item 
            RetrieveLessonData(Apmt_PickLesson_ComboBox.SelectedIndex);

            //Sets the LesId-property of the previously initialized CurrentLesson
            //to the same as the LesId-property of the RetrieveLessonData-item,
            //lessonToBeRetrieved. CurrentLesson will hold the data to be saved or edited.
            CurrentLesson.LesId = lessonToBeRetrieved.LesId;
        }

        //Event for when a class is selected in Apmt_PickClass_ComboBox
        private void Apmt_PickClass_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Retrives the data from VK_Classes at the index of the selected ComboBox-item 
            RetrieveClassData(Apmt_PickClass_ComboBox.SelectedIndex);

            //Sets the all the properties (used to construct the ClassName-property)
            //of the previously initialized CurrentClass to the same as the
            //corresponding properties of the RetrieveClassData-item, classToBeRetrieved.
            //CurrentClass will hold the data to be saved or edited.
            CurrentClass.ClassQuarter = classToBeRetrieved.ClassQuarter;
            CurrentClass.ClassYear = classToBeRetrieved.ClassYear;
            CurrentClass.ClassNumber = classToBeRetrieved.ClassNumber;
            CurrentClass.ClassName = classToBeRetrieved.ClassName;
        }

        //Event for when a student is selected in Apmt_PickStudent_ComboBox
        private void Apmt_PickStudent_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Retrives the data from VK_Students at the index of the selected ComboBox-item
            RetrieveStudentData(Apmt_PickStudent_ComboBox.SelectedIndex);

            //Sets the StuCPR-property of the previously initialized CurrentStudent
            //to the same as the StuCPR-property of the RetrieveStudentData-item,
            //studentToBeRetrieved. CurrentStudent will hold the data to be saved or edited.
            CurrentStudent.StuCPR = studentToBeRetrieved.StuCPR;
        }

        //Event for when a instructor is selected in Apmt_PickInstructor_ComboBox
        private void Apmt_PickInstructor_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Retrives the data from VK_Instructors at the index of the selected ComboBox-item
            RetrieveInstructorData(Apmt_PickInstructor_ComboBox.SelectedIndex);

            //Sets the InstId-property of the previously initialized CurrentInstructor
            //to the same as the InstId-property of the RetrieveInstructorData-item,
            //instructorToBeRetrieved. CurrentInstructor will hold the data to be saved or edited.
            CurrentInstructor.InstId = instructorToBeRetrieved.InstId;
        }

        //Event for when a DateTime is selected in Apmt_PickDateTime_DateTimePicker
        private void Apmt_PickDateTime_DateTimePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //Sets the ApmtDate-property of the previously initialized CurrentAppointment
            //to the same as the value of the DateTimePicker.
            //CurrentAppointment will hold the data to be saved or edited.
            CurrentAppointment.ApmtDate = (DateTime)Apmt_PickDateTime_DateTimePicker.Value;
        }

        //The OnLoad-method ensures that instanses of Lesson, Instructor, Student and Class, created 
        //while the program is running, is displayed in the ComboBoxes on the AppoinmentPage. Without this
        //method only instanses of Lesson, Instructor, Student and Class already in the database at 
        //the initialization of the program would be displayed 
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            //The ListBoxFunction is called OnLoad to ensure that if enteties have been deleted elsewhere in the program
            //the affected appointments are also deleted.
            ListBoxFunction();

            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Make a count command to find how many lessons exist in VK_Lessons
                SqlCommand countLessons = new SqlCommand("SELECT COUNT(PK_LesID) from VK_Lessons", con);

                //Run the command and saves the resultat in intCount. intCount is reused later.
                int intCount = (int)countLessons.ExecuteScalar();

                //The if-statement checks if the intCount is different from the amount of
                //lessons in the lessons-list by way of the Count-method.
                if (intCount != lessons.Count())
                {
                    Apmt_PickLesson_ComboBox.Items.Clear(); //If so, the items in Apmt_PickLesson_ComboBox
                                                            //is cleared by way of the Clear-method.

                    AddLessonComboBoxFunction(); //and the AddLessonComboBoxFunction is called to
                                                 //fill the ComboBox again.
                }

                //Make a count command to find how many instructors exist in VK_Instructors.
                SqlCommand countInstructors = new SqlCommand("SELECT COUNT(PK_InstID) from VK_Instructors", con);

                //Run the command and saves the resultat in intCount. intCount is reused.
                intCount = (int)countInstructors.ExecuteScalar();

                //The if-statement checks if the intCount is different from the amount of
                //instructors in the instructors-list by way of the Count-method.
                if (intCount != instructors.Count())
                {
                    Apmt_PickInstructor_ComboBox.Items.Clear(); //If so, the items in Apmt_PickInstructor_ComboBox
                                                                //is cleared by way of the Clear-method

                    AddInstructorComboBoxFunction(); //And the AddInstructorComboBoxFunction is called to fill
                                                     //the ComboBox again.
                }

                //Make a count command to find how many students exist in VK_Students
                SqlCommand countStudents = new SqlCommand("SELECT COUNT(PK_StuCPR) from VK_Students", con);

                //Run the command and saves the resultat in intCount. intCount is reused.
                intCount = (int)countStudents.ExecuteScalar();

                //The if-statement checks if the intCount is different from the amount of
                //students in the students-list by way of the Count-method.
                if (intCount != students.Count())
                {
                    Apmt_PickStudent_ComboBox.Items.Clear(); //If so, the items in Apmt_PickStudent_ComboBox
                                                             //is cleared by way of the Clear-method

                    AddStudentComboBoxFunction(); //And the AddStudentComboBoxFunction is called to fill
                                                  //the ComboBox again.
                }

                //Make a count command to find how many classes exist in VK_Classes
                SqlCommand countClasses = new SqlCommand("SELECT COUNT(PK_ClassName) from VK_Classes", con);

                //Run the command and saves the resultat in intCount. intCount is reused.
                intCount = (int)countClasses.ExecuteScalar();

                //The if-statement checks if the intCount is different from the amount of
                //classes in the classes-list by way of the Count-method.
                if (intCount != classes.Count())
                {
                    Apmt_PickClass_ComboBox.Items.Clear(); //If so, the items in Apmt_PickClass_ComboBox
                                                           //is cleared by way of the Clear-method

                    AddClassComboBoxFunction(); //And the AddClassComboBoxFunction is called to fill
                                                //the ComboBox again.
                }
            }
        }
        #endregion

        #region Retrieve methods
        //Retrieves the lesson of a specific row in the database
        //where the row number is equal to dbRowNumber + 1
        public void RetrieveLessonData(int dbRowNumber)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates an SqlCommand, cmdLes, which SELECTs specific rows from each table in the DB 
                SqlCommand cmdLes = new SqlCommand("SELECT PK_LesID, LesName, LesType, LesDescription FROM VK_Lessons ORDER BY PK_LesID ASC OFFSET @dbRowNumber ROWS FETCH NEXT 1 ROW ONLY", con);

                //Set dbRowNumber to 0 if under 0
                if (dbRowNumber < 0)
                {
                    dbRowNumber = 0;
                }
                //Gives @dbRowNumber the value of dbRowNumber
                cmdLes.Parameters.AddWithValue("@dbRowNumber", dbRowNumber);

                //Set up a data reader called dr, which reads the data from cmdLes (the previous sql command)
                using (SqlDataReader dr = cmdLes.ExecuteReader())
                {
                    //While-loop running while dr is reading 
                    while (dr.Read())
                    {
                        //Sets lessonToBeRetrieved a new empty Lesson, which is then filled
                        lessonToBeRetrieved = new Lesson(int.Parse(dr["PK_LesID"].ToString()), dr["LesName"].ToString(), dr["LesType"].ToString(), dr["LesDescription"].ToString());
                    }
                }
            }
        }

        //Retrieves the instructor of a specific row in the database
        //where the row number is equal to dbRowNumber + 1
        public void RetrieveInstructorData(int dbRowNumber)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates an SqlCommand, cmdInst, which SELECTs specific rows from each table in the DB 
                SqlCommand cmdInst = new SqlCommand("SELECT PK_InstID, InstFirstName, InstLastName, InstPhone, InstEmail FROM VK_Instructors ORDER BY PK_InstID ASC OFFSET @dBRowNumber ROWS FETCH NEXT 1 ROW ONLY", con);

                //Set dbRowNumber to 0 if under 0
                if (dbRowNumber < 0)
                {
                    dbRowNumber = 0;
                }
                //Gives @dbRowNumber the value of dbRowNumber
                cmdInst.Parameters.AddWithValue("@dbRowNumber", dbRowNumber);

                //Set up a data reader called dr, which reads the data from cmdInst (the previous sql command)
                using (SqlDataReader dr = cmdInst.ExecuteReader())
                {
                    //While-loop running while dr is reading 
                    while (dr.Read())
                    {
                        //Sets instructorToBeRetrieved, a new empty Instructor, which is then filled
                        instructorToBeRetrieved = new Instructor(int.Parse(dr["PK_InstID"].ToString()), dr["InstFirstName"].ToString(), dr["InstLastName"].ToString(), dr["InstPhone"].ToString(), dr["InstEmail"].ToString());
                    }
                }
            }
        }

        //Retrieves the student of a specific row in the database
        //where the row number is equal to dbRowNumber + 1
        public void RetrieveStudentData(int dbRowNumber)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates an SqlCommand, cmdStu, which SELECTs specific rows from each table in the DB 
                SqlCommand cmdStu = new SqlCommand("SELECT PK_StuCPR, StuFirstName, StuLastName, StuPhone, StuEmail FROM VK_Students ORDER BY PK_StuCPR ASC OFFSET @dBRowNumber ROWS FETCH NEXT 1 ROW ONLY", con);

                //Set dbRowNumber to 0 if under 0
                if (dbRowNumber < 0)
                {
                    dbRowNumber = 0;
                }
                //Gives @dbRowNumber the value of dbRowNumber
                cmdStu.Parameters.AddWithValue("@dbRowNumber", dbRowNumber);

                //Set up a data reader called dr, which reads the data from cmdStu (the previous sql command)
                using (SqlDataReader dr = cmdStu.ExecuteReader())
                {
                    //While-loop running while dr is reading 
                    while (dr.Read())
                    {
                        //Sets studentToBeRetrieved, a new empty Student, which is then filled
                        studentToBeRetrieved = new Student(dr["PK_StuCPR"].ToString(), dr["StuFirstName"].ToString(), dr["StuLastName"].ToString(), dr["StuPhone"].ToString(), dr["StuEmail"].ToString());
                    }
                }
            }
        }

        //Retrieves the class of a specific row in the database
        //where the row number is equal to dbRowNumber + 1
        public void RetrieveClassData(int dbRowNumber)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates an SqlCommand, cmdClass, which SELECTs specific rows from each table in the DB 
                SqlCommand cmdClass = new SqlCommand("SELECT PK_ClassName, ClassYear, ClassNumber, ClassQuarter, ClassLicenseType FROM VK_Classes ORDER BY PK_ClassName ASC OFFSET @dbRowNumber ROWS FETCH NEXT 1 ROW ONLY", con);

                //Set dbRowNumber to 0 if under 0
                if (dbRowNumber < 0)
                {
                    dbRowNumber = 0;
                }
                //Gives @dbRowNumber the value of dbRowNumber
                cmdClass.Parameters.AddWithValue("@dbRowNumber", dbRowNumber);

                //Set up a data reader called dr, which reads the data from cmdClass (the previous sql command)
                using (SqlDataReader dr = cmdClass.ExecuteReader())
                {
                    //While-loop running while dr is reading 
                    while (dr.Read())
                    {
                        //Sets classToBeRetrieved, a new empty Class, which is then filled
                        classToBeRetrieved = new Class((Quarter)Enum.Parse(typeof(Quarter), dr["ClassQuarter"].ToString()), dr["ClassYear"].ToString(), dr["ClassNumber"].ToString(), (LicenseType)Enum.Parse(typeof(LicenseType), dr["ClassLicenseType"].ToString()), dr["PK_ClassName"].ToString());
                    }
                }
            }
        }

        //Retrieves the appointment of a specific row in the database
        //where the row number is equal to dbRowNumber + 1
        public void RetrieveAppointmentData(int dbRowNumber)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates an SqlCommand, cmdApmt, which SELECTs specific rows from all five tables in the DB and joins them 
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

                //Set up a data reader called dr, which reads the data from cmdApmt (the previous sql command)
                using (SqlDataReader dr = cmdApmt.ExecuteReader())
                {
                    //While-loop running while dr is reading 
                    while (dr.Read())
                    {
                        //Sets appointmentDetailsToBeRetrieved, a new empty Appointment,
                        //which is then filled with specific data SELECT'ed from the database. 
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
        #endregion

        #region Save, edit and delete methods
        //Saves the data of a new Appointment by creating a new row
        //in the database from appointmentToBeCreated
        public void SaveAppointment(Appointment appointmentToBeCreated, Instructor instructorToBeCreated, Lesson lessonToBeCreated, Class classToBeCreated, Student studentToBeCreated)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates an SqlCommand, cmdApmt, which enables the ability to
                //INSERT the corresponding attributes INTO the table VK_Appointments in a new row.
                SqlCommand cmdApmt = new SqlCommand("INSERT INTO VK_Appointments (ApmtDate, FK_InstID, FK_LesID, FK_ClassName) " +
                                                    "VALUES(@ApmtDate, @FK_InstID, @FK_LesID, @FK_ClassName) " +
                                                    "SELECT @@IDENTITY", con);

                //Sets the database properties for columns ApmtDate, FK_InstID, FK_LesID, FK_ClassName in
                //VK_Appointments to the corresponding values in appointmentToBeCreated, instructorToBeCreated,
                //lessonToBeCreated and classToBeCreated.
                cmdApmt.Parameters.Add("@ApmtDate", SqlDbType.DateTime2).Value = appointmentToBeCreated.ApmtDate;
                cmdApmt.Parameters.Add("@FK_InstID", SqlDbType.Int).Value = instructorToBeCreated.InstId;
                cmdApmt.Parameters.Add("@FK_LesID", SqlDbType.Int).Value = lessonToBeCreated.LesId;
                cmdApmt.Parameters.Add("@FK_ClassName", SqlDbType.NVarChar).Value = classToBeCreated.ClassName;

                //Tells the database to execute the sqlcommand cmdApmt and assign an int to ApmtId and set
                //appointmentToBeCreated.ApmtId to said Id - row is selected by autoincrement.
                appointmentToBeCreated.ApmtId = Convert.ToInt32(cmdApmt.ExecuteScalar());


                //Creates an SqlCommand, cmdStu, which enables the ability to
                //INSERT the corresponding attributes INTO the connecting table VK_Student_Appointment in a new row.
                //We used two SqlCommands since we wanted to insert into two different tables simultaneously.
                SqlCommand cmdStu = new SqlCommand("INSERT INTO VK_Student_Appointment (CK_StuCPR, CK_ApmtID) " +
                                                   "VALUES(@CK_StuCPR, @CK_ApmtID) " +
                                                   "SELECT @@IDENTITY", con);

                //Sets the database properties for columns CK_StuCPR, CK_ApmtID in
                //VK_Student_Appointment to the corresponding values in studentToBeCreated, appointmentToBeCreated.
                cmdStu.Parameters.Add("@CK_StuCPR", SqlDbType.NVarChar).Value = studentToBeCreated.StuCPR;
                cmdStu.Parameters.Add("@CK_ApmtID", SqlDbType.Int).Value = appointmentToBeCreated.ApmtId;

                //Tells the database to execute the sqlcommand cmdStu, connecting the two composite keys in
                //the connecting table.
                cmdStu.ExecuteScalar();
            }
        }

        //Retrieves the data of the appointment with ApmtId equal to the method parameter _apmtId.
        //This method is run when selecting an item in the listbox, so it is basically gathering
        //data for editing in case the user chooses to edit the selected item in the listbox.
        public void AppointmentDataToBeEdited(int _apmtId)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates an SqlCommand, cmdApmt, which SELECTs everything (*) from VK_Appointments 
                SqlCommand cmdApmt = new SqlCommand("SELECT * FROM VK_Appointments WHERE PK_ApmtID = @ApmtId", con);


                //Sets @ApmtId to the value of the method parameter _apmtId
                cmdApmt.Parameters.AddWithValue("@ApmtId", _apmtId);

                //Set up a data reader called dr, which reads the data from cmdApmt
                using (SqlDataReader dr = cmdApmt.ExecuteReader())
                {
                    //While-loop running while dr is reading 
                    while (dr.Read())
                    {
                        //Sets the properties of CurrentAppointment to the data SELECT'ed from the database. 
                        CurrentAppointment.ApmtId = Convert.ToInt32(dr["PK_ApmtID"]);
                        CurrentAppointment.ApmtDate = (DateTime)dr["ApmtDate"];
                        CurrentAppointment.FK_LesId = Convert.ToInt32(dr["FK_LesId"]);
                        CurrentAppointment.FK_InstId = Convert.ToInt32(dr["FK_InstId"]);
                        CurrentAppointment.FK_ClassName = dr["FK_ClassName"].ToString();
                    }
                }
            }
        }

        //Edits the data of an existing Appointment handed to the method via the parameters.
        //When the edit-button is pressed, the edit bool is set to true, and the ComboBox input fields are set
        //to be equal to the data from the selected item in the AppointmentListBox. When the item in
        //the AppointmentListBox was selected the AppointmentDataToBeEdited was run and the CurrentAppointment
        //was set with the data from the existing appointment. When pressing the save-button while edit bool is true,
        //EditAppointment is run with CurrentAppointment, CurrentInstructor, CurrentLesson, CurrentClass and CurrentStudent
        //as arguments. When EditAppointment is run the method checks whether a null-argument was passed to it, ie. whether
        //the user have made a ComboBox-selection or not. If the user have made a ComboBox-selection in a ComboBox,
        //CurrentInstructor, CurrentLesson, CurrentClass or CurrentStudent will be holding an argument, and the
        //specific value therefrom will be passed to the database. //In case the user haven't made a ComboBox-selection,
        //the corresponding value from CurrentAppointment (the already registered value) will be passed = the entity will remain 
        //the same.
        public void EditAppointment(Appointment appointmentToBeEdited, Instructor instructorToBeEdited, Lesson lessonToBeEdited, Class classToBeEdited, Student studentToBeEdited)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates the SqlCommand cmdApmt which UPDATEs the attributes of a specific row in VK_Appointments, based on the PK_ApmtID = @ApmtId
                SqlCommand cmdApmt = new SqlCommand("UPDATE VK_Appointments SET ApmtDate = @ApmtDate, FK_InstID = @InstId, FK_LesID = @LesId, FK_ClassName = @ClassName " +
                                                "WHERE PK_ApmtID = @ApmtId", con);

                //Gives @ApmtDate the value of ApmtDate
                cmdApmt.Parameters.AddWithValue("@ApmtDate", appointmentToBeEdited.ApmtDate);

                //Gives @ApmtId the value of ApmtId
                cmdApmt.Parameters.AddWithValue("@ApmtId", appointmentToBeEdited.ApmtId);

                //If the user have made a ComboBox-selection in the instructor-ComboBox, instructorToBeEdited
                //will be holding an argument (!= null), and the specific value therefrom will be passed to the database.
                if (instructorToBeEdited != null)
                {
                    cmdApmt.Parameters.AddWithValue("@InstId", instructorToBeEdited.InstId);
                }
                //In case the user haven't made a ComboBox-selection (== null), the corresponding value from
                //CurrentAppointment (the already registered value) will be passed, and the entity will remain the same.
                else
                {
                    cmdApmt.Parameters.AddWithValue("@InstId", appointmentToBeEdited.FK_InstId);
                }

                //If the user have made a ComboBox-selection in the lesson-ComboBox, lessonToBeEdited
                //will be holding an argument (!= null), and the specific value therefrom will be passed to the database.
                if (lessonToBeEdited != null)
                {
                    cmdApmt.Parameters.AddWithValue("@LesId", lessonToBeEdited.LesId);
                }
                //In case the user haven't made a ComboBox-selection (== null), the corresponding value from
                //CurrentAppointment (the already registered value) will be passed, and the entity will remain the same.
                else
                {
                    cmdApmt.Parameters.AddWithValue("@InstId", appointmentToBeEdited.FK_LesId); 
                }

                //If the user have made a ComboBox-selection in the class-ComboBox, classToBeEdited
                //will be holding an argument (!= null), and the specific value therefrom will be passed to the database.
                if (classToBeEdited != null)
                {
                    cmdApmt.Parameters.AddWithValue("@ClassName", classToBeEdited.ClassName);
                }
                //In case the user haven't made a ComboBox-selection (== null), the corresponding value from
                //CurrentAppointment (the already registered value) will be passed, and the entity will remain the same.
                else
                {
                    cmdApmt.Parameters.AddWithValue("@ClassName", appointmentToBeEdited.FK_ClassName);
                }

                //Tells the database to execute the sqlcommand cmdApmt.
                cmdApmt.ExecuteNonQuery();


                //Creates the SqlCommand cmdStu which UPDATEs the attributes of a specific row in
                //VK_Student_Appointment, based on the PK_ApmtID = @ApmtId
                SqlCommand cmdStu = new SqlCommand("UPDATE VK_Student_Appointment SET CK_StuCPR = @StuCPR " +
                                                   "WHERE CK_ApmtID = @ApmtId", con);

                //Gives @StuCPR the value of StuCPR
                cmdStu.Parameters.AddWithValue("@StuCPR", studentToBeEdited.StuCPR);

                //Gives @ApmtId the value of ApmtId
                cmdStu.Parameters.AddWithValue("@ApmtId", appointmentToBeEdited.ApmtId);

                //Tells the database to execute the SqlCommand cmdStu
                cmdStu.ExecuteNonQuery();
            }
        }

        //The DeleteAppointment method is defined with the parameter appointmentIdToBeDeleted.
        //The method takes the CurrentAppointment.ListBoxApmtId as argument when called.
        public void DeleteAppointment(int appointmentIdToBeDeleted)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates an SqlCommand cmd which DELETEs the attributes of a specific row in
                //VK_Appointments, based on the PK_ApmtID = @ApmtId
                SqlCommand cmd = new SqlCommand("DELETE FROM VK_Appointments WHERE PK_ApmtID = @ApmtId", con);

                //Gives @ApmtId the value of appointmentIdToBeDeleted
                cmd.Parameters.AddWithValue("@ApmtId", appointmentIdToBeDeleted);

                //Tells the database to execute the SqlCommand cmd
                cmd.ExecuteScalar(); 
            }

            //The Input-fields are cleared to indicate that deletion have been executed.
            ClearInputFields(); 
        }
        #endregion

        #region Quality of life funktions

        //Clears the input fields by setting the SelectedItem of the ComboBoxes
        //to null and the Value of the DateTimePicker to DateTime.Now
        private void ClearInputFields()
        {
            Apmt_PickLesson_ComboBox.SelectedItem = null;
            Apmt_PickClass_ComboBox.SelectedItem = null;
            Apmt_PickStudent_ComboBox.SelectedItem = null;
            Apmt_PickInstructor_ComboBox.SelectedItem = null;
            Apmt_PickDateTime_DateTimePicker.Value = DateTime.Now;
        }

        //Locks the ComboBoxes and the DateTimePicker
        private void LockInputFields()
        {
            Apmt_PickLesson_ComboBox.IsEnabled = false;
            Apmt_PickClass_ComboBox.IsEnabled = false;
            Apmt_PickStudent_ComboBox.IsEnabled = false;
            Apmt_PickInstructor_ComboBox.IsEnabled = false;
            Apmt_PickDateTime_DateTimePicker.IsEnabled = false;
        }

        //Unlocks the ComboBoxes and the DateTimePicker
        private void UnlockInputFields()
        {
            Apmt_PickLesson_ComboBox.IsEnabled = true;
            Apmt_PickClass_ComboBox.IsEnabled = true;
            Apmt_PickStudent_ComboBox.IsEnabled = true;
            Apmt_PickInstructor_ComboBox.IsEnabled = true;
            Apmt_PickDateTime_DateTimePicker.IsEnabled = true;
        }
        #endregion
    }
}
