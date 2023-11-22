using Models.Enumerations;

namespace Models
{
    public class Client
    {
        public string Id { get; set; } = "";
        public string Secret { get; set; }
        public string Uri { get; set; } = "";
        public string Name { get; set; }
        public string Developer { get; set; }
        public ClientType Type { get; set; }
        public int UserId { get; set; }

        public static bool operator ==(Client left, Client right)
        {
            return left?.Equals(right) ?? ReferenceEquals(right, null);
        }

        public static bool operator !=(Client left, Client right)
        {
            if (ReferenceEquals(left, null))
                return !ReferenceEquals(right, null);

            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var client = obj as Client;
            return client != null && Id.Equals(client.Id);
        }

        public override int GetHashCode() => Id.GetHashCode() ^ Uri.GetHashCode();

        public override string ToString() => $"{{{Id}}} {Name}: {Uri}";
    }
}
