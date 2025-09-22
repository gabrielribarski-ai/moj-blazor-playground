using IzracunInvalidnostiBlazor.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Threading.Tasks;

namespace IzracunInvalidnostiBlazor.Services
{
    public class OcenjevalniModelSessionService
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private const string Key = "OcenjevalniModel";

        public OcenjevalniModelSessionService(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        /// <summary>
        /// Naloži model iz session storage ali vrne nov prazen model.
        /// </summary>
        public async Task<OcenjevalniModel> GetAsync()
        {
            var result = await _sessionStorage.GetAsync<OcenjevalniModel>(Key);
            return result.Success && result.Value != null
                ? result.Value
                : new OcenjevalniModel();
        }

        /// <summary>
        /// Shrani trenutni model v session storage.
        /// </summary>
        public async Task SaveAsync(OcenjevalniModel model)
        {
            await _sessionStorage.SetAsync(Key, model);
        }

        /// <summary>
        /// Pobriše model iz session storage.
        /// </summary>
        public async Task ClearAsync()
        {
            await _sessionStorage.DeleteAsync(Key);
        }
    }
}
