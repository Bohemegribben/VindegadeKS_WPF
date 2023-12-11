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

namespace VindegadeKS_WPF
{
    /// <summary>
    /// Interaction logic for StudentPage.xaml
    /// </summary>
    public partial class StudentPage : Page
    {
       

        public StudentPage()
        {
            InitializeComponent();
            this.DataContext = this; // Binder denne klasse til dens XAML view

            //Locks the input fields, so the user can't interact before pressing a button
            LockInputFields();

            //Calls the ListBoxFunction method which create the ListBoxItems for your ListBox
            ListBoxFunction();

            //Disables the buttons which aren't relevant yet
            Stu_Save_Button.IsEnabled = false;
            Stu_Edit_Button.IsEnabled = false;
            Stu_Delete_Button.IsEnabled = false;
        }

        // Create CurrentStudent to contain current object - Needed in: Save_Button_Click & Edit_Button_Click
        public Student CurrentStudent = new Student();

        // Moved out here instead of staying in 'Retrieve', so ListBoxFunction can access - Needed in: ListBoxFunction & RetrieveData
        public Student StudentToBeRetrieved;

        // Keeps track of if CurrentLesson is a new object or an old one being edited -
        // Needed in: Add_Button_Click, Save_Button_Click, Edit_Button_Click & ListBox_SelectionChanged
        bool edit = false;

        //Keeps track of the CPR of ListBoxItem while it's selected - Edit_Button_Click & ListBox_SelectionChanged
        string currentItem;



        #region Lock and Unlock inputFields

        // metode til at låse textboxe/input boxe fra start
        private void LockInputFields()
        {
            Stu_CPR_TextBox.IsEnabled = false;
            Stu_FirstName_TextBox.IsEnabled = false;
            Stu_LastName_TextBox.IsEnabled = false;
            Stu_Phone_TextBox.IsEnabled = false;
            Stu_Email_TextBox.IsEnabled = false;
        }

        // //Unlocks all inputfields, so that they can be edited
        private void UnlockInputFields()
        {
            Stu_CPR_TextBox.IsEnabled = true;
            Stu_FirstName_TextBox.IsEnabled = true;
            Stu_LastName_TextBox.IsEnabled = true;
            Stu_Phone_TextBox.IsEnabled = true;
            Stu_Email_TextBox.IsEnabled = true;
        }

        #endregion

        #region clear input

        private void ClearInputFields()    
        {
            Stu_CPR_TextBox.Clear();
            Stu_FirstName_TextBox.Clear();
            Stu_LastName_TextBox.Clear();
            Stu_Phone_TextBox.Clear();
            Stu_Email_TextBox?.Clear();
        }

        #endregion

        #region Search_Bar
        private void watermarkTxtBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //hvis man er trykket ind på "search bar textbox" skal "watermark textbox" ikke være synlig længere
            stu_txtBox_watermark.Visibility = Visibility.Collapsed;

            // og vores "search textbox skal være synlig
            stu_txtBox_search.Visibility = Visibility.Visible;
            stu_txtBox_search.Focus();

        }



