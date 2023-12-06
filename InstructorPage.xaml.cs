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


namespace VindegadeKS_WPF
{
    /// <summary>
    /// Interaction logic for InstructorPage.xaml
    /// </summary>
    public partial class InstructorPage : Page
    {
        public InstructorPage()
        {
            InitializeComponent();
            LockInputFields();
            ListBoxFunction();
        }

        public Instructor CurrentInstructor = new Instructor();
        Instructor instructorToBeRetrieved;
        bool edit = false;
        int currentItem;


        private void Inst_DisInst_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Inst_DisInst_ListBox.SelectedItem != null)
            {
                currentItem = (Inst_DisInst_ListBox.SelectedItem as InstListBoxItems).Id;
            }
        }

        private void Inst_Create_Button_Click(object sender, RoutedEventArgs e)
        {
            UnlockInputFields();
        }

        private void Inst_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            CurrentInstructor.InstFirstName = Inst_FirstName_TextBox.Text;
            CurrentInstructor.InstLastName = Inst_LastName_TextBox.Text;
            CurrentInstructor.InstPhone = Inst_Phone_TextBox.Text;
            CurrentInstructor.InstEmail = Inst_Email_TextBox.Text;

            if (edit == false) { SaveInstructor(CurrentInstructor); }
            else { EditInstructor(CurrentInstructor); }

            ClearInputFields();
            LockInputFields();
            ListBoxFunction();

            edit = false;
        }

        private void Inst_Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            UnlockInputFields();
            edit = true;
            CurrentInstructor.InstId = currentItem;
            Inst_FirstName_TextBox.Text = (Inst_DisInst_ListBox.SelectedItem as InstListBoxItems).FirstName;
            Inst_LastName_TextBox.Text = (Inst_DisInst_ListBox.SelectedItem as InstListBoxItems).LastName;
            Inst_Phone_TextBox.Text = (Inst_DisInst_ListBox.SelectedItem as InstListBoxItems).Phone;
            Inst_Email_TextBox.Text = (Inst_DisInst_ListBox.SelectedItem as InstListBoxItems).Email;
        }

        private void Inst_Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            CurrentInstructor.InstId = currentItem;
            DeleteInstructor(CurrentInstructor.InstId);
            ListBoxFunction();
        }

        public void SaveInstructor(Instructor instructorToBeCreated)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO VK_Instructors (InstFirstName, InstLastName, InstPhone, InstEmail)" +
                                                 "VALUES(@InstFirstName,@InstLastName,@InstPhone,@InstEmail)" +
                                                 "SELECT @@IDENTITY", con);
                cmd.Parameters.Add("@InstFirstName", SqlDbType.NVarChar).Value = instructorToBeCreated.InstFirstName;
                cmd.Parameters.Add("@InstLastName", SqlDbType.NVarChar).Value = instructorToBeCreated.InstLastName;
                cmd.Parameters.Add("@InstPhone", SqlDbType.NVarChar).Value = instructorToBeCreated.InstPhone;
                cmd.Parameters.Add("@InstEmail", SqlDbType.NVarChar).Value = instructorToBeCreated.InstEmail;
                instructorToBeCreated.InstId = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void EditInstructor(Instructor instructorToBeUpdated)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE VK_Instructors SET InstFirstName = @InstFirstName, InstLastName = @InstLastName, InstPhone = @InstPhone, InstEmail = @InstEmail WHERE PK_InstID = @InstId", con);
                cmd.Parameters.AddWithValue("@InstFirstName", instructorToBeUpdated.InstFirstName);
                cmd.Parameters.AddWithValue("@InstLastName", instructorToBeUpdated.InstLastName);
                cmd.Parameters.AddWithValue("@InstPhone", instructorToBeUpdated.InstPhone);
                cmd.Parameters.AddWithValue("@InstEmail", instructorToBeUpdated.InstEmail);
                cmd.Parameters.AddWithValue("@InstId", instructorToBeUpdated.InstId);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteInstructor(int instructorIdToBeDeleted)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM VK_Instructors WHERE PK_InstId = @PK_InstId", con);
                cmd.Parameters.AddWithValue("@PK_InstId", instructorIdToBeDeleted);
                cmd.ExecuteScalar();
            }

            ClearInputFields();
        }

        public void RetrieveInstructorData(int dBRowNumber)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT PK_InstID, InstFirstName, InstLastName, InstPhone, InstEmail FROM VK_Instructors ORDER BY PK_InstID ASC OFFSET @dBRowNumber ROWS FETCH NEXT 1 ROW ONLY", con);

                if (dBRowNumber < 0)
                {
                    dBRowNumber = 0;
                }
                cmd.Parameters.AddWithValue("@dBRowNumber", dBRowNumber);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        instructorToBeRetrieved = new Instructor(0, "", "", "", "")
                        {
                            InstId = int.Parse(dr["PK_InstID"].ToString()),
                            InstFirstName = dr["InstFirstName"].ToString(),
                            InstLastName = dr["InstLastName"].ToString(),
                            InstPhone = dr["InstPhone"].ToString(),
                            InstEmail = dr["InstEmail"].ToString(),
                        };
                    }
                }
            }
        }

        public class InstListBoxItems
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Setup { get; set; }
        }

        private void ListBoxFunction()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_InstID) FROM VK_Instructors", con);
                int intCount = (int)count.ExecuteScalar();

                List<InstListBoxItems> items = new List<InstListBoxItems>();

                for (int i = 0; i < intCount; i++)
                {
                    RetrieveInstructorData(i);

                    items.Add(new InstListBoxItems()
                    {
                        Id = instructorToBeRetrieved.InstId,
                        FirstName = instructorToBeRetrieved.InstFirstName,
                        LastName = instructorToBeRetrieved.InstLastName,
                        Phone = instructorToBeRetrieved.InstPhone,
                        Email = instructorToBeRetrieved.InstEmail
                    });

                    items[i].Setup = $"{items[i].FirstName} {items[i].LastName}\n{items[i].Phone}\n{items[i].Email}";
                }
                Inst_DisInst_ListBox.ItemsSource = items;
            }
        }

        // Clears inputfields after saving to DB
        private void ClearInputFields()
        {
            Inst_FirstName_TextBox.Clear();
            Inst_LastName_TextBox.Clear();
            Inst_Phone_TextBox.Clear();
            Inst_Email_TextBox.Clear();
        }

        //Locks all inputfields, so that they cannot be edited
        private void LockInputFields()
        {
            Inst_FirstName_TextBox.IsEnabled = false;
            Inst_LastName_TextBox.IsEnabled = false;
            Inst_Phone_TextBox.IsEnabled = false;
            Inst_Email_TextBox.IsEnabled = false;
        }

        //Unlocks all inputfields, so that they can be edited
        private void UnlockInputFields()
        {
            Inst_FirstName_TextBox.IsEnabled = true;
            Inst_LastName_TextBox.IsEnabled = true;
            Inst_Phone_TextBox.IsEnabled = true;
            Inst_Email_TextBox.IsEnabled = true;
        }
    }
}
