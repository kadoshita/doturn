using System;

namespace Doturn
{
    public class AppSettings
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Realm { get; set; }
        public string ExternalIPAddress { get; set; }
        public UInt16 ListeningPort { get; set; }
    }
}