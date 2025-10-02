using IzracunInvalidnostiBlazor.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Threading.Tasks;

namespace IzracunInvalidnostiBlazor.Services
{
    public class UserSessionStorageService
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private const string UserKey = "PrijavljenUporabnik";

        public UserSessionStorageService(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        // 🔹 Shrani uporabnika v session storage
        public async Task SaveUserAsync(PrijavljenUporabnik user)
        {
            await _sessionStorage.SetAsync(UserKey, user);
        }

        // 🔹 Prebere uporabnika iz session storage
        public async Task<PrijavljenUporabnik?> LoadUserAsync()
        {
            var result = await _sessionStorage.GetAsync<PrijavljenUporabnik>(UserKey);
            return result.Success ? result.Value : null;
        }

        // 🔹 Pobriše uporabnika iz session storage
        public async Task ClearUserAsync()
        {
            await _sessionStorage.DeleteAsync(UserKey);
        }
    }
}
