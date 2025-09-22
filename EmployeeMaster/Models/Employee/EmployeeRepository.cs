using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;

namespace EmployeeMaster.Models.Employee
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }



        // Get All Employees
        public IEnumerable<Employee> GetAllEmployees()
        {
            var employees = new List<Employee>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"SELECT e.EmployeeID, 
                 e.Name, 
                 e.DepartmentID,
                 d.Name AS DepartmentName, 
                 e.Position, 
                 e.JoiningDate, 
                 e.Salary, 
                 e.QualificationID,
                 q.Name AS QualificationName, 
                 e.Mobile, 
                 e.Email
                 FROM Employee e
                 LEFT JOIN Department d ON e.DepartmentID = d.DepartmentID
                 LEFT JOIN Qualification q ON e.QualificationID = q.QualificationID;
                "; // Join Qualification table

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        employees.Add(new Employee
                        {
                            EmployeeID = (int)reader["EmployeeID"],
                            Name = (string)reader["Name"],
                            DepartmentID = (int)reader["DepartmentID"], // Include DepartmentID
                            Department = (string)reader["DepartmentName"], // Display department name
                            Position = (string)reader["Position"],
                            JoiningDate = (DateTime)reader["JoiningDate"],
                            Salary = (decimal)reader["Salary"],
                            QualificationID = (int)reader["QualificationID"],
                            Qualification = (string)reader["QualificationName"], // Display qualification name
                            Mobile = reader["Mobile"] as string,
                            Email = reader["Email"] as string
                        });
                    }
                }
            }
            return employees;
        }


        // Get Employee by ID
        public Employee GetEmployee(int id)
        {
            Employee employee = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // Updated query to join with Department and Qualification tables
                string query = @"SELECT e.EmployeeID, 
                               e.Name, 
                               d.Name AS DepartmentName, 
                               e.Position, 
                               e.JoiningDate, 
                               e.Salary, 
                               q.Name AS QualificationName, 
                               e.Mobile, 
                               e.Email
                        FROM Employee e
                        LEFT JOIN Department d ON e.DepartmentID = d.DepartmentID
                        LEFT JOIN Qualification q ON e.QualificationID = q.QualificationID
                        WHERE e.EmployeeID = @EmployeeID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", id);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    employee = new Employee
                    {
                        EmployeeID = (int)reader["EmployeeID"],
                        Name = (string)reader["Name"],
                        Department = (string)reader["DepartmentName"], // Get DepartmentName from joined table
                        Position = (string)reader["Position"],
                        JoiningDate = (DateTime)reader["JoiningDate"],
                        Salary = (decimal)reader["Salary"],
                        Qualification = reader["QualificationName"] as string, // Get QualificationName from joined table
                        Mobile = reader["Mobile"] as string,
                        Email = reader["Email"] as string
                    };
                }
            }
            return employee;
        }


        // Add a new Employee
        public Employee AddEmployee(Employee employee)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO Employee (Name, DepartmentID, Position, JoiningDate, Salary, QualificationID, Mobile, Email) " +
                               "OUTPUT INSERTED.EmployeeID " + // This returns the newly inserted ID
                               "VALUES (@Name, @DepartmentID, @Position, @JoiningDate, @Salary, @QualificationID, @Mobile, @Email)";
                SqlCommand command = new SqlCommand(query, connection);
                // Add values to the placeholders
                command.Parameters.AddWithValue("@Name", employee.Name);
                command.Parameters.AddWithValue("@DepartmentID", employee.DepartmentID);
                command.Parameters.AddWithValue("@Position", employee.Position);
                command.Parameters.AddWithValue("@JoiningDate", employee.JoiningDate);
                command.Parameters.AddWithValue("@Salary", employee.Salary);
                command.Parameters.AddWithValue("@QualificationID", employee.QualificationID);
                command.Parameters.AddWithValue("@Mobile", employee.Mobile);
                command.Parameters.AddWithValue("@Email", employee.Email);

                connection.Open();
                employee.EmployeeID = (int)command.ExecuteScalar(); // Get the new EmployeeID
                connection.Close();
            }
            return employee; // Return the newly created employee with its ID
        }


        // Update an existing Employee
        public void UpdateEmployee(Employee employee)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Check if the DepartmentID exists in the Department table
                string checkDepartmentQuery = "SELECT COUNT(1) FROM Department WHERE DepartmentID = @DepartmentID";
                using (SqlCommand checkCommand = new SqlCommand(checkDepartmentQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@DepartmentID", employee.DepartmentID);
                    int departmentExists = (int)checkCommand.ExecuteScalar();

                    if (departmentExists == 0)
                    {
                        // Handle the case where the DepartmentID does not exist
                        throw new Exception("The specified Department does not exist.");
                    }
                }

                // Proceed with the update if the DepartmentID is valid
                string updateQuery = "UPDATE Employee SET Name = @Name, DepartmentID = @DepartmentID, Position = @Position, " +
                                     "JoiningDate = @JoiningDate, Salary = @Salary, QualificationID = @QualificationID, " +
                                     "Mobile = @Mobile, Email = @Email WHERE EmployeeID = @EmployeeID";

                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@EmployeeID", employee.EmployeeID);
                    updateCommand.Parameters.AddWithValue("@Name", employee.Name);
                    updateCommand.Parameters.AddWithValue("@DepartmentID", employee.DepartmentID);
                    updateCommand.Parameters.AddWithValue("@Position", employee.Position);
                    updateCommand.Parameters.AddWithValue("@JoiningDate", employee.JoiningDate);
                    updateCommand.Parameters.AddWithValue("@Salary", employee.Salary);
                    updateCommand.Parameters.AddWithValue("@QualificationID", employee.QualificationID);
                    updateCommand.Parameters.AddWithValue("@Mobile", employee.Mobile);
                    updateCommand.Parameters.AddWithValue("@Email", employee.Email);

                    updateCommand.ExecuteNonQuery();
                }
            }
        }


        // Delete an Employee
        public void DeleteEmployee(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Employee WHERE EmployeeID = @EmployeeID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Search an Employee
        public IEnumerable<Employee> SearchEmployees(string query, string name, string department, string position, string qualification)
        {
            var employees = new List<Employee>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // Updated SQL query to include a join with Department and Qualification
                var sqlQuery = new List<string>
        {
            "SELECT e.EmployeeID, e.Name, d.Name AS DepartmentName, e.Position, e.JoiningDate, e.Salary, q.Name AS QualificationName, e.Mobile, e.Email",
            "FROM Employee e",
            "JOIN Department d ON e.DepartmentID = d.DepartmentID",
            "JOIN Qualification q ON e.QualificationID = q.QualificationID WHERE 1=1"  // Join with Qualification table based on QualificationID
        };

                // Add optional filters
                if (!string.IsNullOrEmpty(query))
                    sqlQuery.Add("AND (e.Name LIKE @Query OR d.Name LIKE @Query OR e.Position LIKE @Query OR q.Name LIKE @Query)"); // Include the qualification name from the Qualification table
                if (!string.IsNullOrEmpty(name))
                    sqlQuery.Add("AND e.Name LIKE @Name");
                if (!string.IsNullOrEmpty(department))
                    sqlQuery.Add("AND d.Name LIKE @Department");  // Filter by Department table
                if (!string.IsNullOrEmpty(position))
                    sqlQuery.Add("AND e.Position LIKE @Position");
                if (!string.IsNullOrEmpty(qualification))
                    sqlQuery.Add("AND q.Name LIKE @Qualification");  // Filter by Qualification table using qualification name

                var commandText = string.Join(" ", sqlQuery);
                using SqlCommand command = new SqlCommand(commandText, connection);

                // Add parameters for each filter
                if (!string.IsNullOrEmpty(query))
                    command.Parameters.AddWithValue("@Query", "%" + query + "%");
                if (!string.IsNullOrEmpty(name))
                    command.Parameters.AddWithValue("@Name", "%" + name + "%");
                if (!string.IsNullOrEmpty(department))
                    command.Parameters.AddWithValue("@Department", "%" + department + "%");
                if (!string.IsNullOrEmpty(position))
                    command.Parameters.AddWithValue("@Position", "%" + position + "%");
                if (!string.IsNullOrEmpty(qualification))
                    command.Parameters.AddWithValue("@Qualification", "%" + qualification + "%");

                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {

                    employees.Add(new Employee
                    {
                        EmployeeID = (int)reader["EmployeeID"],
                        Name = (string)reader["Name"],
                        Department = (string)reader["DepartmentName"], // Use the alias for department name
                        Position = (string)reader["Position"],
                        JoiningDate = (DateTime)reader["JoiningDate"],
                        Salary = (decimal)reader["Salary"],
                        Qualification = (string)reader["QualificationName"], // Use the alias for qualification name from the Qualification table
                        Mobile = reader["Mobile"] as string,
                        Email = reader["Email"] as string
                    });
                }
            }
            return employees;
        }

    }
}
