using System;
using System.Collections.Generic;

namespace AstroFinance.Application.Users.Commands.UpdateUser
{
    public class UpdateUserCommand
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; }

        public UpdateUserCommand()
        {
        }

        public UpdateUserCommand(Guid id, string email, string firstName, string lastName, List<string> roles, bool isActive)
        {
            Id = id;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            Roles = roles;
            IsActive = isActive;
        }
    }
}
