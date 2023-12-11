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
using static VindegadeKS_WPF.AppointmentPage;

namespace VindegadeKS_WPF
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();

            ListBoxFunctionClass();
        }

        #region Properties


        //Moved out here instead of staying in 'Retrieve', so ListBoxFunction can access - Needed in: ListBoxFunction & RetrieveData
        Class classToBeRetrieved;
       
        //Keeps track of the id of ListBoxItem while it's selected - ListBox_SelectionChanged
        int currentItem;

        #endregion

        #region ListBoxFunctions

        #region ListBoxFunctionClass
        private void ListBoxFunctionClass()
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();

                //Creates a count SqlCommand, which gets the number of rows in the table 
                SqlCommand count = new SqlCommand("SELECT COUNT(PK_ClassName) from VK_Classes", con);


                //Saves count command result to int
                int intCount = (int)count.ExecuteScalar();

                //Make a list with the Item Class from below called items (Name doesn't matter)
                //LesListBoxItems in my case
                List<Class> items = new List<Class>();

                //Forloop which adds intCount number of new items to items-list
                for (int i = 0; i < intCount; i++)
                {
                    //Calls RetrieveLessonData method, sending i as index
                    RetrieveClassData(i);

                    //Adds a new item from the item class with specific attributes to the list
                    //The data added comes from RetrieveLessonData
                    items.Add(new Class() { ClassName = classToBeRetrieved.ClassName, ClassYear = classToBeRetrieved.ClassYear, ClassQuarter = classToBeRetrieved.ClassQuarter, ClassNumber = classToBeRetrieved.ClassNumber });

                    //Only necessary for multi-attribute ListBoxItem
                    //Set up the attribute 'SetUp' which is used to determine the appearance of the ListBoxItem 
                    //Mine isn't, so it's out commented
                    //items[i].SetUp = $"{items[i].Name}\n{items[i].Type}\n{items[i].Description}";
                }

                //Set the ItemsSource to the list, so that the ListBox uses the list to make the ListBoxItems
                Dash_DisDash_ListBox.ItemsSource = items;
            }
        }
        #endregion

        #region ListBoxFunctionApmt
        private void ListBoxFunctionApmt()
        {
            //Make a list with the Item Class from below called items (Name doesn't matter)
            //LesListBoxItems in my case
            List<ApmtListBoxItems> items = new List<ApmtListBoxItems>();

            //Add new items from the item class with specific attributes to the list
            //Will later be remade to automatically add items based on the database
            items.Add(new ApmtListBoxItems() { Lesson = "A", LessonType = "A", Class = "A", Student = "A", Instructor = "A", DateTime = default });
            items.Add(new ApmtListBoxItems() { Lesson = "B", LessonType = "B", Class = "B", Student = "B", Instructor = "B", DateTime = default });
            items.Add(new ApmtListBoxItems() { Lesson = "C", LessonType = "C", Class = "C", Student = "C", Instructor = "C", DateTime = default });
            items.Add(new ApmtListBoxItems() { Lesson = "D", LessonType = "D", Class = "D", Student = "D", Instructor = "D", DateTime = default });
            items.Add(new ApmtListBoxItems() { Lesson = "E", LessonType = "E", Class = "E", Student = "E", Instructor = "E", DateTime = default });
            items.Add(new ApmtListBoxItems() { Lesson = "F", LessonType = "F", Class = "F", Student = "F", Instructor = "F", DateTime = default });

            //Only necessary for multi-attribute ListBoxItem
            //Set up the attribute 'SetUp' which is used to determine the appearance of the ListBoxItem 
            //Forloop to go through all items in the items-list, to add and fill the 'SetUp' attribute
            for (int i = 0; items.Count > i; i++)
            {
                items[i].SetUp = $"{items[i].Lesson} - {items[i].LessonType}\n{items[i].DateTime}";
            }

            //Set the ItemsSource to the list, so that the ListBox uses the list to make the ListBoxItems
            Dash_DisApmt_ListBox.ItemsSource = items;
        }

        #endregion

        #endregion

        #region DashboardMethods

        public void RetrieveClassData(int dbRowNum)
        {
            //Setting up a connection to the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseServerInstance"].ConnectionString))
            {
                //Opens said connection
                con.Open();
                //Creates a cmd SqlCommand, which SELECTs a specific row 
                SqlCommand cmd = new SqlCommand("SELECT PK_ClassName, ClassQuarter, ClassYear, ClassNumber, ClassLicenseType FROM VK_Classes ORDER BY PK_ClassName ASC OFFSET @dbRowNum ROWS FETCH NEXT 1 ROW ONLY", con);

                //Set dbRowNum to 0 if under 0
                if (dbRowNum < 0)
                {
                    dbRowNum = 0;
                }
                //Gives @dbRowNum the value of dbRowNum
                cmd.Parameters.AddWithValue("@dbRowNum", dbRowNum);

                //Set up a data reader called dr, which reads the data from cmd (the previous sql command)
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    //While-loop running while dr is reading 
                    while (dr.Read())
                    {
                        //Sets ClassToBeRetrieve a new empty Class, which is then filled
                        classToBeRetrieved = new Class()
                        {
                            //Sets the attributes of lesToBeRetrieved equal to the data from the current row of the database
                            
                            ClassQuarter = Enum.Parse<Quarter>(dr["ClassQuarter"].ToString()),
                            ClassYear = dr["ClassYear"].ToString(),
                            ClassNumber = dr["ClassNumber"].ToString(),
                            ClassLicenseType = Enum.Parse<LicenseType>(dr["ClassLicenseType"].ToString()),
                            ClassName = dr["PK_ClassName"].ToString(),
                        };
                    }
                }
            }

            #endregion

          
        }
    }
}
