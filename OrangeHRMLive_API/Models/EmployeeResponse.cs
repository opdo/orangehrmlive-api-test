namespace OrangeHRMLive_API.Models
{
    public class EmployeeResponse : EmployeeRequest
    {
        public int empNumber { get; set; }
        public object terminationId { get; set; }
    }
}
