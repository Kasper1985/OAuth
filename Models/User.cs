namespace Models
{
    public class User
    {
        public enum Salutations
        {
            Frau = 2,
            Herr = 3,
        };

        public int ID { get; set; }
        public string Salutation { get; set; }
        public string Title { get; set; }
        public string NameFirst { get; set; }
        public string NameLast { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string EMail { get; set; }

        public Tenant Tenant { get; set; }


        public override string ToString()
        {
            var tenantPart = "";
            if (this.Tenant != null)
                tenantPart = $"{this.Tenant.ID} ->";
            return $"{{{this.ID}}} {tenantPart} {this.NameFirst} {this.NameLast}";
        }
    }
}
