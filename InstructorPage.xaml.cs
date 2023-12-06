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
            RetrieveInstructorData(0);
            LockInputFields();
        }

        public Instructor CurrentInstructor = new Instructor();

        //Method to create, control and add instructors to the ListBox
        private void AddInstructors()
        {
            //Make a list with the Item Class from below called items (Name doesn't matter)
            //LesListBoxItems in my case
            List<InstListBoxInstructors> instructors = new List<InstListBoxInstructors>();

            //Add new items from the item class with specific attributes to the list
            //Will later be remade to automatically add items based on the database
            instructors.Add(new InstListBoxInstructors() { FirstName = "A", LastName = "A", Phone = "A", Email = "A" });
            instructors.Add(new InstListBoxInstructors() { FirstName = "B", LastName = "B", Phone = "B", Email = "B" });
            instructors.Add(new InstListBoxInstructors() { FirstName = "C", LastName = "C", Phone = "C", Email = "C" });

            for (int i = 0; instructors.Count > i; i++)
            {
                instructors[i].Setup = $"{instructors[i].FirstName} {instructors[i].LastName}\n{instructors[i].Phone}\n{instructors[i].Email}";
            }

            //Set the ItemsSource to the list, so that the ListBox uses the list to make the ListBoxItems
            Inst_DisInst_ListBox.ItemsSource = instructors;
        }

        //Class to define the content of the ListBoxItems for the ListBox
        public class InstListBoxInstructors
        {
            //The attributes of the items for the ListBox
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Setup { get; set; }
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
            SaveInstructor(CurrentInstructor);
            ClearInput();
        }

        private void Inst_Edit_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Inst_Delete_Button_Click(object sender, RoutedEventArgs e)
        {

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

        
        public void RetrieveInstructorData(int index)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT PK_InstID, InstFirstName, InstLastName, InstPhone, InstEmail FROM VK_Instructors ORDER BY PK_InstID ASC OFFSET @Index ROWS FETCH NEXT 1 ROW ONLY", con);
                
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_InstID) FROM VK_Instructors", con);
                int intCount = (int)count.ExecuteScalar();
                
                if (index < 0)
                {
                    index = 0;
                }
                cmd.Parameters.AddWithValue("@Index", index);
                
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    List<InstListBoxInstructors> instructors = new List<InstListBoxInstructors>();

                    while (dr.Read())
                    {
                        Instructor instructorToBeRetrieved = new Instructor(0, "", "", "", "")
                        {
                            InstId = int.Parse(dr["PK_InstID"].ToString()),
                            InstFirstName = dr["InstFirstName"].ToString(),
                            InstLastName = dr["InstLastName"].ToString(),
                            InstPhone = dr["InstPhone"].ToString(),
                            InstEmail = dr["InstEmail"].ToString(),
                        };

                        for (int i = 0; i < intCount; i++)
                        {
                            instructors.Add(new InstListBoxInstructors() {
                                FirstName = dr["InstFirstName"].ToString(),
                                LastName = dr["InstLastName"].ToString(),
                                Phone = dr["InstPhone"].ToString(),
                                Email = dr["InstEmail"].ToString()
                            });

                            instructors[i].Setup = $"{instructors[i].FirstName} {instructors[i].LastName}\n{instructors[i].Phone}\n{instructors[i].Email}";
                            Inst_DisInst_ListBox.ItemsSource = instructors;
                        }
                    }
                }
            }
        }
        

        // Clears inputfields after saving to DB
        private void ClearInput()
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
