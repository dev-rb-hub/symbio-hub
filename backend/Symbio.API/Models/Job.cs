namespace Symbio.API.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ClientSurname { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public string ContactEmail { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = true;
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
    }
}
