using Models.Enumerations;

namespace Models
{
    public class Client
    {
        public string ID { get; set; }
        public string Secret { get; set; }
        public string URI { get; set; }
        public string Name { get; set; }
        public string Developer { get; set; }
        public ClientType Type { get; set; }
        public int UserID { get; set; }

        public static bool operator ==(Client left, Client right)
        {
            if (object.ReferenceEquals(left, null))
                return object.ReferenceEquals(right, null);

            return left.Equals(right);
        }

        public static bool operator !=(Client left, Client right)
        {
            if (object.ReferenceEquals(left, null))
                return !object.ReferenceEquals(right, null);

            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var client = obj as Client;
            if (client == null)
                return false;

            return this.ID.Equals(client.ID);
        }

        public override int GetHashCode() => this.ID.GetHashCode() ^ this.URI.GetHashCode();

        public override string ToString() => $"{{{this.ID}}} {this.Name}: {this.URI}";
    }
}
