using IzracunInvalidnostiBlazor.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Threading.Tasks;

namespace IzracunInvalidnostiBlazor.Services
{
    public class OcenjevalniModelSessionService
    {
        private readonly ProtectedSessionStorage _sessionStorage;
    }

        private readonly IConfiguration _config;
        public OcenjevalniModel OcenjevalniModel { get; set; }
        public OcenjevalniModelSessionService(IConfiguration config)
        {
            _config = config;
            OcenjevalniModel = new OcenjevalniModel();
            //ocenjevalniModel = new OcenjevalniModel();
        }
    }

}