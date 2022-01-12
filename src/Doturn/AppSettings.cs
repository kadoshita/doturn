namespace Doturn
{
    public class AppSettings
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Realm { get; set; }
        public string ExternalIPAddress { get; set; }
        public ushort ListeningPort { get; set; }
        public ushort MinPort { get; set; }
        public ushort MaxPort { get; set; }
    }
}