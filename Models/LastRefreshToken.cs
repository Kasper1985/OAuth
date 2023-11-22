using System;

namespace Models
{
    public class LastRefreshToken
    {
        public int UserId { get; set; }
        public string ClientId { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpireTime { get; set; }
    }
}
