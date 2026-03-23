using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces
{
    public interface IProfilService
    {
        Task UpdateAsync(Profil item, ProfilEinstellungen settings);
    }
}
