using System;

namespace Models
{
    public class LastRefreshToken
    {
        public int UserID { get; set; }
        public string ClientID { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpireTime { get; set; }
    }
}
