﻿using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
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
using System.Security.Cryptography.X509Certificates;
using static VindegadeKS_WPF.InstructorPage;
using static VindegadeKS_WPF.LessonPage;
using Microsoft.Win32;
using System.IO;


namespace VindegadeKS_WPF
{
    /// <summary>
    /// Interaction logic for DocumentationPage.xaml
    /// </summary>
    public partial class DocumentationPage : Page
    {
        public DocumentationPage()
        {
            InitializeComponent();
            LockInputFields();
            ListBoxFunction();

            //Disables the buttons which aren't relevant yet
            Dok_Save_Button.IsEnabled = false;
            Dok_Edit_Button.IsEnabled = false;
            Dok_Delete_Button.IsEnabled = false;
        }

        // Create CurrentStudent to contain current object - Needed in: Save_Button_Click & Edit_Button_Click
        public Documentation CurrentDocumentation = new Documentation();
        public Student CurrentStudent = new Student();

        // Moved out here instead of staying in 'Retrieve', so ListBoxFunction can access - Needed in: ListBoxFunction & RetrieveData
        public DocListBoxItems documentationToBeRetrieved;
        public Student studentToBeRetrieved;
        

        // Keeps track of if CurrentLesson is a new object or an old one being edited -
        // Needed in: Add_Button_Click, Save_Button_Click, Edit_Button_Click & ListBox_SelectionChanged
        bool edit = false;

        //Keeps track of the CPR of ListBoxItem while it's selected - Edit_Button_Click & ListBox_SelectionChanged
        string currentItem;



        #region ListBoxFunctions
        public class DocListBoxItems
        {
            public int ListBoxDocId { get; set; }
            public string ListBoxStuCPR { get; set; }
            public string ListBoxStuFirstName { get; set; }
            public string ListBoxStuLastName { get; set; }
            public DateOnly ListBoxDocStartDate { get; set; }
            public DateOnly ListBoxDocEndDate { get; set; }
            public string ListBoxDocType { get; set; }
            public string Setup { get; set; }

            // To be able to see both First and Last name in our listbox,
            // we have to concatenate (the joining of two or more strings) our two properties ListBoxStuFirstName and ListBoxStuLastName 
            public string ListBoxStuFullName
            {
                get { return $"{ListBoxStuFirstName} {ListBoxStuLastName}".Trim(); }
            }

            public DocListBoxItems(int _listBoxDocId,
                                      string _listBoxStuCPR,
                                      string _listBoxStuFirstName,
                                      string _listBoxStuLastName,
                                      DateOnly _listBoxDocStartDate,
                                      DateOnly _listBoxDocEndDate,
                                      string _listBoxDocType,
                                      string setup)
            {
                ListBoxDocId = _listBoxDocId;
                ListBoxStuCPR = _listBoxStuCPR;
                ListBoxStuFirstName = _listBoxStuFirstName;
                ListBoxStuLastName = _listBoxStuLastName;
                ListBoxDocStartDate = _listBoxDocStartDate;
                ListBoxDocEndDate = _listBoxDocEndDate;
                ListBoxDocType = _listBoxDocType;
                Setup = setup;
            }

            public DocListBoxItems() : this(0, "", "", "", default, default, "","")
            { }

        }

        private void Dok_DisStudents_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            


        }

        private void ListBoxFunction () 
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens the connection
                con.Open();

