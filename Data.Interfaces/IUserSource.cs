using System.Threading.Tasks;

using Models;

namespace Data.Interfaces
{
    public interface IUserSource
    {
        Task<User> GetUserAsync(int userId);
        Task<User> GetUserAsync(string email);
        Task<User> LoginAsync(string login, string password);
        Task SaveNewPasswordAsync(string password, int userId);
    }
}
