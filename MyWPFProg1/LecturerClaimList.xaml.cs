using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;

namespace WPFPoE2
{
    public partial class LecturerClaimList : Window
    {
        private string connectionString = "Server=labg9aeb3\\sqlexpress;Database=ProgPoE2;Trusted_Connection=True;";

        public LecturerClaimList(string lecturerName)
        {
            InitializeComponent();
            LoadClaims(lecturerName);
        }

        private void LoadClaims(string lecturerName)
        {
            // Update the query to select the new fields
            string query = "SELECT ClaimId, LecturerName, HoursWorked, HourlyRate, AdditionalNotes, Status FROM Claims WHERE LecturerName = @LecturerName";

            List<Claim> claims = new List<Claim>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@LecturerName", lecturerName);

                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        claims.Add(new Claim
                        {
                            ClaimId = reader.GetInt32(0),
                            LecturerName = reader.GetString(1),
                            HoursWorked = reader.GetInt32(2),
                            HourlyRate = reader.GetInt32(3),
                            AdditionalNotes = reader.GetString(4),
                            Status = reader.GetString(5)
                        });
                    }

                    // Check if claims list is empty
                    if (claims.Count == 0)
                    {
                        MessageBox.Show("You have no claims submitted.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while loading claims: {ex.Message}");
                }
            }

            ClaimsListView.ItemsSource = claims;
        }
    }

    public class Claim
    {
        public int ClaimId { get; set; }
        public string LecturerName { get; set; }
        public int HoursWorked { get; set; }
        public int HourlyRate { get; set; }
        public string AdditionalNotes { get; set; }
        public string Status { get; set; }
    }
}
