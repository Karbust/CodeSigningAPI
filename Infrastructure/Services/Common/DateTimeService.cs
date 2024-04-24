using Application.Common.Interfaces;

namespace Infrastructure.Services.Common;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
}