using Microsoft.EntityFrameworkCore;
using Rakipbul.Models;
using Rakipbul.Services.IServices;
using Rakipbul.ViewModels;
using RakipBul.Data;

public class PanoramaService : IPanoramaService
{
    private readonly ApplicationDbContext _context;

    public PanoramaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PanoramaItemDto>> GetFilteredAsync(PanoramaFilterModel filter)
    {
        var q = _context.PanoramaEntries.AsQueryable();

        if (filter.Category.HasValue)
            q = q.Where(x => x.Category == filter.Category.Value);

        if (filter.LeagueId.HasValue)
            q = q.Where(x => x.LeagueId == filter.LeagueId.Value);

        if (filter.SeasonId.HasValue)
            q = q.Where(x => x.SeasonId == filter.SeasonId.Value);

        if (filter.StartDate.HasValue)
            q = q.Where(x => x.StartDate >= filter.StartDate);

        if (filter.EndDate.HasValue)
            q = q.Where(x => x.EndDate <= filter.EndDate);

        var items = await q.OrderByDescending(x => x.CreatedAt)
            .Select(x => new PanoramaItemDto
            {
                Id = x.Id,
                Category = x.Category,
                Title = x.Title,
                YoutubeEmbedLink = x.YoutubeEmbedLink,
                StartDate = x.StartDate,
                EndDate = x.EndDate,

                // Player
                PlayerId = x.PlayerId,
                PlayerName = x.PlayerName,
                PlayerImageUrl = x.PlayerImageUrl,
                PlayerPosition = x.PlayerPosition,

                // Team
                TeamId = x.TeamId,
                TeamName = x.TeamName,
                TeamImageUrl = x.TeamImageUrl,

                // League
                LeagueId = x.LeagueId,
                LeagueName = x.LeagueName,
                ProvinceName = x.ProvinceName
            })
            .ToListAsync();

        return items;
    }

    public async Task AddAsync(PanoramaEntry entry)
    {
        _context.PanoramaEntries.Add(entry);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var e = await _context.PanoramaEntries.FindAsync(id);
        if (e != null)
        {
            _context.PanoramaEntries.Remove(e);
            await _context.SaveChangesAsync();
        }
    }
}
