namespace Models
{
    public class UserScope
    {
        public int UserId { get; set; }
        public string ClientId { get; set; }
        public Scope Scope { get; set; }
        public bool Grant { get; set; }
    }
}
