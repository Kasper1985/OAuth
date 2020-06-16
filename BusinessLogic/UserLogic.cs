using System;
using System.Threading.Tasks;

using Models;
using BusinessLogic.Interfaces;
using Data.Interfaces;

namespace BusinessLogic
{
    public class UserLogic : IUserLogic
    {
        private readonly IUserSource userSource;

        public UserLogic(IUserSource userSource) => this.userSource = userSource ?? throw new ArgumentNullException(nameof(userSource));

        public Task<User> GetUserAsync(int userId)
        {
            return this.userSource.GetUserAsync(userId);
        }

        public Task<User> LoginAsync(string login, string password)
        {
            return this.userSource.LoginAsync(login, password);
        }
    }
}
