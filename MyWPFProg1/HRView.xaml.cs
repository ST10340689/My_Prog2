using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Windows;

namespace WPFPoE2
{
    public partial class HRView : Window
    {
        private string connectionString = "Server=labg9aeb3\\sqlexpress;Database=ProgPoE2;Trusted_Connection=True;";

        public HRView()
        {
            InitializeComponent();
        }


        private void GenerateInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                var approvedClaims = FetchApprovedClaims();


                string reportFilePath = GenerateReport(approvedClaims);


                ReportStatusTextBlock.Text = $"Report generated successfully: {reportFilePath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}");
            }
        }

        private List<string> FetchApprovedClaims()
        {
            var approvedClaims = new List<string>();
            string query = "SELECT LecturerName, HoursWorked, HourlyRate, AdditionalNotes, FinalPayment FROM Claims WHERE Status = 'Approved'";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);

                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string claim = $"Lecturer: {reader["LecturerName"]}, Hours Worked: {reader["HoursWorked"]}, Hourly Rate: {reader["HourlyRate"]}, Notes: {reader["AdditionalNotes"]}, Final Payment: {reader["FinalPayment"]}";
                            approvedClaims.Add(claim);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching claims: {ex.Message}");
                }
            }

            return approvedClaims;
        }

        private string GenerateReport(List<string> claims)
        {
            string reportFilePath = "C:\\Reports\\ApprovedClaimsReport.txt";

            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(reportFilePath));

            using (StreamWriter writer = new StreamWriter(reportFilePath))
            {
                writer.WriteLine("Approved Claims Report");
                writer.WriteLine("----------------------");
                foreach (var claim in claims)
                {
                    writer.WriteLine(claim);
                }
            }

            return reportFilePath;
        }
        private void UpdateLecturerButton_Click(object sender, RoutedEventArgs e)
        {
            string lecturerId = LecturerIdTextBox.Text;
            string newName = NewNameTextBox.Text;
            string newPassword = NewPasswordBox.Password;

            // Validate the input fields
            if (string.IsNullOrEmpty(lecturerId) || (string.IsNullOrEmpty(newName) && string.IsNullOrEmpty(newPassword)))
            {
                MessageBox.Show("Please enter the Lecturer ID and at least one field (Name or Password) to update.");
                return;
            }

            try
            {
                // Call a method to update the lecturer's info
                UpdateLecturerInfo(lecturerId, newName, newPassword);
                MessageBox.Show("Lecturer information updated successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating lecturer info: {ex.Message}");
            }
        }

        private void UpdateLecturerInfo(string lecturerId, string newName, string newPassword)
        {
            // This is where you will implement the logic to update the lecturer's data in your database
            string query = "UPDATE Lecturer SET LecturerName = @NewName, AccPassword = @NewPassword WHERE LecturerID = @LecturerID";

            using (SqlConnection conn = new SqlConnection("Server=labg9aeb3\\sqlexpress;Database=ProgPoE2;Trusted_Connection=True;"))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@LecturerID", lecturerId);
                cmd.Parameters.AddWithValue("@NewName", newName);
                cmd.Parameters.AddWithValue("@NewPassword", newPassword);  

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error updating lecturer info in the database.", ex);
                }
            }
        }
    }
}
