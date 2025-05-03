using System;

namespace AstroFinance.Application.Users.Queries.GetUsersList
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        // Add other relevant user properties as needed
    }
}
