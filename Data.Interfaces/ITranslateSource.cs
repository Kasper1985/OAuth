using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface ITranslateSource
    {
        Task<string> GetTranslationAsync(string text, string ccode);
    }
}
