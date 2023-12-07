using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;


namespace VindegadeKS_WPF
{
    /// <summary>
    /// Interaction logic for LessonPage.xaml
    /// </summary>
    public partial class LessonPage : Page
    {
        public LessonPage()
        {
            InitializeComponent();

            //Locks the input fields, so the user can't interact before pressing a button
            LockInputFields();
            
            //Calls the ListBoxFunction method which create the ListBoxItems for your ListBox
            ListBoxFunction();

            //Calls the ComboBoxFunction method which sets up the ComboBox on your page 
            //If you have multiple ComboBoxes on your page, each method will have its own name
            ComboBoxFunction();

            //Disables the buttons which aren't relevant yet
            Les_Save_Button.IsEnabled = false;
            Les_Edit_Button.IsEnabled = false;
            Les_Delete_Button.IsEnabled = false;
        }

        //Create CurrentLesson to contain current object - Needed in: Save_Button_Click & Edit_Button_Click
        VindegadeKS_WPF.Lesson CurrentLesson = new VindegadeKS_WPF.Lesson();
        //Moved out here instead of staying in 'Retrieve', so ListBoxFunction can access - Needed in: ListBoxFunction & RetrieveData
        VindegadeKS_WPF.Lesson lesToBeRetrieved;
        //Keeps track of if CurrentLesson is a new object or an old one being edited - Needed in: Add_Button_Click, Save_Button_Click, Edit_Button_Click & ListBox_SelectionChanged
        bool edit = false;
        //Keeps track of the id of ListBoxItem while it's selected - Edit_Button_Click & ListBox_SelectionChanged
        int currentItem;

        #region  Buttons
        //Enableds user to add new Lesson
        private void Les_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            //Unlocks the input fields
            UnlockInputFields();

            //Sets edit to false, as it is impossible for it to be true currently
            edit = false;

            //Controls which button the user can interact with - User needs to save, but shouldn't interact with Edit/Delete as Add is adding a new Lesson
            Les_Save_Button.IsEnabled = true;
            Les_Edit_Button.IsEnabled= false;
            Les_Delete_Button.IsEnabled= false;
        }

        //Saves the Lesson from the input fields
        private void Les_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            //Updates the displaypanel to reflect the newly saved Lesson
            Les_DisName_TextBlock.Text = "Modul Navn: " + Les_Name_TextBox.Text;
            Les_DisType_TextBlock.Text = "Modul Type: " + Les_Type_ComboBox.Text;
            Les_DisDescription_TextBlock.Text = "Modul Beskrivelse: " + Les_Description_TextBox.Text;

            //Sets CurrentLesson equal to the input fields
            CurrentLesson.LesName = Les_Name_TextBox.Text;
            CurrentLesson.LesType = Les_Type_ComboBox.Text;
            CurrentLesson.LesDescription = Les_Description_TextBox.Text;

            //If-statement checks if CurrentLesson is a new Lesson or an old Lesson being edited, by checking the edit-bool
            //If it's not being edited run SaveNewLesson(CurrentLesson), else UpdateLesson(CurrentLesson)
            if (edit == false ) { SaveNewLesson(CurrentLesson); }
            else { EditLesson(CurrentLesson); }

            //Lock the input fields and rerun ListBoxFunction to make sure it has all items with correct info
            LockInputFields();
            ListBoxFunction();

            //Sets edit to false, as it is impossible for it to be true currently
            edit = false;

            //Sets the default text in input fields to minimize user effort 
            Les_Name_TextBox.Text = "Modul ";
            Les_Type_ComboBox.SelectedIndex = 0;
            Les_Description_TextBox.Text = "Læringsmål ";

