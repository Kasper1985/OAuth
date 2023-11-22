namespace Models
{
    public class AuthCode
    {
        public string ClientId { get; set; }
        public string AuthorizationCode { get; set; }
        public string AuthorizationTicket { get; set; }
    }
}
