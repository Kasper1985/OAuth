using System;
using System.Collections.Generic;

namespace OAuthServer.Models
{
    public class NewsBundle
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<News> News { get; set; }
    }

    public class News
    {
        public DateTime Date { get; set; }
        public string Message { get; set; }
    }
}