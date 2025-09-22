using EmployeeMaster.Models.Attendance;
using EmployeeMaster.Models.Department;
using EmployeeMaster.Models.Employee;
using EmployeeMaster.Models.Qualification;
using EmployeeMaster.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
namespace EmployeeMaster.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IQualificationRepository _qualificationRepository;
        private readonly IAttendanceRepository _attendanceRepository;



        // Constructor injection of the repository
        public EmployeeController(IEmployeeRepository employeeRepository, IDepartmentRepository departmentRepository, IQualificationRepository qualificationRepository, IAttendanceRepository attendanceRepository)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _qualificationRepository = qualificationRepository;
            _attendanceRepository =attendanceRepository;
    }


        public IActionResult Index()
        {
            var departments = _departmentRepository.GetDepartments(); // Assuming this returns a list of departments
            ViewBag.Department = departments;

            var qualifications = _qualificationRepository.GetQualifications(); // Assuming this returns a list of departments
            ViewBag.Qualification = qualifications;

            var employees = _employeeRepository.GetAllEmployees(); // Get employees
            return View(employees);
        }


        // GET: /Employee/Create
        public IActionResult Create()
        {

            ; return View();
        }


        // POST: /Employee/Create
        [HttpPost]
        public IActionResult Create([FromBody] Employee employee)
        {


            if (ModelState.IsValid)
            {
                // Add the new employee to the database
                var createdEmployee = _employeeRepository.AddEmployee(employee);

                // Get the department name by ID
                var department = _departmentRepository.GetDepartmentById(employee.DepartmentID);
                var qualification = _qualificationRepository.GetQualificationById(employee.QualificationID);

                return Json(new
                {
                    success = true,
                    employee = new
                    {
                        createdEmployee.EmployeeID,
                        createdEmployee.Name,
                        createdEmployee.Position,
                        createdEmployee.JoiningDate,
                        createdEmployee.Salary,
                        createdEmployee.Mobile,
                        createdEmployee.Email,
                        DepartmentID = employee.DepartmentID,
                        DepartmentName = department?.Name,
                        QualificationID = employee.QualificationID,
                        QualificationName = qualification?.Name
                    }
                });
            }

            // If the model state is invalid, return the error messages
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, errorMessage = string.Join(", ", errors) });
        }


        // GET: /Employee/Edit/{id}
        public IActionResult Edit(int id)
        {
            var employee = _employeeRepository.GetEmployee(id);
            ViewBag.Department = _departmentRepository.GetDepartments();
            return View(employee);
        }


        // POST: /Employee/Edit/{id}
        [HttpPost]
        public IActionResult Edit(Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Update employee in the database
                    _employeeRepository.UpdateEmployee(employee);
                    return Json(new { success = true, employee });
                }
                catch (InvalidOperationException ex)
                {
                    return Json(new { success = false, errors = new List<string> { ex.Message } });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            _employeeRepository.DeleteEmployee(id);
            return Json(new { success = true }); // Return a JSON response indicating success
        }



        [HttpPost]
        [Route("Employee/DeleteMultiple")]
        public IActionResult DeleteMultiple([FromBody] List<int> ids) //expects a list
        {
            if (ids == null || !ids.Any()) // Check if ids is null or empty
            {
                return BadRequest("No employee IDs provided."); // Return a bad request if null
            }

            try
            {
                foreach (var id in ids)
                {
                    _employeeRepository.DeleteEmployee(id);
                }
                TempData["SuccessMessage"] = "Selected employees have been deleted successfully.";
                return Json(new { success = true }); // Return a JSON response
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // GET: /Employee/Details/{id}
        public IActionResult Details(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            return employee == null ? NotFound() : View(employee);
        }

        // GET: /Employee/Search?query=your_search_term


        public IActionResult Search(string query, string name, string department, string position, string qualification)
        {
            // Populate departments for the dropdown
            ViewBag.Department = _departmentRepository.GetDepartments(); // Load department list for dropdown

            IEnumerable<Employee> employees = _employeeRepository.SearchEmployees(query, name, department, position, qualification);
            return View("Index", employees);
        }
        // Action to get qualifications by department


        [HttpGet]
        public JsonResult QualificationsByDepartment(int departmentId)
        {
            try
            {
                // Fetch qualifications based on the DepartmentID
                var qualifications = _qualificationRepository.GetQualificationsByDepartment(departmentId);
                return Json(qualifications);
            }
            catch (Exception ex)
            {
                // Log error and return empty list or error message
                Console.WriteLine(ex.Message);
                return Json(new { error = "An error occurred while fetching qualifications." });
            }
        }


        [HttpGet]
        public IActionResult Attendance(int employeeId)
        {
            // Fetch employee details, including department and qualifications
            var employee = _employeeRepository.GetEmployee(employeeId);
            var departments = _departmentRepository.GetDepartments();
            var qualifications = _qualificationRepository.GetQualificationsByDepartment(employee.DepartmentID);

            // Set employee and qualifications data in ViewBag 
            ViewBag.Department = departments;
            ViewBag.Employee = employee;
            ViewBag.Qualifications = qualifications ?? new List<Qualification>(); // Fallback to an empty list if null

            var viewModel = new EmployeeAttendanceViewModel
            {
                Employee = employee,
                AttendanceRecords = _attendanceRepository.GetAttendanceDetailsByEmployeeId(employeeId)
            };

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult AddAttendance([FromBody] AttendanceDetail attendanceData)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save the attendance and get the generated AttendanceID
                    int attendanceId = _attendanceRepository.AddAttendance(attendanceData);

                    // Ensure that the attendanceId is returned in the response
                    return Json(new { success = true, AttendanceID = attendanceId });  // Return the generated AttendanceID
                }
                catch (Exception ex)
                {
                    // Log the exception as needed
                    return Json(new { success = false, message = ex.Message });
                }
            }

            return Json(new { success = false, message = "Invalid data" });
        }
    

        [HttpPost]
        public IActionResult EditAttendance([FromBody] AttendanceDetail updatedAttendance)
        {
            try
            {
                if (updatedAttendance == null)
                {
                    return Json(new { success = false, message = "Invalid attendance data." });
                }

                // Call the UpdateAttendance method in your repository
                _attendanceRepository.UpdateAttendance(updatedAttendance);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }


        [HttpPost]
        public JsonResult DeleteAttendance(int attendanceId)
        {
            try
            {
                _attendanceRepository.DeleteAttendance(attendanceId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }


        [HttpPost]
        [Route("Employee/DeleteMultipleAttendances")]
        public IActionResult DeleteMultipleAttendances([FromBody] List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any()) // Check if attendanceIds is null or empty
            {
                return BadRequest("No attendance IDs provided."); // Return a bad request if null
            }

            try
            {
                foreach (var id in selectedIds)
                {
                    _attendanceRepository.DeleteAttendance(id); // Adjust this line if needed
                }
                return Json(new { success = true }); // Return a JSON response
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }
}
