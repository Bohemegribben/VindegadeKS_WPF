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

            //Locks all input fields at start up until a button is pressed
            LockInputFields();

            // call our ComboBoxStartUp to populate our ComboBoxes
            ComboBoxStartUp();

            //Disables the buttons which aren't relevant yet
            Doc_Save_Button.IsEnabled = false;
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

        // Lets the user add a new document to an existing student
        private void Doc_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            //set edit mode to false, since we are currently adding a new document
            edit = false;
           
            //Enables save button, since it is now relevant
            Doc_Save_Button.IsEnabled = true;
           


            // Enables all comboboxes expect for PickDocument since this combobox is only needed in Edit mode
            Doc_PickStudent_ComboBox.IsEnabled = true;
            Doc_PickDocument_ComboBox.IsEnabled = false;
            Doc_PickType_ComboBox.IsEnabled = true;
            Doc_StartDate_DateTimePicker.IsEnabled = true;
            Doc_EndDate_DateTimePicker.IsEnabled = true;

        }
        #endregion


        #region SaveButton

        // Lets the user save either a new document to a student or save changes made to an existing student
        private void Doc_Save_Button_Click(object sender, RoutedEventArgs e)
        {

            // We want to add a new documentation with the selected values to a Student or update an existing document

            // First Check if the DateTimePicker values are not null, since we do not wish to save a document without StartDate and EndDate
            if (Doc_StartDate_DateTimePicker.Value.HasValue && Doc_EndDate_DateTimePicker.Value.HasValue)
            {                   //- Check for HasValue: Since our DateTimePicker.Value is a nullable type (DateTime), we need to check if it has a value before trying to access it. 
                                //  This is done using the HasValue property, which tells us whether or not our Nullable type T actually has a value. 
                                // If HasValue is true, our DateTimePicker.Value has value and we can proceed to with our save function.
                                // If HasValue is false, our DateTimePicker.Value is NULL, and we will then prompt the program to give an error-message.


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
                        MessageBox.Show("Vælg venligst elev");
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
                        MessageBox.Show("Vælg venligtst dokument type");
                        return;
                    }

                    //Set Properties
                    documentationToBeCreated.DocStartDate = Doc_StartDate_DateTimePicker.Value.Value;
                    documentationToBeCreated.DocEndDate = Doc_EndDate_DateTimePicker.Value.Value;
                    documentationToBeCreated.DocFile = fileData;

                    SaveDocumentation(documentationToBeCreated);
                }

                // If we are not in edit mode, we will call the UpdateDocumentation function and update the chosen document the user wish to edit
                else
                {
                    //First retrieve the selected DocType

                    if (Doc_PickType_ComboBox.SelectedItem is Documentation.DocTypeEnum selectedDocType)
                    {
                        CurrentDocumentation.DocType = selectedDocType;
                    }
                    else
                    {
                        // Handle case where no document type is selected
                        MessageBox.Show("Vælg venligtst dokument type");
                        return;
                    }

                    // Update the CurrentDocumentation with the new values from the input fields
                    CurrentDocumentation.DocStartDate = Doc_StartDate_DateTimePicker.Value.Value;
                    CurrentDocumentation.DocEndDate = Doc_EndDate_DateTimePicker.Value.Value;
                    CurrentDocumentation.DocFile = fileData; 

                    // Call our UpdateDocumentation method to connect to our Database
                    UpdateDocumentation(CurrentDocumentation);

                    // Reset comboBox PickDocumentation
                    Doc_PickDocument_ComboBox.SelectedItem = null;
                }

                //Reset comboboxes
                ClearInputFields();
                //populate comboboxes again
                ComboBoxStartUp();

                //Controls which button the user can interact with - User needs to be able to Add more documents and edit again
                Doc_Add_Button.IsEnabled = true;
                Doc_Save_Button.IsEnabled = false;
                Doc_Edit_Button.IsEnabled = true;
                Doc_Delete_Button.IsEnabled = false;

            }

            else 
            {   
                // Message to ask the user to type in StartDate and EndDate
                MessageBox.Show("Start Dato og Slut Dato skal være sat");
            }
        }
        #endregion

        #region EditButton

        // Lets the user edit an existing document belonging to a student
        private void Doc_Edit_Button_Click(object sender, RoutedEventArgs e)
        {

      
            //Sets edit to true, since we wish to edit a document belonging to a student
            edit = true;

            // Unlock inputFields
            UnlockInputFields();

            // Enable needed buttons and comboboxes (when you pick a student combobox PickDocument unlocks)
            Doc_PickStudent_ComboBox.IsEnabled = true;
            Doc_Save_Button.IsEnabled = true;
            Doc_Delete_Button.IsEnabled = true;

        }
        #endregion

        #region DeleteButton

        //Allows the user to delete a specific document from the selected Student
        private void Doc_Delete_Button_Click(object sender, RoutedEventArgs e)
        {
       
            //Checks that a Document is selected 
            if (Doc_PickDocument_ComboBox.SelectedItem != null)
            {
                
                //DELETE the Document chosen calling our DeleteDocument()
                DeleteDocument(CurrentDocumentation);
            }

            //Reset comboboxes
            ClearInputFields();
         

            //Controls which button the user can interact with again
            Doc_Add_Button.IsEnabled = true;
            Doc_Save_Button.IsEnabled = false;
            Doc_Edit_Button.IsEnabled = true;
            Doc_Delete_Button.IsEnabled = false;
        }
        #endregion

        #region FileUpload

        
        //This allows the user to upload a document file 
        private void Doc_UploadFile_Button_Click(object sender, RoutedEventArgs e)
        {

            // First we create an instance of OpenFIleDialog, to show a dialog so the user can select a file
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Then we show the dialog and check if the user has selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                // create a string named "filePath" to get the path of the selected file
                string filePath = openFileDialog.FileName;

                // create a string named "fileName" to extract the name of the uploaded file (needed later to change the content of the button, when file is succesfully uploaded)
                string fileName = System.IO.Path.GetFileName(filePath);
                
                // Create a byte array named "fileDate" To read the contents of the file
                fileData = File.ReadAllBytes(filePath);


                // Change the button content to the file name for user transparency
                Doc_UploadFile_Button.Content = fileName;

               

            }

        }

        #endregion

        #endregion

        #region InputFieldsMethods

        // Method to Clear all input fields -  used after either saving a new document or updates
        private void ClearInputFields()
        {
            Doc_PickStudent_ComboBox.SelectedItem = null;
          
            Doc_PickType_ComboBox.SelectedItem = null;

            Doc_StartDate_DateTimePicker.Value = DateTime.Now;
            Doc_EndDate_DateTimePicker.Value = DateTime.Now;

            // sets the name off our FileUpload button back to "Upload File"
            Doc_UploadFile_Button.Content = "Upload File";
        }

        // Method to lock all inputfields - used at startup
        private void LockInputFields()
        {
            
            Doc_PickStudent_ComboBox.IsEnabled = false;
            Doc_PickType_ComboBox.IsEnabled = false;
            Doc_StartDate_DateTimePicker.IsEnabled = false;
            Doc_EndDate_DateTimePicker.IsEnabled = false;
        }

        // Method to unlock all inputfields - needed in Edit Button Click

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

        // The Selection Changed event handler for our PickStudent combobox
        private void Doc_PickStudent_ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            // When we selec a student we wish to be able to see what documents the selected student has in our comboBox PickDocument
            // - if ofc, we are in edit mode - if the Edit button hasn´t been changed and we´ve pressed "Add" comboBox PickDocument will not be open
            
            // First we need to check that the selected item is a Student object

            if (Doc_PickStudent_ComboBox.SelectedItem is Student selectedStudent)
            {
                //Set CurrentStudent to SelectedStudent
                CurrentStudent = selectedStudent;

                //Retrieve documents for the selected student by creating a list of documents that calls our RetrieveDocument from the selectedStudent.StuCPR
                List<Documentation> documents = RetrieveDocument(selectedStudent.StuCPR);

                // Now we load the documents into the combobox

                //We set the ItemSource property of our PickDocument combobox to equal the list of documents we created above
                Doc_PickDocument_ComboBox.ItemsSource = documents;

                // we want the DisplayMemberPath propertu of the combobox (what name the combox displays for the documents) to be that of DocType
                Doc_PickDocument_ComboBox.DisplayMemberPath = "DocType";

                // We set the SelectedValuePath property to be DocId, since it is in charge of specifying the path tp the property
                // - of the bound objects SelectedValue property of the ComboBox that it should return
                // When we set it to equal DocId,we are telling the ComboBox that when an item is selected
                // - then the SelectedValue property of the ComboBox should return the DocId property of the selected Documentation Object
                Doc_PickDocument_ComboBox.SelectedValuePath = "DocId";

                // The SelectedIndex property indicates the index of the currently selected item in the combox - it resets the ComboBox so that no item is selected.
                // The index -1, is used to indicate that there´s no selection
                // Resetting the PickDocument comboBox when we pick a new Student is necessary, since the items in the PickStudent comboBox has now changed
                Doc_PickDocument_ComboBox.SelectedIndex = -1; 
            }

            else
            {
                // if no student is selected or an invalid selection is made we set CurrentStudent to equal null 
                CurrentStudent = null;
                if (Doc_PickDocument_ComboBox != null)
                {
                    Doc_PickDocument_ComboBox.ItemsSource = null;
                }
              
            }

        }

        // The SelectionChange event handler for comboBox PickDocument
        private void Doc_PickDocument_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // We wish to check if the selected item in our PickDocument Combobox is a Document
            if(Doc_PickDocument_ComboBox.SelectedItem is Documentation selectedDocument) 
            {
                // then we set CurrentDocumentation to selected document in the ComboBox
                CurrentDocumentation = selectedDocument;

                //Load document detatils into the controls

                // Load the start date of the selected document into the DateTimePicker control
                Doc_StartDate_DateTimePicker.Value = selectedDocument.DocStartDate;

                // Load the end date of the selected document into the DateTimePicker control
                Doc_EndDate_DateTimePicker.Value = selectedDocument.DocEndDate;

                // Set the SelectedItem of Doc_PickType_ComboBox to match the type of the selected document
                // This will display the document's type in the Doc_PickType_ComboBox
                Doc_PickType_ComboBox.SelectedItem = selectedDocument.DocType;


            }
            else 
            {
                // If no document is selected or the selection is invalid, set CurrentDocumentation to null
                // This handles the case where the selection is cleared or an invalid item is selected
                CurrentDocumentation = null;
            } 
        }






        #endregion

        #region ComboBoxMethods

        // Method to call all our comboxs methods in one go at start up
        private void ComboBoxStartUp()
        {
            //Calls the start SetUp methods for the ComboBoxes
            FillStudentComboBox();
            ComboBoxDocType();



        }

        // Method to populate or PickStudent ComboBox
        private void FillStudentComboBox()
        {
            // creates a list of Students that calls our RetrieveAllStudents() method, to retrive all students so we can populate or PickStudent ComboBox 
            List<Student> students = RetrieveAllStudents();

            // First we set the Combox´s ItemSource to the list of Student Objects
            // This tells the ComboBox what collection of items it should display (in this case our newly created list of students above)
            Doc_PickStudent_ComboBox.ItemsSource = students;

            // Then set the DisplayMemberPath to our property "StuFullName", to display students full names

            Doc_PickStudent_ComboBox.DisplayMemberPath = "StuFullName";

        }

        
        // Method to populate our DocType ComboBox
        private void ComboBoxDocType()
        {
            // Assign the enum values we made in our Documentation class, of DocTypeEnum as the data source for Doc_PickType_ComboBox
            // Enum.GetValues fetches all values from the Documentation.DocTypeEnum enumeration
            // and uses them to populate the ComboBox
            Doc_PickType_ComboBox.ItemsSource = Enum.GetValues(typeof(Documentation.DocTypeEnum));

          
        }


        #endregion

        #endregion



        #region Database

        #region RetrieveStudent

        //Method to retrieve data from a single student
        public void RetrieveStudent(int dbRowNum)
        {
            // Establish a connection to the database using the connection string
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens the database connection
                con.Open();

                
                // Create a SQL command to select a specific student based on row number
                // The query orders students by first name and uses OFFSET and FETCH to retrieve a single row
                SqlCommand stu = new SqlCommand("SELECT PK_StuCPR, StuFirstName, StuLastName, StuPhone, StuEmail FROM VK_Students ORDER BY StuFirstName ASC OFFSET @dbRowNum ROWS FETCH NEXT 1 ROW ONLY", con);

                // Ensure dbRowNum is not negative; if it is, reset it to 0
                if (dbRowNum < 0)
                {
                    dbRowNum = 0;
                }

                // Add dbRowNum as a parameter to the SQL command and gives @dbRowNum the value of dbRowNum
                stu.Parameters.AddWithValue("@dbRowNum", dbRowNum);

                //Execute the command 
                //Set up a SqlDataReader called dr, which reads the data from cmd
                using (SqlDataReader dr = stu.ExecuteReader())
                {
                    //While-loop running while dr is reading and loops through the result set
                    while (dr.Read())
                    {
                        
                        // Create a new Student object and populate it with data from the current row
                        studentToBeRetrievedStu = new Student ("", "", "", "", "", "")
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

            // Return the list of students
            return students;
        }

        #endregion

        #region RetrieveDocumentsForSeletedStudent

        // Method to retrieve all documents belonging to a single student, identified by stuCPR (needed in combobox selection changed Pick Student)
        public List<Documentation> RetrieveDocument(string stuCPR)
        {
            // Create a Documentation list to hold the retrieved documents
            List<Documentation> documents = new List<Documentation>();

            // Establish a database connection using the connection string
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                // Open the database connection
                con.Open();

                // Prepare a SQL command to retrieve documents where the foreign key matches the provided student CPR
                SqlCommand cmd = new SqlCommand("SELECT PK_DocID, DocStartDate, DocEndDate, DocType, FK_StuCPR, DocumentationFile FROM VK_Documentations WHERE FK_StuCPR = @StuCPR ORDER BY PK_DocID ASC", con);

                // Add stuCPR as a parameter to the SQL command to avoid SQL injection
                cmd.Parameters.AddWithValue("@StuCPR", stuCPR);

                // Local function to parse the DocType enum from the database value, so we can populate it with data from the from the database later
                Documentation.DocTypeEnum ParseDocTypeEnum(object docTypeValue)
                {
                    if (docTypeValue != DBNull.Value && Enum.TryParse(docTypeValue.ToString(), out Documentation.DocTypeEnum docType))
                    {
                        return docType;
                    }
                    else
                    {
                        return default; // Returns a default value for our enum if parsing fails
                    }
                }


                // Execute the command and use SqlDataReader to read the results
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    // Iterate through each record in the result set
                    while (dr.Read())
                    {
                        // Create a new Documentation object and populate it with data from the current row
                        Documentation document = new Documentation
                        {
                            DocId = int.Parse(dr["PK_DocID"].ToString()),
                            DocStartDate = dr["DocStartDate"] != DBNull.Value ? (DateTime)dr["DocStartDate"] : default(DateTime),
                            DocEndDate = dr["DocEndDate"] != DBNull.Value ? (DateTime)dr["DocEndDate"] : default(DateTime),
                            DocType = ParseDocTypeEnum(dr["DocType"])
                        };

                        // Add the populated document to the documents list
                        documents.Add(document);
                    }
                }
            }

            // Return the list of documents
            return documents;
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
