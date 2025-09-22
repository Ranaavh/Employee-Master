namespace EmployeeMaster.Models.Department
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly string _connectionString;

        public DepartmentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<Department> GetDepartments()
        {
            List<Department> departments = new List<Department>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT DepartmentID, Name, Description FROM Department";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        departments.Add(new Department
                        {
                            DepartmentID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }
            //Console.WriteLine($"Number of departments retrieved: {departments.Count}");
            return departments;
        }

        public Department GetDepartmentById(int departmentId)
        {
            Department department = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT DepartmentID, Name, Description FROM Department WHERE DepartmentID = @DepartmentID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@DepartmentID", departmentId);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        department = new Department
                        {
                            DepartmentID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                        };
                    }
                }
            }
            return department; // Return the found department or null if not found
        }

    }

}
