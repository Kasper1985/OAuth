namespace Models
{
    public class AuthCode
    {
        public string ClientID { get; set; }
        public string AuthorizationCode { get; set; }
        public string AuthorizationTicket { get; set; }
    }
}
