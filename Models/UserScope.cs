namespace Models
{
    public class UserScope
    {
        public int UserID { get; set; }
        public string ClientID { get; set; }
        public Scope Scope { get; set; }
        public bool Grant { get; set; }
    }
}
