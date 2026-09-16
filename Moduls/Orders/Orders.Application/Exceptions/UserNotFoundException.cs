namespace Orders.Application.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public int UserId { get; }

        public UserNotFoundException(int userId)
            : base($"User with ID {userId} was not found.")
        {
            UserId = userId;
        }
    }
}
