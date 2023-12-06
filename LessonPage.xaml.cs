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
            LockInputFields();
            //Call the ListBoxFunction method which create the ListBoxItems for your ListBox
            ListBoxFunction();

            ComboBoxFunction();

            Les_Save_Button.IsEnabled = false;
            Les_Edit_Button.IsEnabled = false;
            Les_Delete_Button.IsEnabled = false;
        }

        public Lesson CurrentLesson = new Lesson();
        Lesson lesToBeRetrieved;
        bool edit = false;
        int currentItem;

         //Buttons
        private void Les_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            UnlockInputFields();
            Les_Save_Button.IsEnabled = true;
            Les_Edit_Button.IsEnabled= false;
            Les_Delete_Button.IsEnabled= false;
        }

        private void Les_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            Les_DisName_TextBlock.Text = "Modul Navn: " + Les_Name_TextBox.Text;
            Les_DisType_TextBlock.Text = "Kørekorts Type: " + Les_Type_ComboBox.Text;
            Les_DisDescription_TextBlock.Text = "Beskrivelse: " + Les_Description_TextBox.Text;

            CurrentLesson.LesName = Les_Name_TextBox.Text;
            CurrentLesson.LesType = Les_Type_ComboBox.Text;
            CurrentLesson.LesDescription = Les_Description_TextBox.Text;

            if(edit == false ) { SaveNewLesson(CurrentLesson); }
            else { UpdateLesson(CurrentLesson); }

            ClearInputFields();
            LockInputFields();
            ListBoxFunction();

            edit = false;

            Les_Name_TextBox.Text = "Modul ";
            Les_Type_ComboBox.SelectedIndex = 0;
            Les_Description_TextBox.Text = "Læringsmål ";

            Les_Add_Button.IsEnabled = true;
            Les_Save_Button.IsEnabled = false;
            Les_Edit_Button.IsEnabled = false;
            Les_Delete_Button.IsEnabled = false;
        }

        private void Les_Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            UnlockInputFields();
            edit = true;
            CurrentLesson.LesId = currentItem;
            Les_Name_TextBox.Text = (Les_DisLes_ListBox.SelectedItem as LesListBoxItems).Name;
            Les_Type_ComboBox.Text = (Les_DisLes_ListBox.SelectedItem as LesListBoxItems).Type;
            Les_Description_TextBox.Text = (Les_DisLes_ListBox.SelectedItem as LesListBoxItems).Description;

            Les_Add_Button.IsEnabled=false;
            Les_Save_Button.IsEnabled = true;
            Les_Edit_Button.IsEnabled = false;

        }

        private void Les_Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            //ClearInputFields();
            /*
            Les_Add_Button.IsEnabled = true;
            Les_Save_Button.IsEnabled = false;
            Les_Edit_Button.IsEnabled = false;
            Les_Delete_Button.IsEnabled = false;
            */
        }

        #region ListBox
        //What happens when you select an item in the ListBox
        private void Les_DisLes_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Safety check, to make sure that the selected item exists
            if(Les_DisLes_ListBox.SelectedItem != null)
            {
                //Everything inside of the if-statement will likely have to be personalised 

                //Changes the text from the display window 
                //After the equal sign; (#ListBoxName.SelectedItem as #itemClass).#attribute;
                //The parts after a #, are the parts that needs to change based on your page
                Les_DisName_TextBlock.Text = "Modul Navn: " + (Les_DisLes_ListBox.SelectedItem as LesListBoxItems).Name;
                Les_DisType_TextBlock.Text = "Kørekorts Type: " + (Les_DisLes_ListBox.SelectedItem as LesListBoxItems).Type;
                Les_DisDescription_TextBlock.Text = "Beskrivelse: " + (Les_DisLes_ListBox.SelectedItem as LesListBoxItems).Description;
                
                currentItem = (Les_DisLes_ListBox.SelectedItem as LesListBoxItems).Id;
                
                Les_Save_Button.IsEnabled = false;
                Les_Edit_Button.IsEnabled = true;
                Les_Delete_Button.IsEnabled = true;
            }
        }

        //Method to create, control and add items to the ListBox
        private void ListBoxFunction()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_LesID) from VK_Lessons", con);
                int intCount = (int)count.ExecuteScalar();

                //Make a list with the Item Class from below called items (Name doesn't matter)
                //LesListBoxItems in my case
                List<LesListBoxItems> items = new List<LesListBoxItems>();

                for (int i = 0; i < intCount; i++)
                {
                    RetrieveLessonData(i);
                    //Add new items from the item class with specific attributes to the list
                    items.Add(new LesListBoxItems() { Id = lesToBeRetrieved.LesId, Name = lesToBeRetrieved.LesName, Type = lesToBeRetrieved.LesType, Description = lesToBeRetrieved.LesDescription });
                    
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
        public class LesListBoxItems
        {
            //The attributes of the items for the ListBox
            public int Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }

            //Extra attribute, used for visuals (Only needed for multi-attribute views)
            //public string SetUp { get; set; }
        }
        #endregion

        #region ComboBox
        private void ComboBoxFunction()
        {
            //New list and datapoints for Combobox
            List<LesComboBoxType> types = new List<LesComboBoxType>();
            types.Add(new LesComboBoxType { LicenseType = "B", DisplayValue = "Kørekort B" });
            types.Add(new LesComboBoxType { LicenseType = "A", DisplayValue = "Kørekort A" });
            types.Add(new LesComboBoxType { LicenseType = "A1", DisplayValue = "Kørekort A1" });
            types.Add(new LesComboBoxType { LicenseType = "A2", DisplayValue = "Kørekort A2" });

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
            public string LicenseType { get; set; }
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
        #endregion

        #region Database
        public void SaveNewLesson(Lesson lesToBeCreated)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO VK_Lessons (LesName, LesType, LesDescription)" +
                                                 "VALUES(@LesName,@LesType,@LesDescription)" +
                                                 "SELECT @@IDENTITY", con);
                cmd.Parameters.Add("@LesName", SqlDbType.NVarChar).Value = lesToBeCreated.LesName;
                cmd.Parameters.Add("@LesType", SqlDbType.NVarChar).Value = lesToBeCreated.LesType;
                cmd.Parameters.Add("@LesDescription", SqlDbType.NVarChar).Value = lesToBeCreated.LesDescription;
                lesToBeCreated.LesId = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void RetrieveLessonData(int index)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT PK_LesID, LesName, LesType, LesDescription FROM VK_Lessons ORDER BY PK_LesID ASC OFFSET @Index ROWS FETCH NEXT 1 ROW ONLY", con);

                if (index < 0)
                {
                    index = 0;
                }
                cmd.Parameters.AddWithValue("@Index", index);
                
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lesToBeRetrieved = new Lesson(0, "", "", "")
                        {
                            LesId = int.Parse(dr["PK_LesID"].ToString()),
                            LesName = dr["LesName"].ToString(),
                            LesType = dr["LesType"].ToString(),
                            LesDescription = dr["LesDescription"].ToString(),
                        };
                    }
                }
            }
        }

        public void UpdateLesson(Lesson lesToBeUpdated)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE VK_Lessons SET LesName = @LesName, LesType = @LesType, LesDescription = @LesDescription WHERE PK_LesID = @LesId", con);
                cmd.Parameters.AddWithValue("@LesName", lesToBeUpdated.LesName);
                cmd.Parameters.AddWithValue("@LesType", lesToBeUpdated.LesType);
                cmd.Parameters.AddWithValue("@LesDescription", lesToBeUpdated.LesDescription);
                cmd.Parameters.AddWithValue("@LesId", lesToBeUpdated.LesId);
                cmd.ExecuteNonQuery();
            }
        }
        #endregion
    }
}
