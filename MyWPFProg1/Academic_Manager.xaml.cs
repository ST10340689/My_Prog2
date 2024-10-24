using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;

namespace WPFPoE2
{
    public partial class Academic_Manager : Window
    {
        private string connectionString = "Server=labg9aeb3\\sqlexpress;Database=ProgPoE2;Trusted_Connection=True;";

        public Academic_Manager()
        {
            InitializeComponent();
            LoadPendingClaims();
        }

        private void LoadPendingClaims()
        {
            List<Claim> pendingClaims = new List<Claim>();

            // Adjusted to reflect column names in your database
            string query = "SELECT ClaimId, LecturerName, AdditionalNotes FROM Claims WHERE Status = 'Pending'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Claim claim = new Claim
                        {
                            ClaimId = reader.GetInt32(0),
                            LecturerName = reader.GetString(1),
                            AdditionalNotes = reader.GetString(2)
                        };
                        pendingClaims.Add(claim);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while loading claims: {ex.Message}");
                }
            }

            // Binding the list of claims to the ListView
            ClaimListView.ItemsSource = pendingClaims;
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            // Logic for approving the claim
            if (ClaimListView.SelectedItem is Claim selectedClaim)
            {
                UpdateClaimStatus(selectedClaim.ClaimId, "Approved");
            }
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            // Logic for rejecting the claim
            if (ClaimListView.SelectedItem is Claim selectedClaim)
            {
                UpdateClaimStatus(selectedClaim.ClaimId, "Rejected");
            }
        }

        private void UpdateClaimStatus(int claimId, string status)
        {
            string query = "UPDATE Claims SET Status = @Status WHERE ClaimId = @ClaimId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Status", status);
                command.Parameters.AddWithValue("@ClaimId", claimId);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show($"Claim {status} successfully.");
                    LoadPendingClaims();  // Refresh the claim list
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
        }

        private void ClaimListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ClaimListView.SelectedItem is Claim selectedClaim)
            {
                ClaimDetailsTextBox.Text = $"Lecturer: {selectedClaim.LecturerName}\n" +
                                           $"Notes: {selectedClaim.AdditionalNotes}";
            }
        }
    }
}