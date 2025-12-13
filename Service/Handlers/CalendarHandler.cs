namespace Service.Handlers;

using Service.Interfaces;
using Service.Models;
using Utils;

using System.Text.Json;

public class CalendarFetchHandler : AHandler, IHandler
{
    private const string TAG = "CALENDAR-FETCH";
    private ISupabaseRepositoryUser _repository;

    public CalendarFetchHandler(ISupabaseRepositoryUser repository) { _repository = repository; }

    public async Task<MResponse> Execute(JsonElement json)
    {
        Logger.Debug(TAG, "Processing request...");
        try
        {
            UserSessionDto data = _execute<UserSessionDto>(json);

            var rawEvents = await _repository.FetchCalendarEvents(data.AccessToken);

            var eventsDto = rawEvents.Select(Event => new CalendarEventDto
            {

                Id = Event.Id,
                Title = Event.Title,
                Description = Event.Description ?? "",
                StartTime = Event.StartTime,
                EndTime = Event.EndTime,
                Priority = Event.Priority,
                Color = Event.Color,
                Attendees = Event.Attendees ?? new List<object>()
            }).ToList();

            return new MResponse
            {
                status = "Success",
                message = "Events fetched",
                data = JsonSerializer.SerializeToElement(eventsDto)
            };
        }
        catch (Exception error)
        {
            Logger.Error(TAG, "Error fetching events", error);
            return new MResponse { status = "Error", message = error.Message };
        }
    }
}

public class CalendarAddHandler : AHandler, IHandler
{
    private const string TAG = "CALENDAR-ADD";
    private ISupabaseRepositoryUser _repository;

    public CalendarAddHandler(ISupabaseRepositoryUser repository) { _repository = repository; }

    public async Task<MResponse> Execute(JsonElement json)
    {
        Logger.Debug(TAG, "Processing add event request...");
        try
        {
            UserSessionDto session = _execute<UserSessionDto>(json);

            var eventData = _execute<CalendarEventDto>(json);

            if (eventData == null) { throw new Exception("Invalid event data"); }

            bool success = await _repository.AddCalendarEvent(session.AccessToken, eventData);

            if (success) { return new MResponse { status = "Success", message = "Event created successfully." }; }
            else { return new MResponse { status = "Error", message = "Failed to create event in database." }; }
        }
        catch (Exception error) { Logger.Error(TAG, "Error adding event", error); return new MResponse { status = "Error", message = error.Message }; }
    }
}

public class CalendarDeleteHandler : AHandler, IHandler
{
    private const string TAG = "CALENDAR-DELETE";
    private ISupabaseRepositoryUser _repository;

    public CalendarDeleteHandler(ISupabaseRepositoryUser repository) { _repository = repository; }

    public async Task<MResponse> Execute(JsonElement json)
    {
        Logger.Debug(TAG, "Processing delete event request...");
        try
        {
            UserSessionDto session = _execute<UserSessionDto>(json);

            var deleteData = _execute<CalendarEventDeleteDto>(json);

            if (string.IsNullOrEmpty(deleteData.Id)) { throw new Exception("Event ID is missing"); }

            bool success = await _repository.DeleteCalendarEvent(session.AccessToken, deleteData.Id);

            if (success) { return new MResponse { status = "Success", message = "Event deleted successfully." }; }
            else { return new MResponse { status = "Error", message = "Failed to delete event." }; }
        }
        catch (Exception error) { Logger.Error(TAG, "Error deleting event", error); return new MResponse { status = "Error", message = error.Message }; }
    }
}