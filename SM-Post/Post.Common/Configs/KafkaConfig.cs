namespace Post.Common.Configs
{
    public class KafkaConfig
    {
        public string BootstrapServers { get; set; }
        public string GroupId { get; set; }
        public string AutoOffsetReset { get; set; }
        public bool EnableAutoCommit { get; set; }
        public bool AllowAutoCreateTopics { get; set; }
    }
}
