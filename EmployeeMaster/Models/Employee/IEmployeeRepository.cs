using EmployeeMaster.Models.Employee;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

public interface IEmployeeRepository
{
    IEnumerable<Employee> GetAllEmployees();
    Employee GetEmployee(int id);
    Employee AddEmployee(Employee employee);
    void UpdateEmployee(Employee employee);
    void DeleteEmployee(int id);
  

    IEnumerable<Employee> SearchEmployees(string query, string name, string department, string position, string qualification);


}
