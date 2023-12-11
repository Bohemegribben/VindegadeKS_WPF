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
        //samtlige pages initialiseres inden MainWindow-metoden køres for at undgå reset af pages under navigation 
        DashboardPage dP = new DashboardPage();
        AppointmentPage aP = new AppointmentPage();
        ClassPage cP = new ClassPage();
        InstructorPage iP = new InstructorPage();
        LessonPage lP = new LessonPage();
        StudentPage sP = new StudentPage();

        public MainWindow()
        {
            InitializeComponent();
            Menu_Dash_Button.Background = Brushes.Gray; //Dashboardknappen gøres grå for at indikere, hvor brugeren 'befinder' sig
            PageView.Content = dP; //startsiden kaldes
            cP.pageView = PageView; //Giver ClassPage adgang til rammen PageView
        }

        //De nedenstående Button_Click-knapper bruges til at navigere mellem de primærsiderne (svarende til klasserne).
        //Dashboardet er startside, de andre er undersider derfra.
        private void Menu_Dash_Button_Click(object sender, RoutedEventArgs e)
        {
            LightGrayButtons(); //alle knapper gøres lysegrå
            PageView.Content = dP;
            Menu_Dash_Button.Background = Brushes.Gray; //knappen gøres grå, når der trykkes på den
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

        //LightGrayButtons: Metode der gør alle knapper lysegrå.
        //Den kaldes som det første, når der trykke på en ny knap
        private void LightGrayButtons() 
        {
            Menu_Dash_Button.Background = Brushes.LightGray;
            Menu_Class_Button.Background = Brushes.LightGray;
            Menu_Stu_Button.Background = Brushes.LightGray;
            Menu_Inst_Button.Background = Brushes.LightGray;
            Menu_Les_Button.Background = Brushes.LightGray;
            Menu_Apmt_Button.Background = Brushes.LightGray;
        }
    }
}
