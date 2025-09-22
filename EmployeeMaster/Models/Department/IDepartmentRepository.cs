namespace EmployeeMaster.Models.Department
{
    public interface IDepartmentRepository
    {
        List<Department> GetDepartments();

        Department GetDepartmentById(int departmentId);
    }

}
