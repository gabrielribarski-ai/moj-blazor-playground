using IzracunInvalidnostiBlazor.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Threading.Tasks;

namespace IzracunInvalidnostiBlazor.Services
{
    public class OcenjevalniModelSessionService
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private const string Key = "OcenjevalniModel";
    }
}
