using Microsoft.Data.SqlClient;
using System.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for Class_Sub_ShowClass_Page.xaml
    /// </summary>

    /// Redo the comments (/add new), they are a mismatch of (unedited) copied and newly written comments
    public partial class Class_Sub_ShowClass_Page : Page
    {
        //The Class_Sub_ShowClass_Page constructor
        public Class_Sub_ShowClass_Page(string cn) //Passed the classname from ClassPage on entry
        {
            InitializeComponent();
            currentClassName = cn; //Sets currentClassName to cn - which is the ClassName of the selected class in ClassPage passed on entry
            Class_Sub_Title_TextBlock.Text = currentClassName; //Sets the TextBlock at the top of the page to reflect the current class
            ListBoxFunction(); //Runs ListBoxFunction when the page is opened
            ComboBoxStartUp(); //Runs ComboBoxStartUp with all of the set up for the ComboBoxes to minimize clutter
            Class_Sub_Save_Button.IsEnabled = false; //Makes sure that the 'Gem' button is not enabled when the page is opened - There were some issues with it staying enabled if it was open and the user left the page
        }

        #region Variables and ConStuClass
        ConStuClass conToBeRetrieved; //Used to store the data retrieved when calling either RetrieveConnection or RetrieveStudent both of which stores the data in the class ConStuClass
        Class classToBeRetrieved; //Used to store the data retrieved when calling RetrieveClass - Is its own class because the class properties has not been added to ConStuClass

        ConStuClass currentCon = new ConStuClass(); //Instantiation of ConStuClass
        Class currentClass = new Class(); //Instantiation of Class

        string currentClassName; //Stores the ClassName of the current class of the page - Used by many methods, when only the name of the current class is needed
        string currentConStuID; //Keeps track of which student has been chosen - Used by DeleteConnection

        //A class used by the page storing data from the tables VK_Students and VK_Class_Student, as well as the extra attribute SetUp
        public class ConStuClass
        {
            //Attributes from the tables VK_Students and VK_Class_Student
            public string CK_ClassName { get; set; }
            public string CK_StuCPR { get; set; }
            public string StuFirstName { get; set; }
            public string StuLastName { get; set; }
            public string StuPhone { get; set; }
            public string StuEmail { get; set; }

            //Extra attribute used for the GUI
            public string SetUp { get; set; }
            
            //Constructor for ConStuClass
            public ConStuClass(string _cK_ClassName, string _cK_StuCPR, string _stuFirstName, string _stuLastName, string _stuPhone, string _stuEmail, string _setUp)
            {
                CK_ClassName = _cK_ClassName;
                CK_StuCPR = _cK_StuCPR;
                StuFirstName = _stuFirstName;
                StuLastName = _stuLastName;
                StuPhone = _stuPhone;
                StuEmail = _stuEmail;
                SetUp = _setUp;
            }

            //Acts as a parameterless default constructor
            public ConStuClass() : this("", "", "", "", "", "", "") { }
        }
        #endregion

        #region Buttons - Handles all the Click Events of the buttons
        //Allows the user to edit the data of the current class - Enables the ComboBoxes in the upper right corner
        private void Class_Sub_Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            //Set IsEnabled to true on the ComboBoxes
            Class_Sub_Year_ComboBox.IsEnabled = true;
            Class_Sub_Quarter_ComboBox.IsEnabled = true;
            Class_Sub_Type_ComboBox.IsEnabled = true;

            //Set IsEnabled on the ClassNumber TextBox to false, as a safety measure because the user should never manually set it
            Class_Sub_ClassNumber_TextBox.IsEnabled = false;
        }

        //Allows the user to save the editted data of the current class
        private void Class_Sub_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            //Sets the currentClass attributes to data from the ComboBoxes 
            currentClass.ClassYear = Class_Sub_Year_ComboBox.Text;
            currentClass.ClassQuarter = (Quarter)Enum.Parse(typeof(Quarter), Class_Sub_Quarter_ComboBox.Text);
            currentClass.ClassLicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), Class_Sub_Type_ComboBox.Text);
            
            //Runs the method to UPDATE the data of the current class with the data of currentClass
            UpdateClass(currentClass);

            //Updates the text of Class_Sub_Title_TextBlock and Class_Sub_ClassNumber_TextBox to reflect the updated data
            Class_Sub_Title_TextBlock.Text = currentClassName;
            Class_Sub_ClassNumber_TextBox.Text = currentClass.ClassNumber;

            //Sets IsEnabled of all the class ComboBoxes and the save button to false
            Class_Sub_Year_ComboBox.IsEnabled = false;
            Class_Sub_Quarter_ComboBox.IsEnabled = false;
            Class_Sub_Type_ComboBox.IsEnabled = false;
            Class_Sub_Save_Button.IsEnabled = false;
        }

        //Allows the user to delete the current class
        private void Class_Sub_DelClass_Button_Click(object sender, RoutedEventArgs e)
        {
            //Sets up a MessageBox in which the user confirms that they wish to delete the current class

            //Save two strings message and caption
            string messageBoxText = $"Du er ved at slette {currentClassName}.\nEr du sikker på at du gerne vil slette {currentClassName}?";
            string caption = "ADVARSEL";
            //Saves other MessageBox data
            MessageBoxButton button = MessageBoxButton.OKCancel;
            MessageBoxImage icon = MessageBoxImage.Warning;
            //Create a variable to save the resultat of the message box
            MessageBoxResult result;

            //Runs the MessageBox with the previously established variables and saves the result
            result = MessageBox.Show(messageBoxText, caption, button, icon);

            //If the MessageBoxResult is equal to OK
            if (result == MessageBoxResult.OK)
            {
                //Delete the class
                DeleteClass(currentClassName);
                //Go back to the ClassPage
                this.NavigationService.GoBack();
            }
        }

        //Allows the user to delete the connection between Class and Student
        private void Class_Sub_DelCon_Button_Click(object sender, RoutedEventArgs e)
        {
            //Checks that a student is selected 
            if (Class_Sub_ShowClass_DisStu_ListBox.SelectedItem != null)
            {
                //DELETE the connection
                DeleteConnection(currentConStuID);
            }
            //Rerun ListBoxFunction to make sure it reflect the current situation
            ListBoxFunction();
        }
        #endregion

        #region ListBox
        //Controls what happens when you select an item in the ListBox
        private void Class_Sub_ShowClass_DisStu_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Safety check, to make sure that the selected item exists
            if (Class_Sub_ShowClass_DisStu_ListBox.SelectedItem != null)
            {
                //Sets currentConStuID equal to the CK_StuCPR of selected item - Used by Class_Sub_DelCon_Button_Click
                currentConStuID = (Class_Sub_ShowClass_DisStu_ListBox.SelectedItem as ConStuClass).CK_StuCPR;
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
                SqlCommand count = new SqlCommand("SELECT COUNT(CK_StuCPR) from VK_Class_Student WHERE CK_ClassName = @CK_ClassName", con);
                count.Parameters.AddWithValue("@CK_ClassName", currentClassName);
                //Saves count command result to int
                int intCount = (int)count.ExecuteScalar();

                //Make a list of the class ConStuClass called stu (Name doesn't matter)
                List<ConStuClass> stu = new List<ConStuClass>();
                
                //Forloop which adds intCount number of new stu to stu-list
                for (int i = 0; i < intCount; i++)
                {
                    //Calls RetrieveConnection sending i 
                    RetrieveConnection(i);

                    //Adds a new item with specific attributes to the list
                    stu.Add(conToBeRetrieved);
                   
                    //Only necessary for multi-attribute ListBoxItem
                    //Set up the attribute 'SetUp' which we use to determine the appearance of the ListBoxItem 
                    stu[i].SetUp = $"{stu[i].StuFirstName} {stu[i].StuLastName}\n{stu[i].StuPhone}\n{stu[i].StuEmail}";
                }

                //Set the ItemsSource to the list, so that the ListBox uses the list to make the ListBoxItems
                Class_Sub_ShowClass_DisStu_ListBox.ItemsSource = stu;

                //Sets number of students in TextBox over in ClassData corner 
                Class_Sub_NumberOfStudents_TextBox.Text = intCount.ToString(); 
            }
        }
        #endregion

        #region ComboBox
        //Run by the Class_Sub_ShowClass_Page constructor to minimize the clutter in the constructor
        private void ComboBoxStartUp()
        {
            //Calls the SetUp methods for the ComboBoxes
            ComboBoxAddStuSetUp();
            ComboBoxYearSetUp();
            ComboBoxQuarterSetUp();
            ComboBoxTypeSetUp();

            //Retrives the Class data from the database using the currentClassName, saving the data as classToBeRetrived
            RetrieveClass(currentClassName);

            //Sets the ComboBoxes and the TextBox to the data from classToBeRetrived
            Class_Sub_Year_ComboBox.Text = classToBeRetrieved.ClassYear;
            Class_Sub_Quarter_ComboBox.Text = classToBeRetrieved.ClassQuarter.ToString();
            Class_Sub_ClassNumber_TextBox.Text = classToBeRetrieved.ClassNumber;
            Class_Sub_Type_ComboBox.Text = classToBeRetrieved.ClassLicenseType.ToString();
        }

        //Event for when a student is chosen in AddStu ComboBox 
        private void Class_Sub_AddStu_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //The if/else-statement is used (together with a MessageBox)
            //to stop the program from running the event twice

            //Declare the comboBox variable and set it to the sender of the type ComboBox
            ComboBox comboBox = (ComboBox)sender;
            //Checks if comboBox is not open, then it should simply return
            if (!comboBox.IsDropDownOpen)
                return;
            //Runs when the DropDown of the ComboBox is open
            else
            {
                //Retrives the data of the chosen student
                RetrieveStudent(Class_Sub_AddStu_ComboBox.SelectedIndex);
                
                //Sets currentCon to the correct data
                currentCon.CK_StuCPR = conToBeRetrieved.CK_StuCPR.ToString();
                currentCon.CK_ClassName = currentClassName;
                
                //Creates a new connection using currentCon
                CreateConnection(currentCon);
                
                //Rerun ListBoxFunction to update it with the new connection
                ListBoxFunction();
                
                //Makes sure there is no SelectedItem in the ListBox
                Class_Sub_AddStu_ComboBox.SelectedItem = null;
            }
        }

        //When anything is changed in the ComboBoxes Year, Quarter or Type, enable the Save button
        private void Class_Sub_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Class_Sub_Save_Button.IsEnabled = true;
        }

        #region ComboBox SetUp Method
        //Set up the add student ComboBox
        private void ComboBoxAddStuSetUp()
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

                //Make a list of the class ConStuClass
                List<ConStuClass> types = new List<ConStuClass>();

                //Forloop which runs intCount amount of times
                for (int i = 0; i < intCount; i++)
                {
                    //Retrives the data from VK_Students 
                    RetrieveStudent(i);

                    //Adds a new item to the list (Could specify which attributes)
                    types.Add(conToBeRetrieved);

                    //Sets SetUp to what should be shown in the ComboBox
                    types[i].SetUp = $"{types[i].StuFirstName} {types[i].StuLastName}";
                }
                //Sets the ItemsSource of the ComboBox to the list
                Class_Sub_AddStu_ComboBox.ItemsSource = types;
                
                //Tells the ComboBox which attribute should be displayed
                Class_Sub_AddStu_ComboBox.DisplayMemberPath = "SetUp";
            }
        }

        //Set up the year ComboBox
        private void ComboBoxYearSetUp()
        {
            //Make a list of the class Class
            List<Class> years = new List<Class>();

            //Forloop which runs from 24 to 40
            for (int i = 24; i <= 40; i++)
            {
                years.Add(new Class { ClassYear = $"{i}" });
            }
            //Sets the ItemsSource and which attribute should be displayed
            Class_Sub_Year_ComboBox.ItemsSource = years;
            Class_Sub_Year_ComboBox.DisplayMemberPath = "ClassYear";
        }

        //Set up the quarter ComboBox
        private void ComboBoxQuarterSetUp()
        {
            //Make a list of the class Class
            List<Class> quarters = new List<Class>();

            //Adds four items to the list reflecting the ClassQuarter enum
            quarters.Add(new Class { ClassQuarter = Quarter.F });
            quarters.Add(new Class { ClassQuarter = Quarter.S });
            quarters.Add(new Class { ClassQuarter = Quarter.E });
            quarters.Add(new Class { ClassQuarter = Quarter.V });

            //Sets the ItemsSource and which attribute should be displayed
            Class_Sub_Quarter_ComboBox.ItemsSource = quarters;
            Class_Sub_Quarter_ComboBox.DisplayMemberPath = "ClassQuarter";
        }

        //Set up the types ComboBox
        private void ComboBoxTypeSetUp()
        {
            //Make a list of the class Class
            List<Class> types = new List<Class>();

            //Adds four items to the list reflecting the LicenseType enum
            types.Add(new Class { ClassLicenseType = LicenseType.B });
            types.Add(new Class { ClassLicenseType = LicenseType.A1 });
            types.Add(new Class { ClassLicenseType = LicenseType.A2 });
            types.Add(new Class { ClassLicenseType = LicenseType.A });

            //Sets the ItemsSource and which attribute should be displayed
            Class_Sub_Type_ComboBox.ItemsSource = types;
            Class_Sub_Type_ComboBox.DisplayMemberPath = "ClassLicenseType";
        }
        #endregion
        #endregion

        #region Database Methods
        #region Connection
        //Creates a connect between students and class
        public void CreateConnection(ConStuClass conToBeCreated)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //MessageBox used to stop the program from running the method twice - Shouldn't be necessary, but it is. Have tried many things
                MessageBox.Show("Works if there is a MessageBox somewhere in CreateConnection, " +
                    "\notherwise the program will run CreateConnection twice. " +
                    "\n\nHave tried 'IsLoaded' - Didn't work. " +
                    "\n\nHave tried making CreateConnection async and using an await timer - Didn't work. " +
                    "\n\nBut a MessageBox fixes it, so a MessageBox there shall be.");

                //Opens said connection
                con.Open();
                
                //Create the cmd command which checks if the connection already exist and if it doesn't; INSERT a new one with the given data
                SqlCommand cmd = new SqlCommand("IF NOT EXISTS (SELECT * FROM VK_Class_Student WHERE CK_ClassName = @CK_ClassName AND CK_StuCPR = @CK_StuCPR) " +
                                                "BEGIN INSERT INTO VK_Class_Student (CK_ClassName, CK_StuCPR) " +
                                                "VALUES(@CK_ClassName, @CK_StuCPR) END " +
                                                "SELECT @@IDENTITY", con);

                //Add corresponding attribute to the database through the use of cmd
                cmd.Parameters.AddWithValue("@CK_ClassName", conToBeCreated.CK_ClassName);
                cmd.Parameters.AddWithValue("@CK_StuCPR", conToBeCreated.CK_StuCPR);
                
                //Tells the database to execute the sql commands
                cmd.ExecuteScalar();
            }
        }

        //Retrieves a specific row of Connection in the database where the row number is equal to dbRowNum + 1
        public void RetrieveConnection(int dbRowNum)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates two SqlCommand, which SELECTs a specific row in VK_Students and VK_Class_Student
                //VK_Class_Student by row number and VK_Students by StuCPR
                SqlCommand cmd = new SqlCommand("SELECT CK_ClassName, CK_StuCPR FROM VK_Class_Student ORDER BY CK_ClassName ASC OFFSET @dbRowNum ROWS FETCH NEXT 1 ROW ONLY", con);
                SqlCommand stu = new SqlCommand("SELECT StuFirstName, StuLastName, StuPhone, StuEmail FROM VK_Students WHERE PK_StuCPR = @CK_StuCPR", con); 

                //Set dbRowNum to 0 if under 0
                if (dbRowNum < 0) { dbRowNum = 0; }

                //Gives @dbRowNum the value of dbRowNum
                cmd.Parameters.AddWithValue("@dbRowNum", dbRowNum);

                //Set up a data reader called dr, which reads the data specified by cmd
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    //While-loop running while dr is reading 
                    while (dr.Read())
                    {
                        //Assigns value to @CK_StuCPR from stu based on a value from cmd
                        stu.Parameters.AddWithValue("@CK_StuCPR", dr["CK_StuCPR"].ToString());

                        //Initialization of conToBeRetrived
                        conToBeRetrieved = new ConStuClass("", "", "", "", "", "", "")
                        {
                            //Sets the attributes of conToBeRetrieved equal to the data from the current row of the database
                            CK_ClassName = dr["CK_ClassName"].ToString(),
                            CK_StuCPR = dr["CK_StuCPR"].ToString(),
                        };
                    }
                }
                //Set up a second data reader for stu
                using (SqlDataReader dr2 = stu.ExecuteReader())
                {
                    //While-loop running while dr2 is reading 
                    while (dr2.Read())
                    {
                        //Sets more of the attributes of the current conToBeRetrieved
                        conToBeRetrieved.StuFirstName = dr2["StuFirstName"].ToString();
                        conToBeRetrieved.StuLastName = dr2["StuLastName"].ToString();
                        conToBeRetrieved.StuPhone = dr2["StuPhone"].ToString();
                        conToBeRetrieved.StuEmail = dr2["StuEmail"].ToString();
                    }
                } 
            }
        }

        //Deletes the selected connection from the database
        public void DeleteConnection(string conToBeDeleted)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates a cmd SqlCommand, which DELETEs a specific row in the table, based on the CK_StuCPR
                SqlCommand cmd = new SqlCommand("DELETE FROM VK_Class_Student WHERE CK_StuCPR = @CK_StuCPR", con);

                //Gives @CK_StuCPR the value of conToBeDeleted
                cmd.Parameters.AddWithValue("@CK_StuCPR", conToBeDeleted);

                //Tells the database to execute the cmd sql command
                cmd.ExecuteScalar();
            }
        }
        #endregion
        #region Class
        //Retrieves a specific row of Class in the database where the row number is equal to dbRowNum + 1
        public void RetrieveClass(string className)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates a SqlCommand, which SELECTs a specific row in VK_Classes based on PK_ClassName
                SqlCommand cmd = new SqlCommand("SELECT PK_ClassName, ClassYear, ClassNumber, ClassQuarter, ClassLicenseType FROM VK_Classes WHERE PK_ClassName = @className", con);

                //Gives @className the value of className
                cmd.Parameters.AddWithValue("@className", className);

                //Set up a data reader called dr, which reads the data specified by cmd
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    //While-loop running while dr is reading 
                    while (dr.Read())
                    {
                        //Initialization of classToBeRetrieved
                        classToBeRetrieved = new Class(default, "", "", default, "")
                        {
                            //Sets the attributes of classToBeRetrieved equal to the data from the current row of the database, enum-parsing two of them 
                            ClassQuarter = (Quarter)Enum.Parse(typeof(Quarter), dr["ClassQuarter"].ToString()),
                            ClassYear = dr["ClassYear"].ToString(),
                            ClassNumber = dr["ClassNumber"].ToString(),
                            ClassLicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), dr["ClassLicenseType"].ToString()),
                            ClassName = dr["PK_ClassName"].ToString(),
                        };
                    }
                }
            }
        }

        //Edits the data of the current Class
        public void UpdateClass(Class classToBeUpdated)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates a SqlCommand, which UPDATEs the attributes of a specific row in the table, based on the PK_ClassName
                SqlCommand cmd = new SqlCommand("UPDATE VK_Classes SET ClassYear = @ClassYear, ClassNumber = @ClassNumber, ClassQuarter = @ClassQuarter, ClassLicenseType = @ClassLicenseType, PK_ClassName = @NewClassName WHERE PK_ClassName = @PK_ClassName", con);

                //Creates a SqlCommand, which COUNTs how many Classes exists with the same year and quarter
                SqlCommand count = new SqlCommand("SELECT COUNT(ClassQuarter) FROM VK_Classes WHERE ClassQuarter = @ClassQuarter AND ClassYear = @ClassYear", con);
                
                //Sets the attributes
                count.Parameters.Add("@ClassQuarter", SqlDbType.NVarChar).Value = classToBeUpdated.ClassQuarter;
                count.Parameters.Add("@ClassYear", SqlDbType.NVarChar).Value = classToBeUpdated.ClassYear;
                
                //
                int intCount = (int)count.ExecuteScalar();
                classToBeUpdated.ClassNumber = (intCount + 1).ToString();

                if(intCount != 0)
                {
                    SqlCommand c = new SqlCommand("SELECT MAX(CAST(ClassNumber AS Int)) FROM VK_Classes WHERE ClassQuarter = @ClassQuarter AND ClassYear = @ClassYear AND IsNumeric(ClassNumber) = 1", con);
                    c.Parameters.Add("@ClassQuarter", SqlDbType.NVarChar).Value = classToBeUpdated.ClassQuarter;
                    c.Parameters.Add("@ClassYear", SqlDbType.NVarChar).Value = classToBeUpdated.ClassYear;
                    int cCount = (int)c.ExecuteScalar();
                    if ((intCount + 1) <= cCount) { classToBeUpdated.ClassNumber = (cCount + 1).ToString(); }
                }
                
                string newName = $"{currentClass.ClassQuarter}{currentClass.ClassYear}-{classToBeUpdated.ClassNumber}";
                
                //Gives @attribute the value of attribute
                cmd.Parameters.AddWithValue("@PK_ClassName", currentClassName);
                cmd.Parameters.AddWithValue("@NewClassName", newName);
                cmd.Parameters.AddWithValue("@ClassYear", classToBeUpdated.ClassYear);
                cmd.Parameters.AddWithValue("@ClassNumber", classToBeUpdated.ClassNumber);
                cmd.Parameters.AddWithValue("@ClassQuarter", classToBeUpdated.ClassQuarter);
                cmd.Parameters.AddWithValue("@ClassLicenseType", classToBeUpdated.ClassLicenseType);

                //Tells the database to execute the cmd sql command
                cmd.ExecuteNonQuery();
                currentClassName = newName;
            }
        }

        //Deletes the selected connection from the database
        public void DeleteClass(string classToBeDeleted)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates a cmd SqlCommand, which DELETEs a specific row in the table, based on the CK_ClassName
                SqlCommand cmd = new SqlCommand("DELETE FROM VK_Classes WHERE PK_ClassName = @PK_ClassName", con);

                //Gives @PK_LesId the value of conToBeDeleted
                cmd.Parameters.AddWithValue("@PK_ClassName", classToBeDeleted);

                //Tells the database to execute the cmd sql command
                cmd.ExecuteScalar();
            }
        }
        #endregion
        #region Students
        public void RetrieveStudent(int dbRowNum)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();
                //Creates a cmd SqlCommand, which SELECTs a specific row 
                SqlCommand stu = new SqlCommand("SELECT PK_StuCPR, StuFirstName, StuLastName, StuPhone, StuEmail FROM VK_Students ORDER BY StuFirstName ASC OFFSET @dbRowNum ROWS FETCH NEXT 1 ROW ONLY", con);


                //Set dbRowNum to 0 if under 0
                if (dbRowNum < 0)
                {
                    dbRowNum = 0;
                }
                //Gives @dbRowNum the value of dbRowNum
                stu.Parameters.AddWithValue("@dbRowNum", dbRowNum);

                //Set up a data reader called dr, which reads the data from cmd (the previous sql command)
               
                using (SqlDataReader dr = stu.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        //Sets conToBeRetrieve a new empty ClassStuConnection, which is then filled
                        conToBeRetrieved = new ConStuClass("", "", "", "", "", "", "")
                        {
                            //Sets the attributes of conToBeRetrieved equal to the data from the current row of the database
                            
                            CK_StuCPR = dr["PK_StuCPR"].ToString(),
                            StuFirstName = dr["StuFirstName"].ToString(),
                            StuLastName = dr["StuLastName"].ToString(),
                            StuPhone = dr["StuPhone"].ToString(),
                            StuEmail = dr["StuEmail"].ToString(),
                        };
                    }
                }
            }
        }
        #endregion
        #endregion

    }
}
