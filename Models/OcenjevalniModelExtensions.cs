using IzracunInvalidnostiBlazor.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Threading.Tasks;

namespace IzracunInvalidnostiBlazor.Extensions
{
    public static class OcenjevalniModelExtensions
    {
        private const string Key = "OcenjevalniModel";

        public static async Task SaveSessionToStorage(this OcenjevalniModel model, ProtectedSessionStorage storage)
        {
            await storage.SetAsync(Key, model);
        }

        public static async Task<OcenjevalniModel> ReadFromSessionStorage(this OcenjevalniModel model, ProtectedSessionStorage storage)
         {
            try
            {
                var result = await storage.GetAsync<OcenjevalniModel>(Key);
                return (result.Success && result.Value != null)
                    ? result.Value
                    : new OcenjevalniModel();
            }
            catch (InvalidOperationException)
            {
                // Interop not ready: return a clean model; caller can retry later
                return new OcenjevalniModel();
            }
        }
         
    }
}
