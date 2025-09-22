namespace EmployeeMaster.Models.Qualification
{
    public interface IQualificationRepository
    {

        List<Qualification> GetQualifications();

        Qualification GetQualificationById(int qualificationId);

        List<Qualification> GetQualificationsByDepartment(int departmentId);

    }
}
