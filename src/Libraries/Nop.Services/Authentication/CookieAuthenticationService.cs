using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;

namespace Nop.Services.Authentication
{
    /// <summary>
    /// Cookie-based authentication service for ASP.NET Core
    /// Pattern based on official nopCommerce 4.70.5 implementation
    /// </summary>
    public partial class CookieAuthenticationService : IAuthenticationService
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private Customer _cachedCustomer;

        #endregion

        #region Ctor

        public CookieAuthenticationService(
            CustomerSettings customerSettings,
            ICustomerService customerService,
            IHttpContextAccessor httpContextAccessor)
        {
            _customerSettings = customerSettings;
            _customerService = customerService;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sign in - sync version adapted from official async pattern
        /// </summary>
        public virtual void SignIn(Customer customer, bool createPersistentCookie)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            // Create claims based on official 4.70.5 pattern
            var claims = new List<Claim>();

            if (!string.IsNullOrEmpty(customer.Username))
                claims.Add(new Claim(ClaimTypes.Name, customer.Username, ClaimValueTypes.String, "nopCommerce"));

            if (!string.IsNullOrEmpty(customer.Email))
                claims.Add(new Claim(ClaimTypes.Email, customer.Email, ClaimValueTypes.Email, "nopCommerce"));

            claims.Add(new Claim(ClaimTypes.NameIdentifier, customer.CustomerGuid.ToString(), ClaimValueTypes.String, "nopCommerce"));

            // Create identity and principal
            var identity = new ClaimsIdentity(claims, "NopAuthenticationScheme");
            var principal = new ClaimsPrincipal(identity);

            // Set authentication properties
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = createPersistentCookie,
                IssuedUtc = DateTimeOffset.UtcNow
            };

            // Sign in synchronously (official uses async)
            _httpContextAccessor.HttpContext?.SignInAsync("NopAuthenticationScheme", principal, authProperties).GetAwaiter().GetResult();

            // Cache customer
            _cachedCustomer = customer;
        }

        /// <summary>
        /// Sign out - sync version adapted from official
        /// </summary>
        public virtual void SignOut()
        {
            _cachedCustomer = null;
            _httpContextAccessor.HttpContext?.SignOutAsync("NopAuthenticationScheme").GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get authenticated customer - sync version adapted from official
        /// </summary>
        public virtual Customer GetAuthenticatedCustomer()
        {
            // Return cached if available
            if (_cachedCustomer != null)
                return _cachedCustomer;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return null;

            // Authenticate using official pattern
            var authenticateResult = httpContext.AuthenticateAsync("NopAuthenticationScheme").GetAwaiter().GetResult();
            if (!authenticateResult.Succeeded)
                return null;

            Customer customer = null;

            // Get customer by username or email based on settings
            if (_customerSettings.UsernamesEnabled)
            {
                var usernameClaim = authenticateResult.Principal.FindFirst(c => 
                    c.Type == ClaimTypes.Name && 
                    c.Issuer.Equals("nopCommerce", StringComparison.InvariantCultureIgnoreCase));
                
                if (usernameClaim != null)
                    customer = _customerService.GetCustomerByUsername(usernameClaim.Value);
            }
            else
            {
                var emailClaim = authenticateResult.Principal.FindFirst(c => 
                    c.Type == ClaimTypes.Email && 
                    c.Issuer.Equals("nopCommerce", StringComparison.InvariantCultureIgnoreCase));
                
                if (emailClaim != null)
                    customer = _customerService.GetCustomerByEmail(emailClaim.Value);
            }

            // Validate customer status (following official pattern)
            if (customer == null || !customer.Active || customer.Deleted || !customer.IsRegistered())
                return null;

            // Cache and return
            _cachedCustomer = customer;
            return _cachedCustomer;
        }

        #endregion
    }
}