                //Creates a count SqlCommand, which gets the number of rows in the table 
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_ApmtID) from VK_Appointments", con);

                //Saves count command result to int
                int intCount = (int)count.ExecuteScalar();

                //Make a list of 5-tuples containing instanses of each class necessary in appointments

                List<DocListBoxItems> items = new List<DocListBoxItems>();

                //Forloop which adds intCount number of new items to items-list
                for (int i = 0; i < intCount; i++)
                {
                    //Calls RetrieveLessonData method, sending i as index
                    RetrieveStudentData(i);

                    //Add new items from the item class with specific attributes to the list
                    //Will later be remade to automatically add items based on the database
                    items.Add(documentationToBeRetrieved);

                    //Adds a new item from the item class with specific attributes to the list
                    //The data added comes from RetrieveLessonData
                    // items.Add(new DocListBoxItems() { ListBoxStuCPR = studentToBeRetrieved.StuCPR, ListBoxStuFirstName = studentToBeRetrieved.StuFirstName, ListBoxStuLastName = studentToBeRetrieved.StuLastName, ListBoxDocStartDate = documentationToBeRetrieved.ListBoxDocStartDate, ListBoxDocEndDate = documentationToBeRetrieved.ListBoxDocEndDate, ListBoxDocType = documentationToBeRetrieved.ListBoxDocType });

                    items.Add(new DocListBoxItems() { ListBoxStuCPR = studentToBeRetrieved.StuCPR, ListBoxStuFirstName = studentToBeRetrieved.StuFirstName, ListBoxStuLastName = studentToBeRetrieved.StuLastName });


                    //Only necessary for multi-attribute ListBoxItem
                    //Set up the attribute 'SetUp' which is used to determine the appearance of the ListBoxItem 
                    //Forloop to go through all items in the items-list, to add and fill the 'SetUp' attribute

                    // documentations[i].Setup = $"{documentations[i].ListBoxStuFirstName} - {documentations[i].ListBoxDocStartDate} \n{documentations[i].ListBoxDocEndDate} - {documentations[i].ListBoxDocType}";
                }

                //Set the ItemsSource to the list, so that the ListBox uses the list to make the ListBoxItems
                Dok_DisStudents_ListBox.ItemsSource = items;
            }
        }

        #endregion

        #region SeachBar
        private void Dok_DisSearch_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Hvis search bar er tom
            if (string.IsNullOrEmpty(Dok_DisSearch_TextBox.Text))
            {
                // så skal search textbox ikke være synlig
                Dok_DisSearch_TextBox.Visibility = Visibility.Collapsed;

                // watermark textbox skal være synlig
                Dok_DisWaterMark_TextBox.Visibility = Visibility.Visible;

            }
        }

        private void Dok_DisWaterMark_TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //hvis man er trykket ind på "search bar textbox" skal "watermark textbox" ikke være synlig længere
            Dok_DisWaterMark_TextBox.Visibility = Visibility.Collapsed;

            // og vores "search textbox skal være synlig
            Dok_DisSearch_TextBox.Visibility = Visibility.Visible;
            Dok_DisSearch_TextBox.Focus();
        }
        #endregion


        #region Buttons

       
        private void Dok_Add_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Dok_Save_Button_Click(object sender, RoutedEventArgs e)
        {


            // call the the UploadFile method to connect to our database and save the uploaded file
            UploadFile(fileData);


        }

        private void Dok_Edit_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Dok_Delete_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        #region FileUpload

        // byte array fileData, is used in DokuFile_button to read the contents of the file, and Save_Button to call the UploadFile method
        byte[] fileData;

        private void Dok_DokuFile_Button_Click(object sender, RoutedEventArgs e)
        {

            // First we create an instance of OpenFIleDialog, to show a dialog so the user can select a file
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Then we show the dialog and check if the user has selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                // create a string named "filePath" to get the path of the selected file
                string filePath = openFileDialog.FileName;

                // create a string named "fileName" to extract the name of the uploaded file (need later to change the content of the button, when file is succesfully uploaded)
                string fileName = System.IO.Path.GetFileName(filePath);
                
                // Create a byte array named "fileDate" To read the contents of the file
                fileData = File.ReadAllBytes(filePath);


                // Change the button content to the file name
                Dok_DokuFile_Button.Content = fileName;

                /*
                 try
                 {
                     // Create a byte array named "fileDate" To read the contents of the file
                     byte[] fileData = File.ReadAllBytes(filePath);

                     // call the the UploadDocumentation method to connect to our database
                     UploadFile(fileData);
                 }

                 // to handle any exceptioons such as file read errors, database connection issues etc.
                 catch (Exception ex)
                 {
                     MessageBox.Show("An Error occured:" + ex.Message);
                 }*/

            }

        }

        #endregion
        #endregion

        #region ButtonMethods

        #region UploadFile

        public void UploadFile(byte[] fileData) 
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString)) 
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("INSERT INTO VK_Documentations (DocumentationFile)" + 
                    "VALUES (@FileData)" + 
                    "SELECT @@IDENTITY", con);

                cmd.Parameters.AddWithValue("@FileData", fileData);

                cmd.ExecuteNonQuery();

             
            }

        }

        #endregion

        #region RetrieveDocumentationData
        public void RetrieveDocumentationData(int dBRowNumber)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT PK_DocID, DocStartDate, DocEndDate, DocType, FK_StuCPR, DocumentationFile FROM VK_Documentations ORDER BY PK_DocId ASC OFFSET @dBRowNumber ROWS FETCH NEXT 1 ROW ONLY", con);

                if (dBRowNumber < 0)
                {
                    dBRowNumber = 0;
                }
                cmd.Parameters.AddWithValue("@dBRowNumber", dBRowNumber);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {

                        documentationToBeRetrieved = new DocListBoxItems (0, "", "", "", default, default, "", "")
                        {
                            ListBoxDocId = int.Parse(dr["PK_DocID"].ToString()),
                            ListBoxDocStartDate = DateOnly.FromDateTime((DateTime)dr["DocStarDate"]),
                            ListBoxDocEndDate = DateOnly.FromDateTime((DateTime)dr["DocEndDate"]),
                            ListBoxDocType = dr["DocType"].ToString(),
                           
                        };
                    }
                }
            }
        }



        #endregion

        #region RetrieveStudentData
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
        #endregion

        #endregion

        #region InputFieldsMethods
        private void ClearInputFields()
        {
            Dok_PickCPR_ComboBox.SelectedItem = null;
            Dok_PickType_ComboBox.SelectedItem = null;
            Dok_StartDate_DateTimePicker.Value = DateTime.Now;
            Dok_EndDate_DateTimePicker.Value = DateTime.Now;
        }

        private void LockInputFields()
        {
            Dok_PickCPR_ComboBox.IsEnabled = false;
            Dok_PickType_ComboBox.IsEnabled = false;
            Dok_StartDate_DateTimePicker.IsEnabled = false;
            Dok_EndDate_DateTimePicker.IsEnabled = false;
        }

        private void UnlockInputFields()
        {
            Dok_PickCPR_ComboBox.IsEnabled = true;
            Dok_PickType_ComboBox.IsEnabled = true;
            Dok_StartDate_DateTimePicker.IsEnabled = true;
            Dok_EndDate_DateTimePicker.IsEnabled = true;
        }

        #endregion

        #region ComboBoxes

        private void Dok_PickCPR_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Dok_PickType_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #endregion

        #region DateTimePickers
        private void Dok_EndDate_DateTimePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void Dok_StartDate_DateTimePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }
        #endregion
    }
}