using Ensuranx.Application.Interfaces;
using Ensuranx.Infrastructure.DbContext;

namespace Ensuranx.Infrastructure.Repositories;

public class EventCollectionDeviceRepository : IEventCollectionDeviceRepository
{
    private readonly ApplicationDbContext _context;

    public EventCollectionDeviceRepository(ApplicationDbContext context)
    {
        _context = context;
    }
}