        private void searchTxtBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Hvis search bar er tom
            if (string.IsNullOrEmpty(stu_txtBox_search.Text))
            {
                // så skal search textbox ikke være synlig
                stu_txtBox_search.Visibility = Visibility.Collapsed;

                // watermark textbox skal være synlig
                stu_txtBox_search.Visibility = Visibility.Visible;

            }
        }

        #endregion

        #region Buttons


        #region Add Button
        //Enableds user to add new Lesson
        private void Stu_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            //Unlocks the input fields
            UnlockInputFields();

            //Sets edit to false, as it is impossible for it to be true currently
            edit = false;

            //Controls which button the user can interact with - User needs to save, but shouldn't interact with Edit/Delete as Add is adding a new Lesson
            Stu_Save_Button.IsEnabled = true;
            Stu_Edit_Button.IsEnabled = false;
            Stu_Delete_Button.IsEnabled = false;
        }
        #endregion


        #region Save Button

        //Saves the Student from the input fields
        private void Stu_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            //Sets CurrentStudent equal to the input fields
            CurrentStudent.StuCPR = Stu_CPR_TextBox.Text;
            CurrentStudent.StuFirstName = Stu_FirstName_TextBox.Text;
            CurrentStudent.StuLastName = Stu_LastName_TextBox.Text;
            CurrentStudent.StuPhone = Stu_Phone_TextBox.Text;
            CurrentStudent.StuEmail = Stu_Email_TextBox.Text;

            //If-statement checks if CurrentStudent is a new Student or an old Student being edited, by checking the edit-bool
            //If it's not being edited run SaveNewStudent(CurrentStudent), else UpdateStudent(Student)
            if (edit == false) { SaveStudent(CurrentStudent); }
            else { EditStudent(CurrentStudent); }

            //Lock the input fields and rerun ListBoxFunction to make sure it has all items with correct info
            ClearInputFields();
            LockInputFields();
            ListBoxFunction();

            //Sets edit to false, as it is impossible for it to be true currently
            edit = false;

        }

        #endregion

        #region Edit Button
        //Lets the user edit previously created Students
        private void Stu_Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            //Unlocks the input fields
            UnlockInputFields();

            //Sets edit to true, as the user is currently editing the Lesson
            edit = true;

            //Sets CurrentStudent CPR to currentItem
            CurrentStudent.StuCPR = currentItem;

            //Sets the input fields to equal the info from the ListBoxItems
            Stu_CPR_TextBox.Text = (Stu_DisStu_ListBox.SelectedItem as StuListBoxItems).CPR;
            Stu_FirstName_TextBox.Text = (Stu_DisStu_ListBox.SelectedItem as StuListBoxItems).FirstName;
            Stu_LastName_TextBox.Text = (Stu_DisStu_ListBox.SelectedItem as StuListBoxItems).LastName;
            Stu_Phone_TextBox.Text = (Stu_DisStu_ListBox.SelectedItem as StuListBoxItems).Phone;
            Stu_Email_TextBox.Text = (Stu_DisStu_ListBox.SelectedItem as StuListBoxItems).Email;

            Stu_Save_Button.IsEnabled = true;
        }

        #endregion


        #region Delete Button
        //Lets the user delete previously created Lessons
        private void Stu_Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            CurrentStudent.StuCPR = currentItem;     // CurrentStudent.StuCPR sættes lig med currentItem,
                                                        // currentItem blev defineret da ListBox.SelectedItem
                                                        // blev valgt ved museklik

            DeleteStudent(CurrentStudent.StuCPR); // DeleteInstructor kaldes med argumentet CurrentStudent.StuCPR,
                                                        // CurrentStudent.StuCPR bruges til at finde database entity
                                                        // med tilsvarende StuCPR via DeleteStudent-metoden

            ListBoxFunction();                          // ListBoxFunction kaldes for at opdatere listboxens indhold
                                                        // efter sletningen er udført
        }
        #endregion

        #endregion

        #region Button methods

   

        #region SaveStudent

        // Save Student metode
        public void SaveStudent (Student studentToBeCreated) 
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO VK_Students (PK_StuCPR, StuFirstName, StuLastName, StuPhone, StuEmail)" +
                                                 "VALUES(@StuCPR, @StuFirstName,@StuLastName,@StuPhone,@StuEmail)" +
                                                 "SELECT @@IDENTITY", con);
                cmd.Parameters.Add("@StuCPR", SqlDbType.NVarChar).Value = studentToBeCreated.StuCPR;
                cmd.Parameters.Add("@StuFirstName", SqlDbType.NVarChar).Value = studentToBeCreated.StuFirstName;
                cmd.Parameters.Add("@StuLastName", SqlDbType.NVarChar).Value = studentToBeCreated.StuLastName;
                cmd.Parameters.Add("@StuPhone", SqlDbType.NVarChar).Value = studentToBeCreated.StuPhone;
                cmd.Parameters.Add("@StuEmail", SqlDbType.NVarChar).Value = studentToBeCreated.StuEmail;

                cmd.ExecuteScalar();
               
            }
        }
        #endregion

        #region EditStudent
        // Edit Student metode
        public void EditStudent(Student StudentToBeUpdated)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE VK_Students SET PK_StuCPR = @StuCPR, StuFirstName = @StuFirstName, StuLastName = @StuLastName, StuPhone = @StuPhone, StuEmail = @StuEmail WHERE PK_StuCPR = @StuCPR", con);
                cmd.Parameters.AddWithValue("@StuCPR", StudentToBeUpdated.StuCPR);
                cmd.Parameters.AddWithValue("@StuFirstName", StudentToBeUpdated.StuFirstName);
                cmd.Parameters.AddWithValue("@StuLastName", StudentToBeUpdated.StuLastName);
                cmd.Parameters.AddWithValue("@StuPhone", StudentToBeUpdated.StuPhone);
                cmd.Parameters.AddWithValue("@StuEmail", StudentToBeUpdated.StuEmail);
               
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region RetrieveSudentData
        public void RetrieveStudentData(int dBRowNumber)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT PK_StuCPR, StuFirstName, StuLastName, StuPhone, StuEmail FROM VK_Students ORDER BY PK_StuCPR ASC OFFSET @dBRowNumber ROWS FETCH NEXT 1 ROW ONLY", con);

                if (dBRowNumber < 0)
                {
                    dBRowNumber = 0;
                }
                cmd.Parameters.AddWithValue("@dBRowNumber", dBRowNumber);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        StudentToBeRetrieved = new Student("", "", "", "", "")
                        {
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

        #region DeleteStudent

       public void DeleteStudent(string StudentCPRToBeDeleted) // DeleteStudent - metoden defineres med parameteren string StudentCPRToBeDeleted,
                                                                  // Metoden tager CurrentStudent.StuCPR (som har referencesemantisk lighed med currentItem)
                                                                  // som argument, når den kaldes
        {
            // Sql-Connection definerer forbindelsen 'con' til databasen
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
               
                /*con.Open(); // 'Open' åbner forbindelsen 'con' til databasen
                 SqlCommand cmd = new SqlCommand("DELETE VK_Class_Student, VK_Students FROM VK_Class_Student JOIN VK_Students ON VK_Class_Student.CK_StuCPR = VK_Students.PK_StuCPR WHERE VK_Class_Student.CK_StuCPR = @CK_StuCPR", con); // SqlCommand definerer Sql-query-indholdet
                                                                                                                                                                                                                                     // (en DELETE-kommando rettet mod en specifik
                                                                                                                                                                                                                                     // tabel i databasen) af 'cmd', som skal
                                                                                                                                                                                                                                     // sendes via forbindelsen 'con'

                 cmd.Parameters.AddWithValue("@PK_StuCPR", StudentCPRToBeDeleted); // cmd.Parameters.AddWithValue sætter en SQL-variabel (@PK_StuCPR) lig
                                                                                     // med parameteren 'StudentCPRToBeDeleted', der får sit argument, når
                                                                                     // metoden bliver kaldt
                 cmd.ExecuteScalar(); // ExecuteScalar-metoden kører kommandoen cmd
                */


                con.Open();

                // Delete from VK_Class_Student where there´s dependencies first to avoid the reference constraint
                SqlCommand cmd2 = new SqlCommand("DELETE FROM VK_Class_Student WHERE CK_StuCPR = @CK_StuCPR", con);
                cmd2.Parameters.AddWithValue("@CK_StuCPR", StudentCPRToBeDeleted);
                cmd2.ExecuteScalar();

                // Delete from VK_Students
                SqlCommand cmd1 = new SqlCommand("DELETE FROM VK_Students WHERE PK_StuCPR = @PK_StuCPR", con);
                cmd1.Parameters.AddWithValue("@PK_StuCPR", StudentCPRToBeDeleted);
                cmd1.ExecuteScalar(); // Use ExecuteNonQuery for DELETE, INSERT, UPDATE statements
                

               
            }

            ClearInputFields(); // Input-felterne cleares for at indikere, at sletningen er gennemført
           
        } 

        #endregion



        #endregion

        #region ListBoxFunctions


        public class StuListBoxItems
        {
            public string CPR { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Setup { get; set; }
        }

        #region ListBoxFunction

        // Metode til at indsætte Elever på vores liste
        private void ListBoxFunction()
        {
          
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates a count SqlCommand, which gets the number of rows in the table 
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_StuCPR) from VK_Students", con);
                //Saves count command result to int
                int intCount = (int)count.ExecuteScalar();

                //Make a list with the Item Class from below called items (Name doesn't matter)
                //LesListBoxItems in my case
                List<StuListBoxItems> items = new List<StuListBoxItems>();

                //Forloop which adds intCount number of new items to items-list
                for (int i = 0; i < intCount; i++)
                {
                    //Calls RetrieveLessonData method, sending i as index
                    RetrieveStudentData(i);

                    //Adds a new item from the item class with specific attributes to the list
                    //The data added comes from RetrieveLessonData
                    items.Add(new StuListBoxItems() { CPR = StudentToBeRetrieved.StuCPR, FirstName = StudentToBeRetrieved.StuFirstName, LastName = StudentToBeRetrieved.StuLastName, Phone = StudentToBeRetrieved.StuPhone, Email = StudentToBeRetrieved.StuEmail });

                    //Only necessary for multi-attribute ListBoxItem
                    //Set up the attribute 'SetUp' which is used to determine the appearance of the ListBoxItem 
                    //Mine isn't, so it's out commented
                    //items[i].SetUp = $"{items[i].Name}\n{items[i].Type}\n{items[i].Description}";
                }

                //Set the ItemsSource to the list, so that the ListBox uses the list to make the ListBoxItems
                Stu_DisStu_ListBox.ItemsSource = items;
            }

        }
        #endregion

        #region SelectionChanged
        private void studentsListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (Stu_DisStu_ListBox.SelectedItem != null)
            {
                // currentItem = (Stu_DisStu_ListBox.SelectedItem as StuListBoxItems).CPR;


                //Safety check, to make sure that the selected item exists
                if (Stu_DisStu_ListBox.SelectedItem != null)
                {


                    //Changes the text from the display window 
                    //After the equal sign; (#ListBoxName.SelectedItem as #itemClass).#attribute;
                    //The parts after a #, are the parts that needs to change based on your page

                    Stu_DisCPR_TextBlock.Text = "CPR: " + (Stu_DisStu_ListBox.SelectedItem as StuListBoxItems).CPR;
                    Stu_DisFirstName_TextBlock.Text = "Fornavn: " + (Stu_DisStu_ListBox.SelectedItem as StuListBoxItems).FirstName;
                    Stu_DisLastName_TextBlock.Text = "Efternavn: " + (Stu_DisStu_ListBox.SelectedItem as StuListBoxItems).LastName;
                    Stu_DisPhone_TextBlock.Text = "Telefon: " + (Stu_DisStu_ListBox.SelectedItem as StuListBoxItems).Phone;
                    Stu_DisEmail_TextBlock.Text = "Email: " + (Stu_DisStu_ListBox.SelectedItem as StuListBoxItems).Email;

                    //Sets currentItem to equal the CPR of selected item
                    currentItem = (Stu_DisStu_ListBox.SelectedItem as StuListBoxItems).CPR;

                    //Sets edit to false, as it is impossible for it to be true currently
                    edit = false;

                    //Controls which button the user can interact with - User needs able to edit and delete, but not save
                    Stu_Save_Button.IsEnabled = false;
                    Stu_Edit_Button.IsEnabled = true;
                    Stu_Delete_Button.IsEnabled = true;
                }
            }

        }
        #endregion

        #endregion


    }
}
