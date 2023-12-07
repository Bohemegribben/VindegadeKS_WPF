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
        }

        ClassStuConnection conToBeRetrieved;
        Class classToBeRetrieved;

        public class ClassStuConnection
        {
            public string CK_ClassName { get; set; }
            public string CK_StuCPR { get; set;}

            public ClassStuConnection(string _CK_ClassName, string _CK_StuCPR)
            {
                CK_ClassName = _CK_ClassName;
                CK_StuCPR = _CK_StuCPR;
            }
        }

        #region ListBox
        //Controls what happens when you select an item in the ListBox
        private void Les_DisLes_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Safety check, to make sure that the selected item exists
            if (Les_DisLes_ListBox.SelectedItem != null)
            {
                //Everything inside of the if-statement will likely have to be personalised 

                //Changes the text from the display window 
                Les_DisName_TextBlock.Text = "Modul Navn: " + (Les_DisLes_ListBox.SelectedItem as Lesson).LesName;
                Les_DisType_TextBlock.Text = "Modul Type: " + (Les_DisLes_ListBox.SelectedItem as Lesson).LesType;
                Les_DisDescription_TextBlock.Text = "Modul Beskrivelse: " + (Les_DisLes_ListBox.SelectedItem as Lesson).LesDescription;


                //Sets currentItem to equal the ID of selected item
                currentItem = (Les_DisLes_ListBox.SelectedItem as Lesson).LesId;

                //Sets edit to false, as it is impossible for it to be true currently
                edit = false;

                //Controls which button the user can interact with - User needs able to edit and delete, but not save
                Les_Save_Button.IsEnabled = false;
                Les_Edit_Button.IsEnabled = true;
                Les_Delete_Button.IsEnabled = true;
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
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_LesID) from VK_Lessons", con);
                //Saves count command result to int
                int intCount = (int)count.ExecuteScalar();

                //Make a list with the Item Class from below called items (Name doesn't matter)
                //LesListBoxItems in my case
                List<Lesson> items = new List<Lesson>();

                //Forloop which adds intCount number of new items to items-list
                for (int i = 0; i < intCount; i++)
                {
                    //Calls RetrieveLessonData method, sending i as index
                    RetrieveLessonData(i);

                    //Adds a new item from the item class with specific attributes to the list
                    //The data added comes from RetrieveLessonData
                    items.Add(new Lesson() { LesId = lesToBeRetrieved.LesId, LesName = lesToBeRetrieved.LesName, LesType = lesToBeRetrieved.LesType, LesDescription = lesToBeRetrieved.LesDescription });

                    //Only necessary for multi-attribute ListBoxItem
                    //Set up the attribute 'SetUp' which is used to determine the appearance of the ListBoxItem 
                    //Mine isn't, so it's out commented
                    //items[i].SetUp = $"{items[i].Name}\n{items[i].Type}\n{items[i].Description}";
                }

                //Set the ItemsSource to the list, so that the ListBox uses the list to make the ListBoxItems
                Les_DisLes_ListBox.ItemsSource = items;
            }
        }
        #endregion

        #region ComboBox
        //Setup the ComboBox
        private void ComboBoxFunction()
        {
            //New list and datapoints for Combobox
            List<Lesson> types = new List<Lesson>();
            types.Add(new Lesson { LesType = "Teori", DisplayValue = "Teorisk modul" });
            types.Add(new Lesson { LesType = "Praktisk_Hold", DisplayValue = "Praktisk m/ hold" });
            types.Add(new Lesson { LesType = "Praktisk_Solo", DisplayValue = "Praktisk m/ individuel studerende" });

            //Set the ItemsSource
            Les_Type_ComboBox.ItemsSource = types;
            //Sets which attribute is displayed
            Les_Type_ComboBox.DisplayMemberPath = "DisplayValue";
            //Sets default choice
            Les_Type_ComboBox.SelectedIndex = 0;
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
        public void RetrieveClassData(int dbRowNum)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();
                //Creates a cmd SqlCommand, which SELECTs a specific row 
                SqlCommand cmd = new SqlCommand("SELECT PK_ClassName, ClassQuarter, ClassYear, ClassNumber, ClassLicenseType FROM VK_Classes ORDER BY PK_ClassName ASC OFFSET @dbRowNum ROWS FETCH NEXT 1 ROW ONLY", con);

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
                        //Sets lesToBeRetrieve a new empty Lesson, which is then filled
                        classToBeRetrieved = new Class("", "", "", "")
                        {
                            //Sets the attributes of lesToBeRetrieved equal to the data from the current row of the database
                            ClassName = dr["PK_ClassName"].ToString(),
                            ClassQuarter = dr["ClassQuarter"].ToString(),
                            ClassYear = dr["ClassYear"].ToString(),
                            ClassNumber = dr["ClassNumber"].ToString(),
                            ClassLicenceType = dr["ClassLicenseType"].ToString(),
                        };
                    }
                }
            }
        }

        //Edits the data of a previously existing Lesson
        public void EditClass(VindegadeKS_WPF.Lesson lesToBeUpdated)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates a cmd SqlCommand, which UPDATEs the attributes of a specific row in the table, based on the LesId
                SqlCommand cmd = new SqlCommand("UPDATE VK_Lessons SET LesName = @LesName, LesType = @LesType, LesDescription = @LesDescription WHERE PK_LesID = @LesId", con);

                //Gives @attribute the value of attribute
                cmd.Parameters.AddWithValue("@LesName", lesToBeUpdated.LesName);
                cmd.Parameters.AddWithValue("@LesType", lesToBeUpdated.LesType);
                cmd.Parameters.AddWithValue("@LesDescription", lesToBeUpdated.LesDescription);
                cmd.Parameters.AddWithValue("@LesId", lesToBeUpdated.LesId);

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
                SqlCommand cmd = new SqlCommand("DELETE FROM VK_Class_Student WHERE CK_ClassName = @CK_ClassName", con);

                //Gives @PK_LesId the value of conToBeDeleted
                cmd.Parameters.AddWithValue("@CK_ClassName", classToBeDeleted);

                //Tells the database to execute the cmd sql command
                cmd.ExecuteScalar();
            }
        }
        #endregion
        #endregion
    }
}
