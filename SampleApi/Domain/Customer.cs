namespace SampleApi.Domain
{
    public class Customer
    {
        public Guid Id { get; set; }
        public bool EmailNotificationsEnabled { get; set; }
        public bool SmsNotificationsEnabled { get; set; }
    }
}