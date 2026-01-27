using Nop.Core.Domain.Customers;

namespace Nop.Services.Authentication
{
    public partial interface IAuthenticationService
    {
        void SignIn(Customer customer, bool createPersistentCookie);
        void SignOut();
        Customer GetAuthenticatedCustomer();
    }
}

