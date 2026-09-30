namespace Post.Query.Infrastructure.Config
{
    public class SqlServerConfig
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }

        public string GetConnectionString()
        {
            return $"Server={Server},{Port};Database={Database};User Id={User};Password={Password};";
        }
    }
}
