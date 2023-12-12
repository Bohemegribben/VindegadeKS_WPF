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
        public Class_Sub_ShowClass_Page(string cn)
        {
            currentClassName = cn;
            InitializeComponent();
            Class_Sub_Title_TextBlock.Text = currentClassName;
            ListBoxFunction();
            AddStuComboBoxFunction();
            ComboBoxFunctionYear();
            ComboBoxFunctionQuarters();
            ComboBoxFunctionLicenseTypes();
            ClassComboBoxSetUp();
        }

        ConStuClass conToBeRetrieved; /// Can conToBeRetrieved and stuToBeRetrieved be merged
        ConStuClass stuToBeRetrieved;
        Class classToBeRetrieved; /// This as well

        Class currentClass = new Class(); /// Is Class needed or is ConStu fine? (Combine?)
        ConStuClass currentStu = new ConStuClass();

        string currentClassName; ///Send something when opening page

        string currentConStuID;/// Are two ID strings needed?
        string currentConClassID;

        string newName; /// Does this need to be accessable outside of it's method?

        #region Hold Buttons
        private void Class_Sub_Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            Class_Sub_Year_ComboBox.IsEnabled = true;
            Class_Sub_Quarter_ComboBox.IsEnabled = true;
            Class_Sub_ClassNumber_TextBox.IsEnabled = true; ///Change to auto (Done in ClassPage) - Disable if true
            Class_Sub_Type_ComboBox.IsEnabled = true;
        }

        private void Class_Sub_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            RetrieveClassData(currentClassName); ///Can the Retrieve(/other database) methods (if only used once) be moved into here?

            currentClass.ClassYear = Class_Sub_Year_ComboBox.Text;
            currentClass.ClassQuarter = (Quarter)Enum.Parse(typeof(Quarter), Class_Sub_Quarter_ComboBox.Text);
            currentClass.ClassLicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), Class_Sub_Type_ComboBox.Text);
            currentClass.ClassNumber = Class_Sub_ClassNumber_TextBox.Text;
      
            UpdateClass(currentClass);

            
            Class_Sub_Title_TextBlock.Text = currentClassName;
            Class_Sub_ClassNumber_TextBox.Text = currentClass.ClassNumber;

            Class_Sub_Year_ComboBox.IsEnabled = false;
            Class_Sub_Quarter_ComboBox.IsEnabled = false;
            Class_Sub_ClassNumber_TextBox.IsEnabled = false;
            Class_Sub_Type_ComboBox.IsEnabled = false;
        }
        #endregion

        #region ListBox
        //Controls what happens when you select an item in the ListBox
        private void Class_Sub_ShowClass_DisStu_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Safety check, to make sure that the selected item exists
            if (Class_Sub_ShowClass_DisStu_ListBox.SelectedItem != null)
            {
                //Sets currentConStuID to equal the ID of selected item
                currentConStuID = (Class_Sub_ShowClass_DisStu_ListBox.SelectedItem as ConStuClass).CK_StuCPR; ///What are the IDs used for? And are both used?
                //currentConClassID = (Class_Sub_ShowClass_DisStu_ListBox.SelectedItem as ConStuClass).CK_ClassName;
            }
        }

        //Method to create, control and add stu to the ListBox
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

                //Make a list with the Item Class from below called stu (Name doesn't matter)
                //LesListBoxItems in my case
                List<ConStuClass> stu = new List<ConStuClass>();
                
                //Forloop which adds intCount number of new stu to stu-list
                for (int i = 0; i < intCount; i++)
                {
                    
                    //Calls RetrieveLessonData method, sending i as index
                    RetrieveConnection(i);

                    //Adds a new item from the item class with specific attributes to the list
                    //The data added comes from RetrieveLessonData
                    stu.Add(new ConStuClass() { CK_StuCPR = conToBeRetrieved.CK_StuCPR, CK_ClassName = conToBeRetrieved.CK_ClassName, 
                                              StuFirstName = conToBeRetrieved.StuFirstName, StuLastName = conToBeRetrieved.StuLastName, 
                                              StuPhone = conToBeRetrieved.StuPhone, StuEmail = conToBeRetrieved.StuEmail });

                    //Only necessary for multi-attribute ListBoxItem
                    //Set up the attribute 'SetUp' which is used to determine the appearance of the ListBoxItem 
                    //Mine isn't, so it's out commented
                    stu[i].SetUp = $"{stu[i].StuFirstName} {stu[i].StuLastName}\n{stu[i].StuPhone}\n{stu[i].StuEmail}";
                }

                //Set the ItemsSource to the list, so that the ListBox uses the list to make the ListBoxItems
                Class_Sub_ShowClass_DisStu_ListBox.ItemsSource = stu;
                Class_Sub_NumberOfStudents_TextBox.Text = intCount.ToString();
            }
        }
        public class ConStuClass 
        {
            public string CK_ClassName { get; set; }
            public string CK_StuCPR { get; set; }
            public string StuFirstName { get; set; }
            public string StuLastName { get; set; }
            public string StuPhone { get; set; }
            public string StuEmail { get; set; }
            public string SetUp { get; set; }
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
            public ConStuClass() : this("", "", "", "", "", "", "")
            { }
        }
        private void Class_Sub_DelStu_Button_Click(object sender, RoutedEventArgs e)
        {
            if(Class_Sub_ShowClass_DisStu_ListBox.SelectedItem != null)
            {
                DeleteConnection(currentConStuID);
            }
            ListBoxFunction();
        }
        #endregion

        #region ComboBox
        private void ClassComboBoxSetUp()
        {
            RetrieveClassData(currentClassName);
            Class_Sub_Year_ComboBox.Text = classToBeRetrieved.ClassYear;
            Class_Sub_Quarter_ComboBox.Text = classToBeRetrieved.ClassQuarter.ToString();
            Class_Sub_ClassNumber_TextBox.Text = classToBeRetrieved.ClassNumber;
            Class_Sub_Type_ComboBox.Text = classToBeRetrieved.ClassLicenseType.ToString();
        }

        private void Class_Sub_AddStu_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RetrieveStudent(Class_Sub_AddStu_ComboBox.SelectedIndex);
            currentStu.CK_StuCPR = stuToBeRetrieved.CK_StuCPR.ToString();
            currentStu.CK_ClassName = currentClassName;
            if(DoesConExist(currentStu) == false)
                CreateConnection(currentStu);
            ListBoxFunction();
            Class_Sub_AddStu_ComboBox.SelectedItem = null;
        }
        private void AddStuComboBoxFunction()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();

                //Make students instead
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_StuCPR) from VK_Students", con);
                int intCount = (int)count.ExecuteScalar();

                List<ConStuClass> types = new List<ConStuClass>();

                for (int i = 0; i < intCount; i++)
                {
                    RetrieveStudent(i);

                    types.Add(new ConStuClass { CK_ClassName = stuToBeRetrieved.CK_ClassName, CK_StuCPR = stuToBeRetrieved.CK_StuCPR, StuFirstName = stuToBeRetrieved.StuFirstName, StuLastName = stuToBeRetrieved.StuLastName, });

                    types[i].SetUp = $"{types[i].StuFirstName} {types[i].StuLastName}";
                }

                Class_Sub_AddStu_ComboBox.DisplayMemberPath = "SetUp";

                Class_Sub_AddStu_ComboBox.ItemsSource = types;
            }
        }
        private void ComboBoxFunctionYear()
        {
            List<Class> years = new List<Class>();

            for (int i = 24; i <= 40; i++)
            {
                years.Add(new Class { ClassYear = $"{i}" });
            }
            Class_Sub_Year_ComboBox.ItemsSource = years;
            Class_Sub_Year_ComboBox.DisplayMemberPath = "ClassYear";
        }
        private void ComboBoxFunctionQuarters()
        {

            List<Class> quarters = new List<Class>();

            quarters.Add(new Class { ClassQuarter = Quarter.F });
            quarters.Add(new Class { ClassQuarter = Quarter.S });
            quarters.Add(new Class { ClassQuarter = Quarter.E });
            quarters.Add(new Class { ClassQuarter = Quarter.V });

            Class_Sub_Quarter_ComboBox.ItemsSource = quarters;
            Class_Sub_Quarter_ComboBox.DisplayMemberPath = "ClassQuarter";

        }
        private void ComboBoxFunctionLicenseTypes()
        {

            List<Class> types = new List<Class>();

            types.Add(new Class { ClassLicenseType = LicenseType.B });
            types.Add(new Class { ClassLicenseType = LicenseType.A1 });
            types.Add(new Class { ClassLicenseType = LicenseType.A2 });
            types.Add(new Class { ClassLicenseType = LicenseType.A });

            Class_Sub_Type_ComboBox.ItemsSource = types;
            Class_Sub_Type_ComboBox.DisplayMemberPath = "ClassLicenseType";
        }
        #endregion

        #region Database
        #region Connection
        //Creates a connect between students and class
        public void CreateConnection(ConStuClass conToBeCreated)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();
                
                //Creates a cmd SqlCommand, which enableds the ability to INSERT INTO the table with the corresponding attributes 
                SqlCommand cmd = new SqlCommand("INSERT INTO VK_Class_Student (CK_ClassName, CK_StuCPR)" +
                                                "VALUES(@CK_ClassName,@CK_StuCPR)" +
                                                "SELECT @@IDENTITY", con);


                //Add corresponding attribute to the database through the use of cmd
                cmd.Parameters.AddWithValue("@CK_ClassName", conToBeCreated.CK_ClassName);
                cmd.Parameters.AddWithValue("@CK_StuCPR", conToBeCreated.CK_StuCPR);
                
                //Tells the database to execute the sql commands
                cmd.ExecuteScalar();
            }
        }

        public bool DoesConExist(ConStuClass conToBeCreated) /// Was it possible to add this back into the original method? 
        {
            bool exist = true;
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();
                SqlCommand exists = new SqlCommand("SELECT COUNT(CK_ClassName) FROM VK_Class_Student WHERE CK_ClassName = @CK_ClassName AND CK_StuCPR = @CK_StuCPR", con);
                exists.Parameters.AddWithValue("@CK_StuCPR", currentStu.CK_StuCPR);
                exists.Parameters.AddWithValue("@CK_ClassName", currentStu.CK_ClassName);
                int stuCount = (int)exists.ExecuteScalar();
                if (stuCount == 0) { exist = false; }
            }
            return exist;
        }

        //Retrieves the data of a specific row in the database where the row number is equal to dbRowNum + 1
        public void RetrieveConnection(int dbRowNum)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();
                //Creates a cmd SqlCommand, which SELECTs a specific row 
                SqlCommand cmd = new SqlCommand("SELECT CK_ClassName, CK_StuCPR FROM VK_Class_Student ORDER BY CK_ClassName ASC OFFSET @dbRowNum ROWS FETCH NEXT 1 ROW ONLY", con);

                SqlCommand stu = new SqlCommand("SELECT StuFirstName, StuLastName, StuPhone, StuEmail FROM VK_Students WHERE PK_StuCPR = @CK_StuCPR", con); 

                //Set dbRowNum to 0 if under 0
                if (dbRowNum < 0)
                {
                    dbRowNum = 0;
                }
                //Gives @dbRowNum the value of dbRowNum
                cmd.Parameters.AddWithValue("@dbRowNum", dbRowNum);

                //Set up a data reader called dr, which reads the data from cmd (the previous sql command)
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    //While-loop running while dr is reading 
                    while (dr.Read())
                    {
                        string temp = dr["CK_StuCPR"].ToString();
                        stu.Parameters.AddWithValue("@CK_StuCPR", temp);
                        conToBeRetrieved = new ConStuClass("", "", "", "", "", "", "")
                        {
                            //Sets the attributes of conToBeRetrieved equal to the data from the current row of the database
                            CK_ClassName = dr["CK_ClassName"].ToString(),
                            CK_StuCPR = dr["CK_StuCPR"].ToString(),
                        };
                    }
                }
                using (SqlDataReader dr2 = stu.ExecuteReader())
                {
                    while (dr2.Read())
                    {
                        //Sets conToBeRetrieve a new empty ClassStuConnection, which is then filled
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

                //Creates a cmd SqlCommand, which DELETEs a specific row in the table, based on the CK_ClassName
                SqlCommand cmd = new SqlCommand("DELETE FROM VK_Class_Student WHERE CK_StuCPR = @CK_StuCPR", con);

                //Gives @PK_LesId the value of conToBeDeleted
                cmd.Parameters.AddWithValue("@CK_StuCPR", conToBeDeleted);

                //Tells the database to execute the cmd sql command
                cmd.ExecuteScalar();
            }
        }
        #endregion
        #region Class
        //Retrieves the data of a specific row in the database where the row number is equal to dbRowNum + 1
        public void RetrieveClassData(string dbRow)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT PK_ClassName, ClassYear, ClassNumber, ClassQuarter, ClassLicenseType FROM VK_Classes WHERE PK_ClassName = @dbRow", con);

                
                cmd.Parameters.AddWithValue("@dbRow", dbRow);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        classToBeRetrieved = new Class(default, "", "", default, "")
                        {
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

        //Edits the data of a previously existing Lesson
        public void UpdateClass(Class classToBeUpdated)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates a cmd SqlCommand, which UPDATEs the attributes of a specific row in the table, based on the LesId
                SqlCommand cmd = new SqlCommand("UPDATE VK_Classes SET ClassYear = @ClassYear, ClassNumber = @ClassNumber, ClassQuarter = @ClassQuarter, ClassLicenseType = @ClassLicenseType, PK_ClassName = @NewClassName WHERE PK_ClassName = @PK_ClassName", con);

                SqlCommand count = new SqlCommand("SELECT COUNT(ClassQuarter) FROM VK_Classes WHERE ClassQuarter = @ClassQuarter AND ClassYear = @ClassYear", con);
                count.Parameters.Add("@ClassQuarter", SqlDbType.NVarChar).Value = classToBeUpdated.ClassQuarter;
                count.Parameters.Add("@ClassYear", SqlDbType.NVarChar).Value = classToBeUpdated.ClassYear;
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
                
                newName = $"{currentClass.ClassQuarter}{currentClass.ClassYear}-{classToBeUpdated.ClassNumber}";
                
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
                        stuToBeRetrieved = new ConStuClass("", "", "", "", "", "", "")
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

        private void Class_Sub_DelClass_Button_Click(object sender, RoutedEventArgs e)
        {
            string messageBoxText = $"Du er ved at slette {currentClassName}.\nEr du sikker på at du gerne vil slette {currentClassName}?";
            string caption = "ADVARSEL";
            MessageBoxButton button = MessageBoxButton.OKCancel;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result;

            result = MessageBox.Show(messageBoxText, caption, button, icon);
            if (result == MessageBoxResult.OK) 
            {
                DeleteClass(currentClassName);
                this.NavigationService.GoBack();
            }
        }
    }
}
