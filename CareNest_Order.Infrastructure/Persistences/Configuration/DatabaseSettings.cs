namespace CareNest_Order.Infrastructure.Persistences.Configuration
{
    public class DatabaseSettings
    {
        public string? Ip { get; set; }
        public int Port { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
        public string? Database { get; set; }
        public string GetConnectionString()
        {
            Console.WriteLine($"Host={Ip};Port={Port};Database={Database};Username={User};Password={Password}");

            return $"Host={Ip};Port={Port};Database={Database};Username={User};Password={Password}";
        }
        public Action Display => () =>
        {
            Console.WriteLine("----- Database Settings -----");
            Console.WriteLine($"IP       : {Ip}");
            Console.WriteLine($"Port     : {Port}");
            Console.WriteLine($"User     : {User}");
            Console.WriteLine($"Password : {Password}");
            Console.WriteLine($"Database : {Database}");
            Console.WriteLine();
        };
    }
}
