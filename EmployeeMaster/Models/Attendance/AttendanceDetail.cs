namespace EmployeeMaster.Models.Attendance
{
    public class AttendanceDetail
    {
        public int AttendanceID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime Date { get; set; }
        public string? Status { get; set; }
        public decimal? TotalHours { get; set; }
        public string? LeaveType { get; set; }
    }

}
