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
    public partial class Class_Sub_ShowClass_Page : Page
    {
        public Class_Sub_ShowClass_Page()
        {
            InitializeComponent();
            Class_Sub_Title_TextBlock.Text = currentClassName;
        }

        ClassStuConnection conToBeRetrieved;
        Class classToBeRetrieved;
        Class currentClass = new Class();
        string currentClassName = "Spring29-2"; //Send something when opening page
        string currentConStuID;
        string currentConClassID;
        string newName;

        public class ClassStuConnection
        {
            public string CK_ClassName { get; set; }
            public string CK_StuCPR { get; set; }

            public ClassStuConnection(string _CK_ClassName, string _CK_StuCPR)
            {
                CK_ClassName = _CK_ClassName;
                CK_StuCPR = _CK_StuCPR;
            }
        }

        #region Hold Buttons
        private void Class_Sub_Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            RetrieveClassData(currentClassName);
            Class_Sub_Year_TextBox.Text = classToBeRetrieved.ClassYear;
            Class_Sub_Quarter_TextBox.Text = classToBeRetrieved.ClassQuarter.ToString();
            Class_Sub_ClassNumber_TextBox.Text = classToBeRetrieved.ClassNumber;
            Class_Sub_LicenseType_TextBox.Text = classToBeRetrieved.ClassLicenseType.ToString();
            Class_Sub_ClassNumber_TextBox.IsEnabled = true;
            Class_Sub_Year_TextBox.IsEnabled = true;
        }

        private void Class_Sub_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            RetrieveClassData(currentClassName);

            currentClass.ClassYear = Class_Sub_Year_TextBox.Text;
            currentClass.ClassQuarter = (Quarter)Enum.Parse(typeof(Quarter), Class_Sub_Quarter_TextBox.Text);
            currentClass.ClassLicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), Class_Sub_LicenseType_TextBox.Text);
            currentClass.ClassNumber = Class_Sub_ClassNumber_TextBox.Text;
            newName = $"{currentClass.ClassQuarter}{currentClass.ClassYear}-{currentClass.ClassNumber}";

            UpdateClass(currentClass);

            currentClassName = newName;
            Class_Sub_Title_TextBlock.Text = currentClassName;

            Class_Sub_ClassNumber_TextBox.IsEnabled = false;
            Class_Sub_Year_TextBox.IsEnabled = false;
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
                currentConStuID = (Class_Sub_ShowClass_DisStu_ListBox.SelectedItem as TempClass).StuID;
                currentConClassID = (Class_Sub_ShowClass_DisStu_ListBox.SelectedItem as TempClass).ClassID;
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
                SqlCommand count = new SqlCommand("SELECT COUNT(CK_StuCPR) from VK_Lessons WHERE CK_ClassName = @CK_ClassName", con);
                count.Parameters.AddWithValue("@CK_ClassName", currentClassName);
                //Saves count command result to int
                int intCount = (int)count.ExecuteScalar();

                //Make a list with the Item Class from below called stu (Name doesn't matter)
                //LesListBoxItems in my case
                List<TempClass> stu = new List<TempClass>();

                //Forloop which adds intCount number of new stu to stu-list
                for (int i = 0; i < intCount; i++)
                {
                    //Calls RetrieveLessonData method, sending i as index
                    RetrieveConnection(i);

//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    //Adds a new item from the item class with specific attributes to the list
                    //The data added comes from RetrieveLessonData
                    stu.Add(new TempClass() { StuID = conToBeRetrieved.CK_StuCPR, ClassID = conToBeRetrieved.CK_ClassName });

                    //Only necessary for multi-attribute ListBoxItem
                    //Set up the attribute 'SetUp' which is used to determine the appearance of the ListBoxItem 
                    //Mine isn't, so it's out commented
                    //stu[i].SetUp = $"{stu[i].Name}\n{stu[i].Type}\n{stu[i].Description}";
                }

                //Set the ItemsSource to the list, so that the ListBox uses the list to make the ListBoxItems
                Class_Sub_ShowClass_DisStu_ListBox.ItemsSource = stu;
            }
        }
        public class TempClass
        {
            public string StuID { get; set; }
            public string ClassID { get; set; }
            public string SetUp { get; set; }
        }
        #endregion

        #region Database
        #region Connection
        //Creates a connect between students and class
        public void CreateConnection(ClassStuConnection conToBeCreated)
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
                cmd.Parameters.Add("@CK_ClassName", SqlDbType.NVarChar).Value = conToBeCreated.CK_ClassName;
                cmd.Parameters.Add("@CK_StuCPR", SqlDbType.NVarChar).Value = conToBeCreated.CK_StuCPR;

                //Tells the database to execute the sql commands
                cmd.ExecuteScalar();
            }
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
                        //Sets conToBeRetrieve a new empty ClassStuConnection, which is then filled
                        conToBeRetrieved = new ClassStuConnection("", "")
                        {
                            //Sets the attributes of conToBeRetrieved equal to the data from the current row of the database
                            CK_ClassName = dr["CK_ClassName"].ToString(),
                            CK_StuCPR = dr["CK_StuCPR"].ToString(),
                        };
                    }
                }
            }
        }

        //Deletes the selected connection from the database
        public void DeleteConnection(int conToBeDeleted)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates a cmd SqlCommand, which DELETEs a specific row in the table, based on the CK_ClassName
                SqlCommand cmd = new SqlCommand("DELETE FROM VK_Class_Student WHERE CK_ClassName = @CK_ClassName", con);

                //Gives @PK_LesId the value of conToBeDeleted
                cmd.Parameters.AddWithValue("@CK_ClassName", conToBeDeleted);

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

                //Gives @attribute the value of attribute
                cmd.Parameters.AddWithValue("@PK_ClassName", currentClassName);
                cmd.Parameters.AddWithValue("@NewClassName", newName);
                cmd.Parameters.AddWithValue("@ClassYear", classToBeUpdated.ClassYear);
                cmd.Parameters.AddWithValue("@ClassNumber", classToBeUpdated.ClassNumber);
                cmd.Parameters.AddWithValue("@ClassQuarter", classToBeUpdated.ClassQuarter);
                cmd.Parameters.AddWithValue("@ClassLicenseType", classToBeUpdated.ClassLicenseType);

                //Tells the database to execute the cmd sql command
                cmd.ExecuteNonQuery();
            }
        }

        //Deletes the selected connection from the database
        public void DeleteClass(int classToBeDeleted)
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

        #endregion
    }
}
