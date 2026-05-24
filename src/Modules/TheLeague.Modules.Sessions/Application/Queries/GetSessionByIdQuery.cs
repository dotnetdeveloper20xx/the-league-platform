using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Sessions.Application.Dtos;
using TheLeague.Modules.Sessions.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Sessions.Application.Queries;

public record GetSessionByIdQuery(Guid SessionId) : IRequest<Result<SessionDetailDto>>;

public class GetSessionByIdQueryHandler : IRequestHandler<GetSessionByIdQuery, Result<SessionDetailDto>>
{
    private readonly SessionsDbContext _db;

    public GetSessionByIdQueryHandler(SessionsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<SessionDetailDto>> Handle(GetSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await _db.Sessions
            .AsNoTracking()
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
            return Result.Failure<SessionDetailDto>("Session not found.");

        var bookingDtos = session.Bookings.Select(b => new SessionBookingDto(
            b.Id, b.SessionId, b.MemberId, b.Status, b.BookedAt, b.CancelledAt
        )).ToList();

        var dto = new SessionDetailDto(
            session.Id, session.Title, session.Category,
            session.VenueId, session.VenueName,
            session.StartTime, session.EndTime, session.Duration,
            session.Capacity, session.Fee, session.CurrentBookingCount,
            session.Capacity - session.CurrentBookingCount,
            session.IsCancelled, session.CancellationReason,
            session.CancellationDeadlineHours, session.CreatedAt,
            bookingDtos);

        return Result.Success(dto);
    }
}
