namespace Service.Models;

using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("calendar_events")]
public class MSupabaseEvent : BaseModel
{
    [PrimaryKey("id", shouldInsert: true)]
    public string Id { get; set; } = string.Empty;

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Column("end_time")]
    public DateTime EndTime { get; set; }

    [Column("priority")]
    public string Priority { get; set; } = "Low";

    [Column("color")]
    public string Color { get; set; } = "#3E6BEC";

    [Column("attendees")]
    public object Attendees { get; set; }
}