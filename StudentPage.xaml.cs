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
            stu_listBoxt_students.Items.Clear(); // Fjerner eksiterende elementer


        }

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

       

        private void studentsListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
