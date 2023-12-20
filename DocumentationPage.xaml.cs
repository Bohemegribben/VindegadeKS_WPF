using Microsoft.Data.SqlClient;
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
using System.Threading;

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

            // call our ComboBoxStartUp to populate our ComboBoxes
            ComboBoxStartUp();

            //Disables the buttons which aren't relevant yet
            Doc_Save_Button.IsEnabled = false;
            Doc_Edit_Button.IsEnabled = true;
            Doc_Delete_Button.IsEnabled = false;
        }

        // Create CurrentDocumentation and CurrentStudent to contain current object - Needed in: Save_Button_Click & Edit_Button_Click
        public Documentation CurrentDocumentation = new Documentation();
        public Student CurrentStudent = new Student();

        // create a Student named studentToBeRetrieved - needed in RetrieveStudent
        public Student studentToBeRetrievedStu;

        // byte array fileData, is used in Doc_UploadFile_button to read the contents of the file
        byte[] fileData;

        // The Bool Edit, keeps track of if CurrentLesson is a new object or an old one being edited -
        // Needed in: Add_Button_Click, Save_Button_Click, Edit_Button_Click & ListBox_SelectionChanged
        bool edit = false;

        //Keeps track of the StuCPR while it's selected - Needed in Edit_Button_Click
        string currentItem;

        #region ButtonClicks

        #region AddButton
        private void Doc_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            Doc_Save_Button.IsEnabled = true;
            Doc_Edit_Button.IsEnabled = true;


            Doc_PickStudent_ComboBox.IsEnabled = true;
            Doc_PickDocument_ComboBox.IsEnabled = false;
            Doc_PickType_ComboBox.IsEnabled = true;
            Doc_StartDate_DateTimePicker.IsEnabled = true;
            Doc_EndDate_DateTimePicker.IsEnabled = true;

        }
        #endregion


        #region SaveButton
        private void Doc_Save_Button_Click(object sender, RoutedEventArgs e)
        {



            // We want add a new documentation with the selected values to a Student or update an existing document

            // Check if the DateTimePicker values are not null
            if (Doc_StartDate_DateTimePicker.Value.HasValue && Doc_EndDate_DateTimePicker.Value.HasValue)
            {                   //- Check for HasValue: Since Value is a nullable type (DateTime), we need to check if it has a value before trying to access it. 
                                //  This is done using the HasValue property.


                // Check if in edit mode or not
                if (edit == false)
                {
                    // prepare the New Documentation object 
                    Documentation documentationToBeCreated = new Documentation();
                    //First get the selected student´s CPR
                    if (Doc_PickStudent_ComboBox.SelectedItem is Student selectedStudent)
                    {
                        //Here we make sure that when we use DocStuCPR it is pulling information from StuCPR in Students to make sure correct CPR is accessed
                        documentationToBeCreated.DocStuCPR = selectedStudent.StuCPR;
                    }
                    else
                    {
                        // method to handle case where no student is selected 
                        return;
                    }

                    // Then get the selected DocType
                    if (Doc_PickType_ComboBox.SelectedItem is Documentation.DocTypeEnum selectedDocType)
                    {
                        documentationToBeCreated.DocType = selectedDocType;
                    }
                    else
                    {
                        // Handle case where no document type is selected
                        return;
                    }

                    //Set Properties
                    documentationToBeCreated.DocStartDate = Doc_StartDate_DateTimePicker.Value.Value;
                    documentationToBeCreated.DocEndDate = Doc_EndDate_DateTimePicker.Value.Value;
                    documentationToBeCreated.DocFile = fileData;

                    SaveDocumentation(documentationToBeCreated);
                }
                else
                {
                    //Now we retrieve the selected DocType

                    if (Doc_PickType_ComboBox.SelectedItem is Documentation.DocTypeEnum selectedDocType)
                    {
                        CurrentDocumentation.DocType = selectedDocType;
                    }
                    else
                    {
                        // Handle case where no document type is selected
                        return;
                    }
                    // Update the CurrentDocumentation with new values from the form
                    CurrentDocumentation.DocStartDate = Doc_StartDate_DateTimePicker.Value.Value;
                    CurrentDocumentation.DocEndDate = Doc_EndDate_DateTimePicker.Value.Value;
                    CurrentDocumentation.DocFile = fileData; // Assuming fileData is set correctly


                    UpdateDocumentation(CurrentDocumentation);

                    // Reset comboBox PickDocumentation
                    Doc_PickDocument_ComboBox.SelectedItem = null;
                }

                //Reset comboboxes
                ClearInputFields();

                //Controls which button the user can interact with - User needs to be able to Add more Lessons, but nothing else
                Doc_Add_Button.IsEnabled = true;
                Doc_Save_Button.IsEnabled = false;
                Doc_Edit_Button.IsEnabled = true;
                Doc_Delete_Button.IsEnabled = false;

            }

            else
            {
                //error message to user if dates are not selected
            }
        }
        #endregion

        #region EditButton
        private void Doc_Edit_Button_Click(object sender, RoutedEventArgs e)
        {

            

            //Sets edit to true, as the user is currently editing the Lesson
            edit = true;

            //Sets CurrentStudent CPR to currentItem
            CurrentStudent.StuCPR = currentItem;

            //Searches through the items (students) in our combobox to find the one whose CPR matches the selected student in our listbox
            //  - the one we stored in currentItem. 
            // The Cast<Student>() method is used to treat each item in the ComboBox as a student object
            var studentToSelect = Doc_PickStudent_ComboBox.Items.Cast<Student>().FirstOrDefault(s => s.StuCPR == currentItem);

            // If a match is found, then the student is set as the selected item in the ComboBox and display it
            
            if (studentToSelect != null)
            {
                Doc_PickStudent_ComboBox.SelectedItem = studentToSelect;
            }


            UnlockInputFields();

            Doc_PickStudent_ComboBox.IsEnabled = true;
            Doc_Save_Button.IsEnabled = true;
            Doc_Delete_Button.IsEnabled = true;

        }
        #endregion

        #region DeleteButton

        private void Doc_Delete_Button_Click(object sender, RoutedEventArgs e)
        {
       
            //Checks that a Document is selected 
            if (Doc_PickDocument_ComboBox.SelectedItem != null)
            {
                
                //DELETE the Document chosen
                DeleteDocument(CurrentDocumentation);
            }

            //Reset comboboxes
            ClearInputFields();
            // Dok_PickDocument_ComboBox.SelectedItem = null;

            //Controls which button the user can interact with - User needs to be able to Add more Lessons, but nothing else
            Doc_Add_Button.IsEnabled = true;
            Doc_Save_Button.IsEnabled = false;
            Doc_Edit_Button.IsEnabled = true;
            Doc_Delete_Button.IsEnabled = false;
        }
        #endregion

        #region FileUpload

        

        private void Doc_UploadFile_Button_Click(object sender, RoutedEventArgs e)
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
                Doc_UploadFile_Button.Content = fileName;

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

        #region InputFieldsMethods
        private void ClearInputFields()
        {
            Doc_PickStudent_ComboBox.SelectedItem = null;
          
            Doc_PickType_ComboBox.SelectedItem = null;

            Doc_StartDate_DateTimePicker.Value = DateTime.Now;
            Doc_EndDate_DateTimePicker.Value = DateTime.Now;

            Doc_UploadFile_Button.Content = "Upload File";
        }

        private void LockInputFields()
        {
            
            Doc_PickStudent_ComboBox.IsEnabled = false;
            Doc_PickType_ComboBox.IsEnabled = false;
            Doc_StartDate_DateTimePicker.IsEnabled = false;
            Doc_EndDate_DateTimePicker.IsEnabled = false;
        }

        private void UnlockInputFields()
        {
            Doc_PickStudent_ComboBox.IsEnabled = true;
            Doc_PickDocument_ComboBox.IsEnabled = true;
            Doc_PickType_ComboBox.IsEnabled = true;
            Doc_StartDate_DateTimePicker.IsEnabled = true;
            Doc_EndDate_DateTimePicker.IsEnabled = true;
        }

        #endregion

        #region ComboBoxes

        #region SelectionChanged
        private void Doc_PickStudent_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // First we need to check that the selected item is a Student object

            if(Doc_PickStudent_ComboBox.SelectedItem is Student selectedStudent) 
            {
                //Set CurrentStudent to SelectedStudent
                CurrentStudent = selectedStudent;

                //Retrieve documents for the selected student
                List<Documentation> documents = RetrieveAllDocuments();

                // Load the documents into the combobox
                Doc_PickDocument_ComboBox.ItemsSource = documents;
                Doc_PickDocument_ComboBox.DisplayMemberPath = "DocType";
                Doc_PickDocument_ComboBox.SelectedValuePath = "DocId";
                Doc_PickDocument_ComboBox.SelectedIndex = -1; // reset or set to a default value
            }

            else 
            {
                // if no student is selected or an invalid selection is made we set CurrentStudent to equal null 
                CurrentStudent = null;
                Doc_PickDocument_ComboBox = null;
            }
        }

        
        private void Doc_PickDocument_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Chekc if the selected item in our PickDocument Combobox is a Document
            if(Doc_PickDocument_ComboBox.SelectedItem is Documentation selectedDocument) 
            {

                CurrentDocumentation = selectedDocument;

                //Load document detatils into the controls
                Doc_StartDate_DateTimePicker.Value = selectedDocument.DocStartDate;
                Doc_EndDate_DateTimePicker.Value = selectedDocument.DocEndDate;

                // Set the SelectedItem of Dok_PickType_ComboBox to the document's type
                Doc_PickType_ComboBox.SelectedItem = selectedDocument.DocType;


            }
            else 
            {
                CurrentDocumentation = null;
            } 
        }






        #endregion

        #region ComboBoxMethods

        private void ComboBoxStartUp()
        {
            //Calls the SetUp methods for the ComboBoxes
            FillStudentComboBox();
            ComboBoxDocType();



        }


        private void FillStudentComboBox()
        {
            List<Student> students = RetrieveAllStudents();

            // First we set the Combox´s ItemSource to the list of Student Objects
            // This tells the ComboBox what collection of items it should display
            Doc_PickStudent_ComboBox.ItemsSource = students;

            // Then set the DisplayMemberPath to our property "StuFullName", to display students full names

            Doc_PickStudent_ComboBox.DisplayMemberPath = "StuFullName";

        }

        private void ComboBoxDocType()
        {

            // Populate the ComboBox with enum values directly
            Doc_PickType_ComboBox.ItemsSource = Enum.GetValues(typeof(Documentation.DocTypeEnum));
            Doc_PickType_ComboBox.SelectedIndex = 0;
        }


        #endregion

        #endregion



        #region Database

        #region RetrieveStudent
        public void RetrieveStudent(int dbRowNum)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates a SqlCommand, which SELECTs a specific row 
                SqlCommand stu = new SqlCommand("SELECT PK_StuCPR, StuFirstName, StuLastName, StuPhone, StuEmail FROM VK_Students ORDER BY StuFirstName ASC OFFSET @dbRowNum ROWS FETCH NEXT 1 ROW ONLY", con);

                //Set dbRowNum to 0 if under 0
                if (dbRowNum < 0)
                {
                    dbRowNum = 0;
                }

                //Gives @dbRowNum the value of dbRowNum
                stu.Parameters.AddWithValue("@dbRowNum", dbRowNum);

                //Set up a data reader called dr, which reads the data from cmd
                using (SqlDataReader dr = stu.ExecuteReader())
                {
                    //While-loop running while dr is reading 
                    while (dr.Read())
                    {
                        //Sets conToBeRetrieve a new empty ClassStuConnection, which is then filled
                        studentToBeRetrievedStu = new Student ("", "", "", "", "")
                        {
                            //Sets the attributes of conToBeRetrieved equal to the data from the current row of the database
                            StuCPR = dr["PK_StuCPR"].ToString(),
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

        #region RetrieveAllStudents

        //This method is needed in our combox PickStudents to retrieve data from all our students

        public List<Student> RetrieveAllStudents()
        {
            List<Student> students = new List<Student>();

            //Establish connection to our Database using our connection string that´s defined in our configuration
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //open the connection
                con.Open();

                //Now we create a SQL Command named "cmd" to select all students from our VK_Students table 
                //We make the Query to select all students ordered by first name
                // This command is fetching all rows in one go, eliminating the need to fect rows one by one.
                SqlCommand cmd = new SqlCommand("SELECT PK_StuCPR, StuFirstName, StuLastName, StuPhone, StuEmail FROM VK_Students ORDER BY StuFirstName ASC", con);

               

               //Executing the command and using SQLdataReader to read the data that is returned from the database
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    // Loop through the data rows our database has returned
                    while (dr.Read())
                    {
                        // Create a new Student Object and initialize it with data from the current row
                        Student student = new Student
                        {
                            StuCPR = dr["PK_StuCPR"].ToString(),
                            StuFirstName = dr["StuFirstName"].ToString(),
                            StuLastName = dr["StuLastName"].ToString(),
                            StuPhone = dr["StuPhone"].ToString(),
                            StuEmail = dr["StuEmail"].ToString(),
                        };

                        // Add the newly created Student object to the list
                        students.Add(student);
                    }
                }


            }

            return students;
        }

        #endregion

        #region RetrieveAllDocuments

       public List<Documentation> RetrieveAllDocuments()
        {
            List<Documentation> documents = new List<Documentation>();

            //Establish connection to our Database using our connection string that´s defined in our configuration
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //open the connection
                con.Open();

                //Now we create a SQL Command named "cmd" to select all students from our VK_Students table 
                //We make the Query to select all students ordered by first name
                // This command is fetching all rows in one go, eliminating the need to fect rows one by one.
                SqlCommand cmd = new SqlCommand("SELECT PK_DocID, DocStartDate, DocEndDate, DocType, FK_StuCPR, DocumentationFile FROM VK_Documentations ORDER BY PK_DocID ASC", con);


                Documentation.DocTypeEnum ParseDocTypeEnum(object docTypeValue)
                {
                    if (docTypeValue != DBNull.Value && Enum.TryParse(docTypeValue.ToString(), out Documentation.DocTypeEnum docType))
                    {
                        return docType;
                    }
                    else
                    {
                        return default; // Or a default value for your enum
                    }
                }

                //Executing the command and using SQLdataReader to read the data that is returned from the database
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    // Loop through the data rows our database has returned
                    while (dr.Read())
                    {
                        // Create a new Documentation Object and initialize it with data from the current row
                        Documentation document = new Documentation
                        {
                            DocId = int.Parse(dr["PK_DocID"].ToString()),
                            DocStartDate = dr["DocStartDate"] != DBNull.Value ? (DateTime)dr["DocStartDate"] : default(DateTime),
                            DocEndDate = dr["DocEndDate"] != DBNull.Value ? (DateTime)dr["DocEndDate"] : default(DateTime),
                            DocType = ParseDocTypeEnum(dr["DocType"])
                            //DocType = Enum.Parse<Documentation.DocTypeEnum>(dr["DocType"].ToString())

                        };

                        // Add the newly created Student object to the list
                        documents.Add(document);
                    }
                }


            }

            return documents;


        } 

        #endregion

        #region UpdateDocumentation
        public void UpdateDocumentation(Documentation documentationToUpdate)
        {
          

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();

                //Creates a SqlCommand named "cmd", which UPDATES the attributes of a specific row in the table, based on the DocID
                //SqlCommand cmd = new SqlCommand("UPDATE VK_Documentations SET DocStartDate = @DocStartDate, DocEndDate = @DocEndDate, DocType = @DocType, DocumentationFile = @DocumentationFile WHERE PK_DocID = @DocId", con);

                // Since we want to be able to edit a document without uploading a new file we have to use If statements to run our command
                // - depending on whether DocumentationFile is NULL or not
                SqlCommand cmd;
                // If the Documentation file isn´t NULL we ask to update tile file
                if (documentationToUpdate.DocFile != null && documentationToUpdate.DocFile.Length > 0)
                {
                    // Update including the DocumentationFile
                    cmd = new SqlCommand("UPDATE VK_Documentations SET DocStartDate = @DocStartDate, DocEndDate = @DocEndDate, DocType = @DocType, DocumentationFile = @DocumentationFile WHERE PK_DocID = @DocId", con);
                    cmd.Parameters.Add("@DocumentationFile", SqlDbType.VarBinary).Value = documentationToUpdate.DocFile;
                }

                // If Documentation is NULL, we ask to update Database without changing DocumentationFile
                else
                {
                    // Update without changing the DocumentationFile
                    cmd = new SqlCommand("UPDATE VK_Documentations SET DocStartDate = @DocStartDate, DocEndDate = @DocEndDate, DocType = @DocType WHERE PK_DocID = @DocId", con);
                }

                //Gives @attribute the value of attribute
                cmd.Parameters.Add("@DocStartDate", SqlDbType.DateTime).Value = documentationToUpdate.DocStartDate;
                cmd.Parameters.Add("@DocEndDate", SqlDbType.DateTime).Value = documentationToUpdate.DocEndDate;
                cmd.Parameters.Add("@DocType", SqlDbType.NVarChar).Value = documentationToUpdate.DocType.ToString();
                //cmd.Parameters.Add("@DocumentationFile", SqlDbType.VarBinary).Value = documentationToUpdate.DocFile;
                cmd.Parameters.Add("@DocId", SqlDbType.Int).Value = documentationToUpdate.DocId;

                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region SaveDocumentation

        public void SaveDocumentation(Documentation documentationToBeCreated)
        {
            

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
             {
                 con.Open();
                 SqlCommand cmd = new SqlCommand("INSERT INTO VK_Documentations (DocStartDate, DocEndDate, DocType, FK_StuCPR, DocumentationFile)" +
                                                  "VALUES(@DocStartDate, @DocEndDate,@DocType,@FK_StuCPR, @DocumentationFile)" +
                                                  "SELECT @@IDENTITY", con);

                 cmd.Parameters.Add("@DocStartDate", SqlDbType.Date).Value = documentationToBeCreated.DocStartDate;
                 cmd.Parameters.Add("@DocEndDate", SqlDbType.Date).Value = documentationToBeCreated.DocEndDate;
                 cmd.Parameters.Add("@DocType", SqlDbType.NVarChar).Value = documentationToBeCreated.DocType;
                 cmd.Parameters.Add("@FK_StuCPR", SqlDbType.NVarChar).Value = documentationToBeCreated.DocStuCPR;

                 cmd.Parameters.Add("@DocumentationFile", SqlDbType.VarBinary).Value = documentationToBeCreated.DocFile;

                 documentationToBeCreated.DocId = Convert.ToInt32(cmd.ExecuteScalar());

                // cmd.ExecuteScalar();

             } 


            

           // UploadFile(fileData);
        }
        #endregion

        #region DeleteDocument
        public void DeleteDocument(Documentation DocumentToBeDeleted)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates a cmd SqlCommand, which DELETEs a specific row in the table, based on the CK_StuCPR
                SqlCommand cmd = new SqlCommand("DELETE FROM VK_Documentations WHERE PK_DocID = @PK_DocID", con);

                //Gives @CK_StuCPR the value of conToBeDeleted
                cmd.Parameters.AddWithValue("@PK_DocId", DocumentToBeDeleted.DocId);

                //Tells the database to execute the cmd sql command
                cmd.ExecuteScalar();
            }
        }

        #endregion

        #endregion
    }
}
