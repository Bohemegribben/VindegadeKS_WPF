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
    /// Interaction logic for InstructorPage.xaml
    /// </summary>
    public partial class InstructorPage : Page
    {
        public InstructorPage()
        {
            InitializeComponent();

            AddInstructors();
        }

        //Method to create, control and add instructors to the ListBox
        private void AddInstructors()
        {
            //Make a list with the Item Class from below called items (Name doesn't matter)
            //LesListBoxItems in my case
            List<InstListBoxInstructors> instructors = new List<InstListBoxInstructors>();

            //Add new items from the item class with specific attributes to the list
            //Will later be remade to automatically add items based on the database
            instructors.Add(new InstListBoxInstructors() { FirstName = "A", LastName = "A", Phone = "A", Email = "A" });
            instructors.Add(new InstListBoxInstructors() { FirstName = "B", LastName = "B", Phone = "B", Email = "B" });
            instructors.Add(new InstListBoxInstructors() { FirstName = "C", LastName = "C", Phone = "C", Email = "C" });

            for (int i = 0; instructors.Count > i; i++)
            {
                instructors[i].Setup = $"{instructors[i].FirstName} {instructors[i].LastName}\n{instructors[i].Phone}\n{instructors[i].Email}";
            }

            //Set the ItemsSource to the list, so that the ListBox uses the list to make the ListBoxItems
            Inst_DisInst_ListBox.ItemsSource = instructors;
        }

        //Class to define the content of the ListBoxItems for the ListBox
        public class InstListBoxInstructors
        {
            //The attributes of the items for the ListBox
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Setup { get; set; }
        }
    }
}
