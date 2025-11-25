using Rakipbul.Models;
using Rakipbul.ViewModels;

namespace Rakipbul.Services.IServices
{
    public interface IPanoramaService
    {
        Task<List<PanoramaItemDto>> GetFilteredAsync(PanoramaFilterModel filter);
        Task DeleteAsync(int id);
        Task AddAsync(PanoramaEntry dto);
    }

}
