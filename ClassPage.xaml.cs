using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
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
            ListBoxFunction();
            ComboBoxFunctionYear();
            ComboBoxFunctionQuarters();
            ComboBoxFunctionLicenseTypes();
        }

        Class CurrentClass = new Class();
        Class classToBeRetrieved;
        bool edit = false;
        int currentItem;

        private void ComboBoxFunctionYear()
        {
            List<ClassComboBox> years = new List<ClassComboBox>();

            for (int i = 24; i <= 40; i++)
            {
                years.Add(new ClassComboBox { Year = $"{i}" });
            }
            Class_Year_ComboBox.ItemsSource = years;
            Class_Year_ComboBox.DisplayMemberPath = "Year";
            Class_Year_ComboBox.SelectedIndex = 0;

        }
        private void ComboBoxFunctionQuarters()
        {

            List<ClassComboBox> quarters = new List<ClassComboBox>();

            quarters.Add(new ClassComboBox { CQuarter = Quarter.Spring });
            quarters.Add(new ClassComboBox { CQuarter = Quarter.Summer });
            quarters.Add(new ClassComboBox { CQuarter = Quarter.Fall });
            quarters.Add(new ClassComboBox { CQuarter = Quarter.Winter });

            Class_Quarter_ComboBox.ItemsSource = quarters;
            Class_Quarter_ComboBox.DisplayMemberPath = "CQuarter";
            Class_Quarter_ComboBox.SelectedIndex = 0;

        }
        private void ComboBoxFunctionLicenseTypes()
        {

            List<ClassComboBox> licenseTypes = new List<ClassComboBox>();

            licenseTypes.Add(new ClassComboBox { CLicenseType = LicenseType.B, DisplayValue = "B (Bil – max. 3500 kg)" });
            licenseTypes.Add(new ClassComboBox { CLicenseType = LicenseType.A1, DisplayValue = "A1 (Lille motorcykel)" });
            licenseTypes.Add(new ClassComboBox { CLicenseType = LicenseType.A2, DisplayValue = "A2 (Mellemstor motorcykel)" });
            licenseTypes.Add(new ClassComboBox { CLicenseType = LicenseType.A, DisplayValue = "A (Stor motorcykel)" });

            Class_LicenseType_ComboBox.ItemsSource = licenseTypes;
            Class_LicenseType_ComboBox.DisplayMemberPath = "DisplayValue";
            Class_LicenseType_ComboBox.SelectedIndex = 0;
        }

        public class ClassComboBox
        {
            public string Year { get; set; }
            public Quarter CQuarter { get; set; }
            public LicenseType CLicenseType { get; set; }
            public string DisplayValue { get; set; }
        }

        private void Les_DisLes_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Class_DisClass_ListBox.SelectedItem != null)
            {
                currentItem = (Class_DisClass_ListBox.SelectedItem as LesListBoxItems).Id;
            }
        }

        private void ListBoxFunction()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_ClassName) from VK_Classes", con);
                int intCount = (int)count.ExecuteScalar();

                List<ClassListBoxItems> items = new List<ClassListBoxItems>();

                for (int i = 0; i < intCount; i++)
                {
                    RetrieveClassData(i);

                    items.Add(new ClassListBoxItems() { Name = classToBeRetrieved.ClassName, Year = classToBeRetrieved.ClassYear, CQuarter = classToBeRetrieved.ClassQuarter, CLicenseType = classToBeRetrieved.ClassLicenseType });
                }
                Class_DisClass_ListBox.ItemsSource = items;
            }
        }

        public class ClassListBoxItems
        {
            public string Name { get; set; }
            public string Year { get; set; }
            public Quarter CQuarter { get; set; }
            public string Number { get; set; }
            public LicenseType CLicenseType { get; set; }
        }

        public void RetrieveClassData(int dbRowNum)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT PK_ClassName, ClassYear, ClassQuarter, ClassLicenseType FROM VK_Classes ORDER BY PK_ClassName ASC OFFSET @dbRowNum ROWS FETCH NEXT 1 ROW ONLY", con);

                if (dbRowNum < 0)
                {
                    dbRowNum = 0;
                }
                cmd.Parameters.AddWithValue("@dbRowNum", dbRowNum);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        classToBeRetrieved = new Class(default, "", "", default)
                        {
                            ClassName = dr["PK_ClassName"].ToString(),
                            ClassYear = dr["ClassYear"].ToString(),
                            ClassQuarter = (Quarter)Enum.Parse(typeof(Quarter), dr["ClassQuarter"].ToString()),
                            ClassLicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), dr["ClassLicenseType"].ToString()),
                        };
                    }
                }
            }
        }
    }
}
