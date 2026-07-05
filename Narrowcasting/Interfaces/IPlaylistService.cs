using Narrowcasting.Models;

namespace Narrowcasting.Interfaces
{
    public interface IPlaylistService
    {
        Task<IEnumerable<Playlist>> GetAllAsync();
        Task<IEnumerable<Playlist>> GetByDepartmentAsync(int departmentId);
        Task<Playlist?> GetByIdAsync(int id);
        Task<Playlist?> GetByScreenAsync(int screenId);
        Task CreateAsync(Playlist playlist, string userId);
        Task UpdateAsync(Playlist playlist, string userId);
        Task DeleteAsync(int id, string userId);
        Task AddItemAsync(int playlistId, PlaylistItem item, string userId);
        Task RemoveItemAsync(int itemId, string userId);
    }
}
