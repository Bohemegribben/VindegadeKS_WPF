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
            LoadStudentsList(); // Indlæser listen af studerende ved opstart
            LockInputFields();
        }

        private void LoadStudentsList() 
        {
            stu_listBoxt_students.Items.Clear(); // Fjerner eksiterende elementer


        }

        #region Lock and Unlock inputFields

        // metode til at låse textboxe/input boxe fra start
        private void LockInputFields()
        {
            stu_txtBox_cpr.IsEnabled = false;
            stu_txtBox_fornavn.IsEnabled = false;
            stu_txtBox_efternavn.IsEnabled = false;
            stu_txtBox_telefon.IsEnabled = false;
            stu_txtBox_email.IsEnabled = false;
        }

        // //Unlocks all inputfields, so that they can be edited
        private void UnlockInputFields()
        {
            stu_txtBox_cpr.IsEnabled = true;
            stu_txtBox_fornavn.IsEnabled = true;
            stu_txtBox_efternavn.IsEnabled = true;
            stu_txtBox_telefon.IsEnabled = true;
            stu_txtBox_email.IsEnabled = true;
        }

        #endregion

        #region clear input

        private void ClearInput() 
        {
            stu_txtBox_cpr.Clear();
            stu_txtBox_fornavn.Clear();
            stu_txtBox_efternavn.Clear();
            stu_txtBox_telefon.Clear();
            stu_txtBox_email.Clear();
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
        private void Stu_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            UnlockInputFields();

        }

        private void Stu_Save_Button_Click(object sender, RoutedEventArgs e)
        {
           

        }

        private void Stu_Edit_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Stu_Delete_Button_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Button methods

        public void SaveStudent (Student studentToBeCreated) 
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO VK_Instructors (InstFirstName, InstLastName, InstPhone, InstEmail)" +
                                                 "VALUES(@InstFirstName,@InstLastName,@InstPhone,@InstEmail)" +
                                                 "SELECT @@IDENTITY", con);
                cmd.Parameters.Add("@InstFirstName", SqlDbType.NVarChar).Value = studentToBeCreated.StuCPR;
                cmd.Parameters.Add("@InstLastName", SqlDbType.NVarChar).Value = studentToBeCreated.StuFirstName;
                cmd.Parameters.Add("@InstPhone", SqlDbType.NVarChar).Value = studentToBeCreated.StuLastName;
                cmd.Parameters.Add("@InstEmail", SqlDbType.NVarChar).Value = studentToBeCreated.StuPhone;
                cmd.Parameters.Add("@InstEmail", SqlDbType.NVarChar).Value = studentToBeCreated.StuEmail;

               
            }
        }

        private void studentsListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
