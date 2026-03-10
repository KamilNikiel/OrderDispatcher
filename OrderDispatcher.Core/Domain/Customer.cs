namespace OrderDispatcher.Core.Domain
{
    public class Customer
    {
        public string FullName { get; private set; } = null!;
        public string Email { get; private set; } = null!;

        private Customer() { }

        public Customer(string fullName, string email)
        {
            FullName = fullName;
            Email = email;
        }
    }
}