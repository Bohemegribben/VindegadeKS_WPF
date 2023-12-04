using System;
using System.Collections.Generic;
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
    /// Interaction logic for StudentPage.xaml
    /// </summary>
    public partial class StudentPage : Page
    {
        public StudentPage()
        {
            InitializeComponent();
            this.DataContext = this; // Binder denne klasse til dens XAML view
            LoadStudentsList(); // Indlæser listen af studerende ved opstart
        }

        private void LoadStudentsList() 
        {
            studentsListBox.Items.Clear(); // Fjerner eksiterende elementer


        }

        private void watermarkTxtBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //hvis man er trykket ind på "search bar textbox" skal "watermark textbox" ikke være synlig længere
            watermarkTxtBox.Visibility = Visibility.Collapsed;

            // og vores "search textbox skal være synlig
            searchTxtBox.Visibility = Visibility.Visible;
            searchTxtBox.Focus();
        
        }

     

        private void searchTxtBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Hvis search bar er tom
            if (string.IsNullOrEmpty(searchTxtBox.Text)) 
            {
                // så skal search textbox ikke være synlig
                searchTxtBox.Visibility = Visibility.Collapsed;

                // watermark textbox skal være synlig
                watermarkTxtBox.Visibility = Visibility.Visible;

            }
        }

        private void Stu_Add_Button_Click(object sender, RoutedEventArgs e)
        {

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

        private void studentsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
