using System;

namespace AstroFinance.Application.Users.Commands.DeleteUser
{
    public class DeleteUserCommand
    {
        public Guid Id { get; set; }

        public DeleteUserCommand()
        {
        }

        public DeleteUserCommand(Guid id)
        {
            Id = id;
        }
    }
}
