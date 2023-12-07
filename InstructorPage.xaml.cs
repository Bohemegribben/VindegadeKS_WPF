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
                currentItem = (Inst_DisInst_ListBox.SelectedItem as Instructor).InstId;
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
            Inst_FirstName_TextBox.Text = (Inst_DisInst_ListBox.SelectedItem as Instructor).InstFirstName;
            Inst_LastName_TextBox.Text = (Inst_DisInst_ListBox.SelectedItem as Instructor).InstLastName;
            Inst_Phone_TextBox.Text = (Inst_DisInst_ListBox.SelectedItem as Instructor).InstPhone;
            Inst_Email_TextBox.Text = (Inst_DisInst_ListBox.SelectedItem as Instructor).InstEmail;
        }

        private void Inst_Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            CurrentInstructor.InstId = currentItem;     // CurrentInstructor.InstId sættes lig med currentItem,
                                                        // currentItem blev defineret da ListBox.SelectedItem
                                                        // blev valgt ved museklik

            DeleteInstructor(CurrentInstructor.InstId); // DeleteInstructor kaldes med argumentet CurrentInstructor.InstId,
                                                        // CurrentInstructor.InstId bruges til at finde databaseentiteten
                                                        // med tilsvarende InstId via DeleteInstructor-metoden

            ListBoxFunction();                          // ListBoxFunction kaldes for at opdatere listboxens indhold
                                                        // efter sletningen er udført
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

        public void DeleteInstructor(int instructorIdToBeDeleted) // DeleteInstructor-metoden defineres med parameteren int instructorIdToBeDeleted,
                                                                  // Metoden tager CurrentInstructor.InstId (som har referencesemantisk lighed med currentItem)
                                                                  // som argument, når den kaldes
        {
            // Sql-Connection definerer forbindelsen 'con' til databasen
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open(); // 'Open' åbner forbindelsen 'con' til databasen
                SqlCommand cmd = new SqlCommand("DELETE FROM VK_Instructors WHERE PK_InstId = @PK_InstId", con); // SqlCommand definerer Sql-query-indholdet
                                                                                                                 // (en DELETE-kommando rettet mod en specifik
                                                                                                                 // tabel i databasen) af 'cmd', som skal
                                                                                                                 // sendes via forbindelsen 'con'

                cmd.Parameters.AddWithValue("@PK_InstId", instructorIdToBeDeleted); // cmd.Parameters.AddWithValue sætter en SQL-variabel (@PK_InstId) lig
                                                                                    // med parameteren 'instructorIdToBeDeleted', der får sit argument, når
                                                                                    // metoden bliver kaldt
                cmd.ExecuteScalar(); // ExecuteScalar-metoden kører kommandoen cmd
            }

            ClearInputFields(); // Input-felterne cleares for at indikere, at sletningen er gennemført
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

        private void ListBoxFunction()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_InstID) FROM VK_Instructors", con);
                int intCount = (int)count.ExecuteScalar();

                List<Instructor> instructors = new List<Instructor>();

                for (int i = 0; i < intCount; i++)
                {
                    RetrieveInstructorData(i);

                    instructors.Add(new Instructor()
                    {
                        InstId = instructorToBeRetrieved.InstId,
                        InstFirstName = instructorToBeRetrieved.InstFirstName,
                        InstLastName = instructorToBeRetrieved.InstLastName,
                        InstPhone = instructorToBeRetrieved.InstPhone,
                        InstEmail = instructorToBeRetrieved.InstEmail
                    });

                    instructors[i].Setup = $"{instructors[i].InstFirstName} {instructors[i].InstLastName}\n{instructors[i].InstPhone}\n{instructors[i].InstEmail}";
                }
                Inst_DisInst_ListBox.ItemsSource = instructors;
            }
        }

        private void ClearInputFields()
        {
            Inst_FirstName_TextBox.Clear();
            Inst_LastName_TextBox.Clear();
            Inst_Phone_TextBox.Clear();
            Inst_Email_TextBox.Clear();
        }

        private void LockInputFields()
        {
            Inst_FirstName_TextBox.IsEnabled = false;
            Inst_LastName_TextBox.IsEnabled = false;
            Inst_Phone_TextBox.IsEnabled = false;
            Inst_Email_TextBox.IsEnabled = false;
        }

        private void UnlockInputFields()
        {
            Inst_FirstName_TextBox.IsEnabled = true;
            Inst_LastName_TextBox.IsEnabled = true;
            Inst_Phone_TextBox.IsEnabled = true;
            Inst_Email_TextBox.IsEnabled = true;
        }
    }
}
