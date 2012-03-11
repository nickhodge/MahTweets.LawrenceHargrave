using System;

namespace MahApps.Twitter.Models
{
    public class ExceptionResponse : ITwitterResponse
    {
        public String ErrorMessage { get; set; }
        public String Content { get; set; }
    }
}