namespace OrderDispatcher.Core.Constants
{
    public static class ConfigConstants
    {
        public const string DefaultConnection = "DefaultConnection";
        public const string WarehouseDb = "WarehouseDb";

        public const string RabbitMq = "RabbitMq";
        public const string RabbitMqHost = RabbitMq + ":Host";
        public const string RabbitMqUser = RabbitMq + ":Username";
        public const string RabbitMqPassword = RabbitMq + ":Password";
        public const string RabbitMqApiPort = RabbitMq + ":Api:Port";
        public const string RabbitMqAmqpPort = RabbitMq + ":Amqp:Port";

        public const string Ftp = "Ftp";
        public const string FtpServerName = Ftp + ":Server:Name";
        public const string FtpHost = Ftp + ":Host";
        public const string FtpUser = Ftp + ":Username";
        public const string FtpPassword = Ftp + ":Password";

        public const string FtpNewFolder = "orders/new";
        public const string FtpArchiveFolder = "orders/archive";
        public const string FtpErrorFolder = "orders/error";
    }
}