            //Controls which button the user can interact with - User needs to be able to Add more Lessons, but nothing else
            Les_Add_Button.IsEnabled = true;
            Les_Save_Button.IsEnabled = false;
            Les_Edit_Button.IsEnabled = false;
            Les_Delete_Button.IsEnabled = false;
        }

        //Lets the user edit previously created Lessons
        private void Les_Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            //Unlocks the input fields
            UnlockInputFields();

            //Sets edit to true, as the user is currently editing the Lesson
            edit = true;

            //Sets CurrentLessons ID to currentItem
            CurrentLesson.LesId = currentItem;

            //Sets the input fields to equal the info from the ListBoxItems
            Les_Name_TextBox.Text = (Les_DisLes_ListBox.SelectedItem as Lesson).LesName;
            Les_Type_ComboBox.Text = (Les_DisLes_ListBox.SelectedItem as Lesson).LesType;
            Les_Description_TextBox.Text = (Les_DisLes_ListBox.SelectedItem as Lesson).LesDescription;

            //Controls which button the user can interact with - User needs able to save, but nothing else
            Les_Add_Button.IsEnabled = false;
            Les_Save_Button.IsEnabled = true;
            Les_Edit_Button.IsEnabled = false;
            Les_Delete_Button.IsEnabled = false;

        }

        //Lets the user delete previously created Lessons - More details in InstructorPage.xaml.cs
        private void Les_Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            //Sets CurrentLesson ID to the currently selected item
            CurrentLesson.LesId = currentItem;

            //Runs DeleteLesson while feeding it CurrentLesson ID
            DeleteLesson(CurrentLesson.LesId);
            ListBoxFunction();
            ClearInputFields();
            
            //Controls which button the user can interact with - User needs able to add new Lesson, but nothing else
            Les_Add_Button.IsEnabled = true;
            Les_Save_Button.IsEnabled = false;
            Les_Edit_Button.IsEnabled = false;
            Les_Delete_Button.IsEnabled = false;
            
        }
        #endregion

        #region ListBox
        //Controls what happens when you select an item in the ListBox
        private void Les_DisLes_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Safety check, to make sure that the selected item exists
            if(Les_DisLes_ListBox.SelectedItem != null)
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

        //Class to define the content of the ListBoxItems for the ListBox
        /*public class Lesson
        {
            //The attributes of the items for the ListBox
            public int Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }

            //Extra attribute, used for visuals (Only needed for multi-attribute views)
            //public string SetUp { get; set; }
        }*/
        #endregion

        #region ComboBox
        //Setup the ComboBox
        private void ComboBoxFunction()
        {
            //New list and datapoints for Combobox
            List<LesComboBoxType> types = new List<LesComboBoxType>();
            types.Add(new LesComboBoxType { TypeInDatabase = "Teori", DisplayValue = "Teorisk modul" });
            types.Add(new LesComboBoxType { TypeInDatabase = "Praktisk_Hold", DisplayValue = "Praktisk m/ hold" });
            types.Add(new LesComboBoxType { TypeInDatabase = "Praktisk_Solo", DisplayValue = "Praktisk m/ individuel studerende" });

            //Set the ItemsSource
            Les_Type_ComboBox.ItemsSource = types;
            //Sets which attribute is displayed
            Les_Type_ComboBox.DisplayMemberPath = "DisplayValue";
            //Sets default choice
            Les_Type_ComboBox.SelectedIndex = 0;
        }

        //Class which defines the ComboBox Data
        public class LesComboBoxType
        {
            //The attributes of the ComboBoxItems for the ComboBox
            public string TypeInDatabase { get; set; }
            public string DisplayValue { get; set; }
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

        //Sets the input fields to equal the info from the ListBoxItems
        //After the equal sign; (#ListBoxName.SelectedItem as #itemClass).#attribute;
        //The parts after a #, are the parts that needs to change based on your page
        private void UpdateDisplaypanel()
        {
            Les_Name_TextBox.Text = (Les_DisLes_ListBox.SelectedItem as Lesson).LesName;
            Les_Type_ComboBox.Text = (Les_DisLes_ListBox.SelectedItem as Lesson).LesType;
            Les_Description_TextBox.Text = (Les_DisLes_ListBox.SelectedItem as Lesson).LesDescription;
        }
        #endregion

        #region Database
        //Create new row in the database from lesToBeCreated
        public void SaveNewLesson(VindegadeKS_WPF.Lesson lesToBeCreated)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();
                //Creates a cmd SqlCommand, which enableds the ability to INSERT INTO the table with the corresponding attributes 
                SqlCommand cmd = new SqlCommand("INSERT INTO VK_Lessons (LesName, LesType, LesDescription)" +
                                                 "VALUES(@LesName,@LesType,@LesDescription)" +
                                                 "SELECT @@IDENTITY", con);

                //Add corresponding attribute to the database through the use of cmd
                cmd.Parameters.Add("@LesName", SqlDbType.NVarChar).Value = lesToBeCreated.LesName;
                cmd.Parameters.Add("@LesType", SqlDbType.NVarChar).Value = lesToBeCreated.LesType;
                cmd.Parameters.Add("@LesDescription", SqlDbType.NVarChar).Value = lesToBeCreated.LesDescription;
                //Tells the database to execute the sql commands and ansign an int to LesId and set lesToBeCreated.LesId to said ID
                lesToBeCreated.LesId = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        //Retrieves the data of a specific row in the database where the row number is equal to dbRowNum + 1
        public void RetrieveLessonData(int dbRowNum)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();
                //Creates a cmd SqlCommand, which SELECTs a specific row 
                SqlCommand cmd = new SqlCommand("SELECT PK_LesID, LesName, LesType, LesDescription FROM VK_Lessons ORDER BY PK_LesID ASC OFFSET @dbRowNum ROWS FETCH NEXT 1 ROW ONLY", con);

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
                        lesToBeRetrieved = new VindegadeKS_WPF.Lesson(0, "", "", "")
                        {
                            //Sets the attributes of lesToBeRetrieved equal to the data from the current row of the database
                            LesId = int.Parse(dr["PK_LesID"].ToString()),
                            LesName = dr["LesName"].ToString(),
                            LesType = dr["LesType"].ToString(),
                            LesDescription = dr["LesDescription"].ToString(),
                        };
                    }
                }
            }
        }

        //Edits the data of a previously existing Lesson
        public void EditLesson(VindegadeKS_WPF.Lesson lesToBeUpdated)
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

        //Deletes the selected item from the database
        public void DeleteLesson(int lesToBeDeleted)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();
                
                //Creates a cmd SqlCommand, which DELETEs a specific row in the table, based on the LesId
                SqlCommand cmd = new SqlCommand("DELETE FROM VK_Lessons WHERE PK_LesId = @PK_LesId", con);

                //Gives @PK_LesId the value of lesToBeDeleted
                cmd.Parameters.AddWithValue("@PK_LesId", lesToBeDeleted);
                
                //Tells the database to execute the cmd sql command
                cmd.ExecuteScalar();
            }
        }
        #endregion
    }
}