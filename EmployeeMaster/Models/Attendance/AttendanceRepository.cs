using System.Data.SqlClient;

namespace EmployeeMaster.Models.Attendance
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly string _connectionString;

        public AttendanceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        // Get Attendance Details for a specific Employee
        public IEnumerable<AttendanceDetail> GetAttendanceDetailsByEmployeeId(int employeeId)
        {
            var attendanceDetails = new List<AttendanceDetail>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"SELECT a.AttendanceID, 
                                 a.EmployeeID, 
                                 a.Date,
                                 a.Status, 
                                 a.TotalHours, 
                                 a.LeaveType 
                                 FROM AttendanceDetail a 
                                 WHERE a.EmployeeID = @EmployeeID"; // Filter by EmployeeID

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", employeeId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        attendanceDetails.Add(new AttendanceDetail
                        {
                            AttendanceID = reader.GetInt32(0),
                            EmployeeID = reader.GetInt32(1),
                            Date = reader.GetDateTime(2),
                            Status = reader.GetString(3),
                            TotalHours = reader.GetDecimal(4),
                            LeaveType = reader.IsDBNull(5) ? null : reader.GetString(5)
                        });
                    }
                }
            }
            return attendanceDetails;
            }

        // Add a new Attendance Record to the database
        public int AddAttendance(AttendanceDetail attendance)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Modified query to include SCOPE_IDENTITY() to get the new ID
                var query = @"INSERT INTO AttendanceDetail (EmployeeID, Date, Status, TotalHours, LeaveType) 
                      VALUES (@EmployeeID, @Date, @Status, @TotalHours, @LeaveType);
                      SELECT CAST(SCOPE_IDENTITY() AS int);"; // Retrieve the new ID

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", attendance.EmployeeID);
                command.Parameters.AddWithValue("@Date", attendance.Date);
                command.Parameters.AddWithValue("@Status", attendance.Status);
                command.Parameters.AddWithValue("@TotalHours", attendance.TotalHours);
                command.Parameters.AddWithValue("@LeaveType", (object)attendance.LeaveType ?? DBNull.Value);

                connection.Open();

                // Log the query for debugging
                Console.WriteLine($"Executing SQL: {command.CommandText}");
                foreach (SqlParameter param in command.Parameters)
                {
                    Console.WriteLine($"Parameter: {param.ParameterName}, Value: {param.Value}");
                }

                // Execute the query and get the generated AttendanceID
                int attendanceId = (int)command.ExecuteScalar();

                return attendanceId; // Return the new ID
            }
        }
        public void UpdateAttendance(AttendanceDetail attendance)
        {
            // Proceed with the update if the attendance ID is valid
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string updateQuery = "UPDATE AttendanceDetail SET Date = @Date, Status = @Status, TotalHours = @TotalHours, " +
                                     "LeaveType = @LeaveType WHERE AttendanceID = @AttendanceID";

                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@AttendanceID", attendance.AttendanceID);
                    updateCommand.Parameters.AddWithValue("@Date", attendance.Date.ToString("yyyy-MM-dd"));

                    updateCommand.Parameters.AddWithValue("@Status", attendance.Status);
                    updateCommand.Parameters.AddWithValue("@TotalHours", attendance.TotalHours);
                    updateCommand.Parameters.AddWithValue("@LeaveType", attendance.LeaveType);

                    updateCommand.ExecuteNonQuery();
                }
            }
        }

        public void DeleteAttendance(int attendanceId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))

                {
                    string query = "DELETE FROM AttendanceDetail WHERE AttendanceID = @AttendanceID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@AttendanceID", attendanceId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it appropriately
                // Example: throw new InvalidOperationException("Error deleting attendance record.", ex);
                throw new Exception("An error occurred while deleting the attendance record.", ex);
            }
        }

 
    }
}
