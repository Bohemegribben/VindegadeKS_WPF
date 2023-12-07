using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
using static VindegadeKS_WPF.LessonPage;

namespace VindegadeKS_WPF
{
    /// <summary>
    /// Interaction logic for ClassPage.xaml
    /// </summary>
    public partial class ClassPage : Page
    {
        public ClassPage()
        {
            InitializeComponent();
            LockInputFields();
            ListBoxFunction();
            ComboBoxFunctionYear();
            ComboBoxFunctionQuarters();
            ComboBoxFunctionLicenseTypes();
        }

        Class CurrentClass = new Class();
        Class classToBeRetrieved;
        public string currentItem;

        private void ComboBoxFunctionYear()
        {
            List<Class> years = new List<Class>();

            for (int i = 24; i <= 40; i++)
            {
                years.Add(new Class { ClassYear = $"{i}" });
            }
            Class_Year_ComboBox.ItemsSource = years;
            Class_Year_ComboBox.DisplayMemberPath = "ClassYear";
            Class_Year_ComboBox.SelectedIndex = 0;

        }
        private void ComboBoxFunctionQuarters()
        {

            List<Class> quarters = new List<Class>();

            quarters.Add(new Class { ClassQuarter = Quarter.Spring });
            quarters.Add(new Class { ClassQuarter = Quarter.Summer });
            quarters.Add(new Class { ClassQuarter = Quarter.Fall });
            quarters.Add(new Class { ClassQuarter = Quarter.Winter });

            Class_Quarter_ComboBox.ItemsSource = quarters;
            Class_Quarter_ComboBox.DisplayMemberPath = "ClassQuarter";
            Class_Quarter_ComboBox.SelectedIndex = 0;

        }
        private void ComboBoxFunctionLicenseTypes()
        {

            List<Class> licenseTypes = new List<Class>();

            licenseTypes.Add(new Class { ClassLicenseType = LicenseType.B /*, DisplayValue = "B (Bil – max. 3500 kg)"*/ });
            licenseTypes.Add(new Class { ClassLicenseType = LicenseType.A1 /*, DisplayValue = "A1 (Lille motorcykel)"*/ });
            licenseTypes.Add(new Class { ClassLicenseType = LicenseType.A2 /*, DisplayValue = "A2 (Mellemstor motorcykel)"*/ });
            licenseTypes.Add(new Class { ClassLicenseType = LicenseType.A /*, DisplayValue = "A (Stor motorcykel)"*/ });

            Class_LicenseType_ComboBox.ItemsSource = licenseTypes;
            Class_LicenseType_ComboBox.DisplayMemberPath = "ClassLicenseType";
            Class_LicenseType_ComboBox.SelectedIndex = 0;
        }

        private void Les_DisLes_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Class_DisClass_ListBox.SelectedItem != null)
            {
                currentItem = (Class_DisClass_ListBox.SelectedItem as Class).ClassName;
            }
        }

        private void ListBoxFunction()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_ClassName) from VK_Classes", con);
                int intCount = (int)count.ExecuteScalar();

                List<Class> items = new List<Class>();

                for (int i = 0; i < intCount; i++)
                {
                    RetrieveClassData(i);

                    items.Add(new Class() { ClassName = classToBeRetrieved.ClassName, ClassYear = classToBeRetrieved.ClassYear, ClassQuarter = classToBeRetrieved.ClassQuarter, ClassLicenseType = classToBeRetrieved.ClassLicenseType });
                }
                Class_DisClass_ListBox.ItemsSource = items;
            }
        }

        public void RetrieveClassData(int dbRowNum)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT PK_ClassName, ClassYear, ClassNumber, ClassQuarter, ClassLicenseType FROM VK_Classes ORDER BY PK_ClassName ASC OFFSET @dbRowNum ROWS FETCH NEXT 1 ROW ONLY", con);

                if (dbRowNum < 0)
                {
                    dbRowNum = 0;
                }
                cmd.Parameters.AddWithValue("@dbRowNum", dbRowNum);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        classToBeRetrieved = new Class(default, "", "", default, default) // problemer
                        {
                            ClassName = dr["PK_ClassName"].ToString(),
                            ClassYear = dr["ClassYear"].ToString(),
                            ClassNumber = dr["ClassNumber"].ToString(), //hvordan laver vi ClassNumber
                            ClassQuarter = (Quarter)Enum.Parse(typeof(Quarter), dr["ClassQuarter"].ToString()),
                            ClassLicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), dr["ClassLicenseType"].ToString()),
                        };
                    }
                }
            }
        }

        private void Class_Create_button_Click(object sender, RoutedEventArgs e)
        {
            UnlockInputFields();
        }

        private void Class_Save_button_Click(object sender, RoutedEventArgs e)
        {
            CurrentClass.ClassYear = Class_Year_ComboBox.Text;
            CurrentClass.ClassNumber = "0";
            CurrentClass.ClassQuarter = (Quarter)Enum.Parse(typeof(Quarter), Class_Quarter_ComboBox.Text);
            CurrentClass.ClassLicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), Class_LicenseType_ComboBox.Text);
            SaveClass(CurrentClass);
        }

        public void SaveClass(Class classToBeCreated)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO VK_Classes (ClassName, ClassYear, ClassNumber, ClassQuarter, ClassLicenseType)" +
                                                 "VALUES(@ClassName, @ClassYear, @ClassNumber, @ClassQuarter, @ClassLicenseType)" +
                                                 "SELECT @@IDENTITY", con);
                cmd.Parameters.Add("@ClassName", SqlDbType.NVarChar).Value = classToBeCreated.ClassName;
                cmd.Parameters.Add("@ClassYear", SqlDbType.NVarChar).Value = classToBeCreated.ClassYear;
                cmd.Parameters.Add("@ClassNumber", SqlDbType.NVarChar).Value = classToBeCreated.ClassNumber;
                cmd.Parameters.Add("@ClassQuarter", SqlDbType.NVarChar).Value = classToBeCreated.ClassQuarter;
                cmd.Parameters.Add("@ClassLicenseType", SqlDbType.NVarChar).Value = classToBeCreated.ClassLicenseType;
                cmd.ExecuteScalar(); // problem!!
            }
        }

        private void LockInputFields()
        {
            Class_Year_ComboBox.IsEnabled = false;
            Class_Quarter_ComboBox.IsEnabled = false;
            Class_LicenseType_ComboBox.IsEnabled = false;
        }

        private void UnlockInputFields()
        {
            Class_Year_ComboBox.IsEnabled = true;
            Class_Quarter_ComboBox.IsEnabled = true;
            Class_LicenseType_ComboBox.IsEnabled = true;
        }
    }
}
