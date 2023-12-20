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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //All pages but the Class_Sub_ShowClass_Page (And Main Window) initialized to avoid reseting the pages under navigation
        DashboardPage dP = new DashboardPage();
        AppointmentPage aP = new AppointmentPage();
        ClassPage cP = new ClassPage();
        InstructorPage iP = new InstructorPage();
        LessonPage lP = new LessonPage();
        StudentPage sP = new StudentPage();
        DocumentationPage doP = new DocumentationPage();

        public MainWindow()
        {
            InitializeComponent();
            
            //Makes the DashBoard button gray to indicate which page the user is on
            Menu_Dash_Button.Background = Brushes.Gray;
            
            //The Frame PageView's content is set to the DashBoardPage dP
            PageView.Content = dP;
            
            //Gives ClassPage access to the Frame PageView
            cP.pageView = PageView; 
        }

        //All of the methods underneath, except the bottom most, are Click events from the MainWindow
        //Handling the navigation between the different pages in the program
        private void Menu_Dash_Button_Click(object sender, RoutedEventArgs e)
        {
            //All buttons' colors are set to light grey
            LightGrayButtons();

            //PageView' content is set to the corresponding page
            PageView.Content = dP;

            //The Clicked Button is set to gray to show which page the user is currently on
            Menu_Dash_Button.Background = Brushes.Gray;
        }

        private void Menu_Class_Button_Click(object sender, RoutedEventArgs e)
        {
            LightGrayButtons();
            PageView.Content = cP;
            Menu_Class_Button.Background = Brushes.Gray;
        }

        private void Menu_Stu_Button_Click(object sender, RoutedEventArgs e)
        {
            LightGrayButtons();
            PageView.Content = sP;
            Menu_Stu_Button.Background = Brushes.Gray;
        }

        private void Menu_Inst_Button_Click(object sender, RoutedEventArgs e)
        {
            LightGrayButtons();
            PageView.Content = iP;
            Menu_Inst_Button.Background = Brushes.Gray;
        }

        private void Menu_Apmt_Button_Click(object sender, RoutedEventArgs e)
        {
            LightGrayButtons();
            PageView.Content = aP;
            Menu_Apmt_Button.Background = Brushes.Gray;
        }

        private void Menu_Les_Button_Click(object sender, RoutedEventArgs e)
        {
            LightGrayButtons();
            PageView.Content = lP;
            Menu_Les_Button.Background = Brushes.Gray;
        }

        private void Menu_Dok_Button_Click(object sender, RoutedEventArgs e)
        {
            LightGrayButtons();
            PageView.Content = doP;
            Menu_Dok_Button.Background = Brushes.Gray;

        }

        //The LightGrayButtons method is a method which set all button to light gray
        //Used to make sure the gray Button from the previous page is set back to default (Light Gray)
        private void LightGrayButtons() 
        {
            Menu_Dash_Button.Background = Brushes.LightGray;
            Menu_Class_Button.Background = Brushes.LightGray;
            Menu_Stu_Button.Background = Brushes.LightGray;
            Menu_Inst_Button.Background = Brushes.LightGray;
            Menu_Les_Button.Background = Brushes.LightGray;
            Menu_Apmt_Button.Background = Brushes.LightGray;
            Menu_Dok_Button.Background = Brushes.LightGray;

        }

      
    }
}
