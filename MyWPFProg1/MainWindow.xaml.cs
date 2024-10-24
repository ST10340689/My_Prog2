using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace WPFPoE2
{
    public partial class MainWindow : Window
    {
        private string connectionString = "Server=labg9aeb3\\sqlexpress;Database=ProgPoE2;Trusted_Connection=True;";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string name = UsernameTextBox.Text; 
            string password = PasswordBox.Password; 

            if (IsLecturer(name, password)) //If the user is a Lecturer it will open the LecturerClaimPrompt window
            {
                LecturerClaimPrompt lecturerView = new LecturerClaimPrompt();
                lecturerView.Show();
                this.Hide();
            }
            else if (IsAcademicManagerOrCoordinator(name, password)) //If the user is an Academic Manager Or Coordinator it will open the Academic_Manager window
            {
                Academic_Manager managerWindow = new Academic_Manager();
                managerWindow.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid credentials. Please try again."); //If the login is wrong this error is displayed
            }
        }

        private bool IsLecturer(string LecturerName, string AccPassword)
        {
            string query = "SELECT COUNT(*) FROM Lecturer WHERE LecturerName = @LecturerName AND AccPassword = @AccPassword";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@LecturerName", LecturerName);
                cmd.Parameters.AddWithValue("@AccPassword", AccPassword);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();

                return count > 0;
            }
        }

        private bool IsAcademicManagerOrCoordinator(string name, string password)
        {
            string query = "SELECT COUNT(*) FROM Programme_Coordinator WHERE PcName = @name AND AccPassword = @password " +
                           "UNION ALL " +
                           "SELECT COUNT(*) FROM Academic_Manager WHERE AmName = @name AND AccPassword = @password";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@password", password);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                int count = 0;
                while (reader.Read())
                {
                    count += reader.GetInt32(0);
                }

                return count > 0;
            }
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
