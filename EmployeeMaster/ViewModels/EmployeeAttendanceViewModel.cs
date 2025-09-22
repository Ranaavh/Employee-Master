using EmployeeMaster.Models.Attendance;
using EmployeeMaster.Models.Employee;

namespace EmployeeMaster.ViewModels
{
    public class EmployeeAttendanceViewModel
    {
        public Employee Employee { get; set; } // Holds employee details
        public IEnumerable<AttendanceDetail> AttendanceRecords { get; set; } // Holds attendance records for the employee
    }
}
