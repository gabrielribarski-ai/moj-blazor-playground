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



    }
}