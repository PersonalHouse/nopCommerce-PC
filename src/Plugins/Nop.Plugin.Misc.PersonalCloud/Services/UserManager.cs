using System;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using SignalServer.Users;

namespace Nop.Plugin.Misc.PersonalCloud.Services
{
    public class UserManager : IUserManager
    {
        private readonly ICustomerService _customerService;
        private readonly ICustomerRegistrationService _customerRegistrationService;

        public UserManager(ICustomerService customerService, ICustomerRegistrationService customerRegistrationService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _customerRegistrationService = customerRegistrationService ?? throw new ArgumentNullException(nameof(customerRegistrationService));
        }

        public UserInfo CheckUserCredentials(string username, string password)
        {
            var loginResult = _customerRegistrationService.ValidateCustomer(username, password);
            if (loginResult == CustomerLoginResults.Successful)
            {
                return GetUserInfo(username);
            }
            return null;
        }

        public UserInfo GetUserInfo(string username)
        {
            var customer = _customerService.GetCustomerByUsername(username);
            if (customer != null)
            {
                return new UserInfo
                {
                    Username = customer.Username,
                    FullName = customer.Username
                };
            }
            return null;
        }
    }
}
