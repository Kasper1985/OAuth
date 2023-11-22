using System.Threading.Tasks;

using Models;

namespace BusinessLogic.Interfaces
{
    public interface IUserLogic
    {
        Task<User> GetUserAsync(int userId);
        Task<User> LoginAsync(string login, string password);
        Task GenerateNewPasswordAsync(string email, string nameLast = null, string nameFirst = null);
    }
}
