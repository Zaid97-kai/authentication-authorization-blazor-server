﻿using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Persistence.Entities;
using Persistence.Context;
using System.Collections.Generic;

namespace AuthenticationAndAuthorization.Data
{
    public class WebsiteAuthenticator : AuthenticationStateProvider
    {
        private readonly ProtectedLocalStorage _protectedLocalStorage;
        private readonly IAuthenticationAndAuthorizationContext _context;

        public WebsiteAuthenticator(ProtectedLocalStorage protectedLocalStorage, IAuthenticationAndAuthorizationContext context)
        {
            _protectedLocalStorage = protectedLocalStorage;
            _context = context;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var principal = new ClaimsPrincipal();

            //try
            //{
                var storedPrincipal = await _protectedLocalStorage.GetAsync<string>("identity");

                if (storedPrincipal.Success)
                {
                    var user = JsonConvert.DeserializeObject<User>(storedPrincipal.Value);
                    var (userInDb, isLookUpSuccess) = LookUpUser(user.Username, user.Password);

                    if (isLookUpSuccess)
                    {
                        var identity = CreateIdentityFromUser(userInDb);
                        principal = new(identity);
                    }
                }
            //}
            //catch
            //{

            //}

            return new AuthenticationState(principal);
        }

        public async Task LoginAsync(LoginFormModel loginFormModel)
        {
            var (userInDatabase, isSuccess) = LookUpUser(loginFormModel.Username, loginFormModel.Password);
            var principal = new ClaimsPrincipal();

            if (isSuccess)
            {
                var identity = CreateIdentityFromUser(userInDatabase);
                principal = new ClaimsPrincipal(identity);
                var ret = JsonConvert.SerializeObject(userInDatabase);
                await _protectedLocalStorage.SetAsync("identity", ret);
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }

        public async Task LogoutAsync()
        {
            await _protectedLocalStorage.DeleteAsync("identity");
            var principal = new ClaimsPrincipal();
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }

        private ClaimsIdentity CreateIdentityFromUser(User user)
        {
            var result = new ClaimsIdentity(new Claim[]
            {
                new (ClaimTypes.Name, user.Username),
                new (ClaimTypes.Hash, user.Password),
                new (AdultRequirement.ClaimName, user.Age.ToString()),
                new ("IsPremiumMember", user.IsPremiumMember.ToString())
            }, "BlazorSchool");

            var roles = _context.UsersRoles.Where(i => i.UserId == user.Id).Select(i => i.Role.Name).ToList();
                                 
            foreach (string role in roles)
            {
                result.AddClaim(new(ClaimTypes.Role, role));
            }

            return result;
        }

        private (User, bool) LookUpUser(string username, string password)
        {
            var result = _context.Users.FirstOrDefault(u => username == u.Username && password == u.Password);

            return (result, result is not null);
        }
    }
}