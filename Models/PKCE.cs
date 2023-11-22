using Models.Enumerations;

namespace Models
{
    public class PKCE
    {
        public string ClientId { get; set; }
        public string CodeChallenge { get; set; }
        public EncryptionMethod CodeChallengeMethod { get; set; }
    }
}
