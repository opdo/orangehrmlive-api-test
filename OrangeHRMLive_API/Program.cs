// See https://aka.ms/new-console-template for more information
using OrangeHRMLive_API.Helpers;
using OrangeHRMLive_API.Models;

APIHelper apiHelper = new APIHelper("https://opensource-demo.orangehrmlive.com");

// Login
Console.WriteLine("Login...");
apiHelper.Login("Admin", "admin123");

// Add employee API
Console.WriteLine("Call Add Employee API");

// Prepare data obj
var data = new EmployeeRequest
{
    employeeId = "0300",
    firstName = "Test1",
    middleName = "Test2",
    lastName = "Test3",
};

var result = apiHelper.AddEmployee(data);

Console.WriteLine("Created empNumber: " + result.empNumber + " successful");
Console.ReadLine();
