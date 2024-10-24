using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace WPFPoE2
{
    public partial class LecturerClaimPrompt : Window
    {
        private List<string> uploadedFileNames = new List<string>();
        private string connectionString = "Server=labg9aeb3\\sqlexpress;Database=ProgPoE2;Trusted_Connection=True;";

        // Replace this with the logic to retrieve the currently logged-in lecturer's ID
        private int currentUserId = 1; // Example: assuming the logged-in user has ID 1

        public LecturerClaimPrompt()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Multiselect = true,
                    Title = "Select Documents",
                    Filter = "PDF Files (*.pdf)|*.pdf|Word Documents (*.docx;*.doc)|*.docx;*.doc|Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string file in openFileDialog.FileNames)
                    {
                        StoreFileSecurely(file);
                    }

                    UploadedFilesTextBlock.Text = string.Join(", ", uploadedFileNames);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void StoreFileSecurely(string filePath)
        {
            // Define the directory to store the uploaded files
            string directoryPath = "C:\\SecureUploads"; // Change this to your secure directory

            // Ensure the directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Generate a unique file name to avoid conflicts
            string fileName = Path.GetFileName(filePath);
            string newFilePath = Path.Combine(directoryPath, Guid.NewGuid().ToString() + "_" + fileName);

            // Copy the file to the secure directory
            File.Copy(filePath, newFilePath, true); // true to overwrite if the file already exists

            // Add the uploaded file name to the list
            uploadedFileNames.Add(fileName);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string LecturerName = GetLecturerName(currentUserId);
            if (string.IsNullOrEmpty(LecturerName))
            {
                MessageBox.Show("Lecturer name not found.");
                return;
            }

            string HoursWorkedText = HoursWorkedTextBox.Text;
            string HourlyRateText = HourlyRateTextBox.Text;
            string AdditionalNotes = AdditionalNotesTextBox.Text;

            // Validate inputs
            if (string.IsNullOrEmpty(HoursWorkedText) || string.IsNullOrEmpty(HourlyRateText))
            {
                MessageBox.Show("Please fill in hours worked and hourly rate.");
                return;
            }

            if (!int.TryParse(HoursWorkedText, out int HoursWorked) || !int.TryParse(HourlyRateText, out int HourlyRate))
            {
                MessageBox.Show("Please enter valid numeric values for hours worked and hourly rate.");
                return;
            }

            SubmitClaimToDatabase(LecturerName, HoursWorked, HourlyRate, AdditionalNotes);
        }

        private string GetLecturerName(int userId)
        {
            string lecturerName = string.Empty;
            string query = "SELECT LecturerName FROM Lecturer WHERE LecturerID = @UserId"; // Adjust if using a different identifier

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId); // Use the int userId

                try
                {
                    conn.Open();
                    lecturerName = cmd.ExecuteScalar() as string; // Execute and retrieve the name
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while retrieving the lecturer's name: {ex.Message}");
                }
            }

            return lecturerName;
        }

        private void SubmitClaimToDatabase(string LecturerName, int HoursWorked, int HourlyRate, string AdditionalNotes)
        {
            if (!VerifyDatabaseConnection())
            {
                MessageBox.Show("Unable to connect to the database. Please check your connection settings.");
                return;
            }

            string query = "INSERT INTO Claims (LecturerName, HoursWorked, HourlyRate, AdditionalNotes, Status) VALUES (@LecturerName, @HoursWorked, @HourlyRate, @AdditionalNotes, @Status)";
            string Status = "Pending";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@LecturerName", LecturerName);
                cmd.Parameters.AddWithValue("@HoursWorked", HoursWorked);
                cmd.Parameters.AddWithValue("@HourlyRate", HourlyRate);
                cmd.Parameters.AddWithValue("@AdditionalNotes", AdditionalNotes);
                cmd.Parameters.AddWithValue("@Status", Status);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Claim submitted successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while submitting the claim: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        private bool VerifyDatabaseConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open(); // Try to open the connection
                    return true; // Connection is successful
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Connection error: {sqlEx.Message}");
                return false; // Connection failed
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string LecturerName = GetLecturerName(currentUserId);
            LecturerClaimList claimListWindow = new LecturerClaimList(LecturerName);
            claimListWindow.Show();
            this.Close();
        }
    }
}
