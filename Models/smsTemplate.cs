namespace WebApplication1.Models
{
    public class smsTemplate
    {
        public int tid { get; set; }
        public string templateid { get; set; } = null!;
        public required string Content { get; set; } = null!;
        public string? CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedDateTime { get; set; } = DateTime.UtcNow;


        public string? Status { get; set; }
    }
}
