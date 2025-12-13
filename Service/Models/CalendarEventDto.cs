namespace Service.Models;

public class CalendarEventDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Priority { get; set; } = "Low";
    public string Color { get; set; } = "#3E6BEC";
    public object? Attendees { get; set; }
    public List<string>? AttendeeIds { get; set; }
}

public class CalendarEventDeleteDto
{
    public string Id { get; set; } = string.Empty;
}