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
            string directoryPath = "C:\\SecureUploads";

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string fileName = Path.GetFileName(filePath);
            string newFilePath = Path.Combine(directoryPath, Guid.NewGuid().ToString() + "_" + fileName);

            File.Copy(filePath, newFilePath, true);
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

            if (!int.TryParse(HoursWorkedTextBox.Text, out int HoursWorked) || HoursWorked < 0)
            {
                MessageBox.Show("Please enter a valid, non-negative value for hours worked.");
                return;
            }

            if (!int.TryParse(HourlyRateTextBox.Text, out int HourlyRate) || HourlyRate < 0)
            {
                MessageBox.Show("Please enter a valid, non-negative value for hourly rate.");
                return;
            }

            // Auto-calculate final payment
            int FinalPayment = HoursWorked * HourlyRate;
            FinalPaymentTextBlock.Text = $"Calculated Payment: R{FinalPayment}";

            if (MessageBox.Show($"Submit claim with calculated payment of R{FinalPayment}?", "Confirm Submission", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                SubmitClaimToDatabase(LecturerName, HoursWorked, HourlyRate, AdditionalNotesTextBox.Text, FinalPayment);
            }
        }

        private string GetLecturerName(int userId)
        {
            string lecturerName = string.Empty;
            string query = "SELECT LecturerName FROM Lecturer WHERE LecturerID = @UserId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                try
                {
                    conn.Open();
                    lecturerName = cmd.ExecuteScalar() as string;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while retrieving the lecturer's name: {ex.Message}");
                }
            }

            return lecturerName;
        }

        private void SubmitClaimToDatabase(string LecturerName, int HoursWorked, int HourlyRate, string AdditionalNotes, int FinalPayment)
        {
            if (!VerifyDatabaseConnection())
            {
                MessageBox.Show("Unable to connect to the database. Please check your connection settings.");
                return;
            }

            string query = "INSERT INTO Claims (LecturerName, HoursWorked, HourlyRate, AdditionalNotes, Status, FinalPayment) " +
                           "VALUES (@LecturerName, @HoursWorked, @HourlyRate, @AdditionalNotes, @Status, @FinalPayment)";
            string Status = "Pending";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@LecturerName", LecturerName);
                cmd.Parameters.AddWithValue("@HoursWorked", HoursWorked);
                cmd.Parameters.AddWithValue("@HourlyRate", HourlyRate);
                cmd.Parameters.AddWithValue("@AdditionalNotes", AdditionalNotes);
                cmd.Parameters.AddWithValue("@Status", Status);
                cmd.Parameters.AddWithValue("@FinalPayment", FinalPayment);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Claim submitted successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while submitting the claim: {ex.Message}");
                }
            }
        }

        private bool VerifyDatabaseConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Connection error: {sqlEx.Message}");
                return false;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string LecturerName = GetLecturerName(currentUserId);
            LecturerClaimList claimListWindow = new LecturerClaimList(LecturerName);
            claimListWindow.Show();
            this.Close();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Ensure both textboxes contain valid numeric values
            if (int.TryParse(HoursWorkedTextBox.Text, out int hoursWorked) && hoursWorked >= 0 &&
                int.TryParse(HourlyRateTextBox.Text, out int hourlyRate) && hourlyRate >= 0)
            {
                // Calculate the final payment
                int finalPayment = hoursWorked * hourlyRate;

                // Display the calculated payment in the FinalPaymentTextBlock
                FinalPaymentTextBlock.Text = $"R{finalPayment}";
            }
            else
            {
                // Clear the final payment display if inputs are invalid
                FinalPaymentTextBlock.Text = "Invalid input";
            }
        }

    }
}
