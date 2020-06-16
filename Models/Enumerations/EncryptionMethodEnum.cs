using System.ComponentModel;

namespace Models.Enumerations
{
    [DefaultValue(S256)]
    public enum EncryptionMethod
    {
        Plain = 0,
        S256 = 1,
    }
}
