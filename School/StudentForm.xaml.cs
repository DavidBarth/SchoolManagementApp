using System;
using System.Windows;

namespace School
{
    /// <summary>
    /// Interaction logic for StudentForm.xaml
    /// </summary>
    public partial class StudentForm : Window
    {
        #region Predefined code

        public StudentForm()
        {
            InitializeComponent();
        }

       

        #endregion


        //event handling
        private void ok_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(this.firstName.Text))
            {
                MessageBox.Show("The student must have first name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (String.IsNullOrEmpty(this.lastName.Text))
            {
                MessageBox.Show("The student must have last name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime result;
            if (!DateTime.TryParse(this.dateOfBirth.Text, out result))
            {
                MessageBox.Show("The student must have valid date", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TimeSpan age = DateTime.Now.Subtract(result);
            if (age.Days / 365 < 5)
            {
                MessageBox.Show("The student must be older than 5 years", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            this.DialogResult = true;
        }
    }
}
