namespace EmployeeMaster.Models.Qualification
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    public class QualificationRepository : IQualificationRepository
    {
        private readonly string _connectionString;

        public QualificationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<Qualification> GetQualifications()
        {
            List<Qualification> qualifications = new List<Qualification>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT QualificationID,Name,Description FROM Qualification";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        qualifications.Add(new Qualification
                        {
                            QualificationID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }

            return qualifications;
        }

        public Qualification GetQualificationById(int qualificationId)
        {
            Qualification qualification = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT QualificationID, Name, Description FROM Qualification WHERE QualificationID = @QualificationID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@QualificationID", qualificationId);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        qualification = new Qualification
                        {
                            QualificationID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                        };
                    }
                }
            }
            return qualification; // Return the found department or null if not found
        }

        public List<Qualification> GetQualificationsByDepartment(int departmentId)
        {
            List<Qualification> qualifications = new List<Qualification>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // SQL query to get qualifications filtered by DepartmentID
                string query = "SELECT QualificationID, Name, Description FROM Qualification WHERE DepartmentID = @DepartmentID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@DepartmentID", departmentId);  // Pass DepartmentID parameter

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        qualifications.Add(new Qualification
                        {
                            QualificationID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }

            return qualifications;
        }
    }

}
