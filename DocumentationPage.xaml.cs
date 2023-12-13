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
        }



        #region ListBox
        private void Dok_DisStudents_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            


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

        }

        private void Dok_Edit_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Dok_Delete_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        #region FileUpload

        private void Dok_DokuFile_Button_Click(object sender, RoutedEventArgs e)
        {

            // First we create an instance of OpenFIleDialog, to show a dialog so the user can select a file
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Then we show the dialog and check if the user has selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                // create a string named "filePath" to get the path of the selected file
                string filePath = openFileDialog.FileName;

                try
                {
                    // Create a byte array named "fileDate" To read the contents of the file
                    byte[] fileData = File.ReadAllBytes(filePath);

                    // call the the UploadDocumentation method to connect to our database
                }

                // to handle any exceptioons such as file read errors, database connection issues etc.
                catch (Exception ex)
                {
                    MessageBox.Show("An Error occured:" + ex.Message);
                }

            }

        }

        #endregion
        #endregion

        #region ButtonMethods

        #region UploadFile

        public void UploadFile(string fileData) 
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString)) 
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("INSERT INTO VK_Documentations (DocumentationFile) VALUES (@FileData)");
                cmd.Parameters.AddWithValue("@FileData", fileData);

                cmd.ExecuteNonQuery();

             
            }

        }

        #endregion

        #endregion

        private void Dok_PickCPR_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Dok_PickType_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
