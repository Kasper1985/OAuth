using System.ComponentModel;

namespace Models.Enumerations
{
    [DefaultValue("Public")]
    public enum ClientType
    {
        Public = 0,
        Private = 1,
        FirstParty = 2
    }
}
