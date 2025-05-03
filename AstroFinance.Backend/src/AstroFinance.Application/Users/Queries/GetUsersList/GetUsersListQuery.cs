namespace AstroFinance.Application.Users.Queries.GetUsersList
{
    public class GetUsersListQuery
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public GetUsersListQuery(string? searchTerm, int pageNumber, int pageSize)
        {
            SearchTerm = searchTerm;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
