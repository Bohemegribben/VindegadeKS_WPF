﻿using Microsoft.Data.SqlClient;
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

            ClearInputFields();

            //Locks the input fields, so the user can't interact before pressing a button
            LockInputFields();

            //Calls the ListBoxFunction method which create the ListBoxItems for your ListBox
            //ListBoxFunction();

            //Calls the three ComboBoxFunction methods which sets up the ComboBoxes on your page 
            ComboBoxFunctionYear();
            ComboBoxFunctionQuarters();
            ComboBoxFunctionLicenseTypes();
        }

        //When loading the page, do the following
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            ListBoxFunction();
            Class_Subpage_button.IsEnabled = false;
        }

        public Frame pageView { get; set; }


        //Create CurrentClass to contain current object - Needed in: Save_Button_Click
        Class currentClass = new Class();

        //Moved out here instead of staying in 'Retrieve', so ListBoxFunction can access - Needed in: ListBoxFunction & RetrieveData
        Class classToBeRetrieved;

        //Keeps track of the ClassName of ListBoxItem while it's selected - Edit_Button_Click & ListBox_SelectionChanged
        string currentItem;

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

            quarters.Add(new Class { ClassQuarter = Quarter.F });
            quarters.Add(new Class { ClassQuarter = Quarter.S });
            quarters.Add(new Class { ClassQuarter = Quarter.E });
            quarters.Add(new Class { ClassQuarter = Quarter.V });

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

        private void Class_DisClass_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Class_DisClass_ListBox.SelectedItem != null)
            {
                currentItem = (Class_DisClass_ListBox.SelectedItem as Class).ClassName;
                Class_Subpage_button.IsEnabled = true;
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

                    items.Add(new Class() { ClassName = classToBeRetrieved.ClassName, ClassYear = classToBeRetrieved.ClassYear, ClassQuarter = classToBeRetrieved.ClassQuarter, ClassNumber = classToBeRetrieved.ClassNumber });
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
                        classToBeRetrieved = new Class(default, "", "", default, "")
                        {
                            ClassName = dr["PK_ClassName"].ToString(),
                            ClassYear = dr["ClassYear"].ToString(),
                            ClassNumber = dr["ClassNumber"].ToString(),
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
            currentClass.ClassYear = Class_Year_ComboBox.Text;
            currentClass.ClassNumber = "0";
            currentClass.ClassQuarter = (Quarter)Enum.Parse(typeof(Quarter), Class_Quarter_ComboBox.Text);
            currentClass.ClassLicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), Class_LicenseType_ComboBox.Text);
            SaveClass(currentClass);
        }

        public void SaveClass(Class classToBeCreated)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                con.Open();

                // Counts how many Class-instanses in the database that has the specific combination of ClassQuarter and ClassYear
                // equal to what is being created in the database when the method runs. If 0 ClassNumber will be set to 1.
                // If 1 ClassNumber will be set to 2. This count is done, before the colected data of the Class is created in the DB.
                // This all defines ClassName, but at the moment ListBox is not displaying ClassName correctly.
                SqlCommand count = new SqlCommand("SELECT COUNT(ClassQuarter) FROM VK_Classes WHERE ClassQuarter = @ClassQuarter AND ClassYear = @ClassYear", con);
                count.Parameters.Add("@ClassQuarter", SqlDbType.NVarChar).Value = classToBeCreated.ClassQuarter;
                count.Parameters.Add("@ClassYear", SqlDbType.NVarChar).Value = classToBeCreated.ClassYear;
                int intCount = (int)count.ExecuteScalar();
                classToBeCreated.ClassNumber = (intCount + 1).ToString();


                SqlCommand cmd = new SqlCommand("INSERT INTO VK_Classes (PK_ClassName, ClassYear, ClassNumber, ClassQuarter, ClassLicenseType)" +
                                                 "VALUES(@ClassName, @ClassYear, @ClassNumber, @ClassQuarter, @ClassLicenseType)" +
                                                 "SELECT @@IDENTITY", con);
                cmd.Parameters.Add("@ClassName", SqlDbType.NVarChar).Value = classToBeCreated.ClassName;
                cmd.Parameters.Add("@ClassYear", SqlDbType.NVarChar).Value = classToBeCreated.ClassYear;
                cmd.Parameters.Add("@ClassNumber", SqlDbType.NVarChar).Value = classToBeCreated.ClassNumber;
                cmd.Parameters.Add("@ClassQuarter", SqlDbType.NVarChar).Value = classToBeCreated.ClassQuarter;
                cmd.Parameters.Add("@ClassLicenseType", SqlDbType.NVarChar).Value = classToBeCreated.ClassLicenseType;
                cmd.ExecuteScalar();
            }

            ClearInputFields();
            ListBoxFunction();
        }

        private void ClearInputFields()
        {
            Class_Year_ComboBox.SelectedItem = null;
            Class_Quarter_ComboBox.SelectedItem = null;
            Class_LicenseType_ComboBox.SelectedItem = null;
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

        private void Class_Subpage_button_Click(object sender, RoutedEventArgs e)
        {
            pageView.Content = new Class_Sub_ShowClass_Page(currentItem);           
        }
        
    }
}
