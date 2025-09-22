using Microsoft.AspNetCore.Mvc;

namespace EmployeeMaster.Models.Attendance
{
    public interface IAttendanceRepository
    {

        IEnumerable<AttendanceDetail> GetAttendanceDetailsByEmployeeId(int employeeId);

        public int AddAttendance(AttendanceDetail attendance);

        public void UpdateAttendance(AttendanceDetail attendance);
   
        public void DeleteAttendance(int attendanceId);
    }
}
