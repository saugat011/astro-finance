using System;

namespace AstroFinance.Application.Users.Queries.GetUserById
{
    public class GetUserByIdQuery
    {
        public Guid Id { get; set; }

        public GetUserByIdQuery()
        {
        }

        public GetUserByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
