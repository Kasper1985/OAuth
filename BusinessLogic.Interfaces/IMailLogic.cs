using System.Threading.Tasks;

using Models;

namespace BusinessLogic.Interfaces
{
    public interface IMailLogic
    {
        Task SendPasswordMailAsync(string password, User user, string cCode = "de");
    }
}
