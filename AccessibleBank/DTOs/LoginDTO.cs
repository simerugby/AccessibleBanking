//Declares the namespace, grouping data transfer objects for the project
namespace AccessibleBank.DTOs
{
    //Defines class, a simple data container used when logging in
    //Dto stands for Data Transfer Object, representing the shape of data sent over the network
    public class LoginDto
    {
        //Declares a public Email and Password property of type string, with its getter and setter for read/write
        //Initialized to string.Empty to ensure it's never null
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
