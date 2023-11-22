namespace Models
{
    public class User
    {
        public int Id { get; set; }
        public int Salutation { get; set; }
        public string Title { get; set; }
        public string NameFirst { get; set; }
        public string NameLast { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string EMail { get; set; }


        public override string ToString() => $"{{{Id}}} {NameFirst} {NameLast}";
    }
}
