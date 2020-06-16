using System.Threading.Tasks;

using Models;

namespace Data.Interfaces
{
    public interface IUserSource
    {
        Task<User> GetUserAsync(int userId);
        Task<User> LoginAsync(string login, string password);
    }
}
