namespace Doturn
{
    public interface IAppSettings
    {
        string Username { get; set; }
        string Password { get; set; }
        string Realm { get; set; }
        string ExternalIPAddress { get; set; }
        ushort ListeningPort { get; set; }
        ushort MinPort { get; set; }
        ushort MaxPort { get; set; }
    }
    public class AppSettings : IAppSettings
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