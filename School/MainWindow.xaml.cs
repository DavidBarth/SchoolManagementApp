using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using School.Data;
using System.Data;
using System.Data.Objects;


namespace School
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Connection to the School database
        private SchoolDBEntities schoolContext = null;

        // Field for tracking the currently selected teacher
        private Teacher teacher = null;

        // List for tracking the students assigned to the teacher's class
        private IList studentsInfo = null;

        #region Predefined code

        public MainWindow()
        {
            InitializeComponent();
        }

        // Connect to the database and display the list of teachers when the window appears
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.schoolContext = new SchoolDBEntities();
            teachersList.DataContext = this.schoolContext.Teachers;
        }

        // When the user selects a different teacher, fetch and display the students for that teacher
        private void teachersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Find the teacher that has been selected
            this.teacher = teachersList.SelectedItem as Teacher;
            this.schoolContext.LoadProperty<Teacher>(this.teacher, s => s.Students);

            // Find the students for this teacher
            this.studentsInfo = ((IListSource)teacher.Students).GetList();

            // Use databinding to display these students
            studentsList.DataContext = this.studentsInfo;
        }

        #endregion

        // When the user presses a key, determine whether to add a new student to a class, remove a student from a class, or modify the details of a student
        private void studentsList_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // If the user pressed Enter, edit the details for the currently selected student
                case Key.Enter: Student student = this.studentsList.SelectedItem as Student;

                    editStudent(student);
                    
                    break;


                // if the the user pressed Insert create new StudentForm 
                case Key.Insert: insertStudent();
                    break;

                case Key.Delete: student = this.studentsList.SelectedItem as Student;
                    removeStudent(student);
                    break;


            }
        }

        private void removeStudent(Student student)
        {
            MessageBoxResult mbr = MessageBox.Show(string.Format("Remove {0}", student.FirstName + " " + student.LastName + "?"),
                       "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            saveChanges.IsEnabled = true;
            schoolContext.Students.DeleteObject(student);
        }

        private void insertStudent()
        {
           StudentForm sf = new StudentForm();
            sf.Title = "New student for Class" + teacher.Class;

            //display the from
            if (sf.ShowDialog().Value)
            {
                //creates new object
                Student newStudent = new Student();
                newStudent.FirstName = sf.firstName.Text;
                newStudent.LastName = sf.lastName.Text;
                newStudent.DateOfBirth = DateTime.Parse(sf.dateOfBirth.Text);
                this.teacher.Students.Add(newStudent);
                this.studentsInfo.Add(newStudent);
                saveChanges.IsEnabled = true;
            }
        }

        #region Predefined code

        private void studentsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            editStudent(this.studentsList.SelectedItem as Student);
        }

        private void editStudent(Student student)
        {
            // Use the StudentsForm to display and edit the details of the student
            StudentForm sf = new StudentForm();

            // Set the title of the form and populate the fields on the form with the details of the student           
            sf.Title = "Edit Student Details";
            sf.firstName.Text = student.FirstName;
            sf.lastName.Text = student.LastName;
            sf.dateOfBirth.Text = student.DateOfBirth.ToString("d"); // Format the date to omit the time element

            // Display the form
            if (sf.ShowDialog().Value)
            {
                // When the user closes the form, copy the details back to the student
                student.FirstName = sf.firstName.Text;
                student.LastName = sf.lastName.Text;
                student.DateOfBirth = DateTime.Parse(sf.dateOfBirth.Text);

                // Enable saving (changes are not made permanent until they are written back to the database)
                saveChanges.IsEnabled = true;
            }
        }

        // Save changes back to the database and make them permanent
        private void saveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.schoolContext.SaveChanges();
                saveChanges.IsEnabled = false;
            }

             //if the user has changed the same student earlier , the overwrite thei changes with the new data
            catch (OptimisticConcurrencyException)
            {
                this.schoolContext.Refresh(RefreshMode.StoreWins, schoolContext.Students);
                this.schoolContext.SaveChanges();
            }

              // if some sort of db exception has occured, then display the reason for rollback
            catch (UpdateException uEX)
            {
                MessageBox.Show(uEX.InnerException.Message, "Error saving changes");
                this.schoolContext.Refresh(RefreshMode.StoreWins, schoolContext.Students);
            }

               
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error saving changes");
                this.schoolContext.Refresh(RefreshMode.ClientWins, schoolContext.Students);
            }

        }

        #endregion

        
    }

    [ValueConversion(typeof(string), typeof(Decimal))]
    class AgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                DateTime studentDOB = (DateTime)value; //casting value to DateTime obj
                TimeSpan diff = DateTime.Now - studentDOB; //calculates difference
                int age = (int)diff.Days / 365; // calculates age

                return age;
            }
            else
            {
                return "";
            }

        }

        #region Predefined code

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
