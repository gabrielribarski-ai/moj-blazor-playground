using IzracunInvalidnostiBlazor.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Threading.Tasks;

namespace IzracunInvalidnostiBlazor.Services
{
    public class UserSessionStorageService
    {
        private readonly ProtectedSessionStorage _session;
        private readonly DataDBLoader _data;
        public PrijavljenUporabnik? CurrentUser { get; private set; }

        public UserSessionStorageService(ProtectedSessionStorage session, DataDBLoader data)
        {
            _session = session;
            _data = data;
        }

        public async Task<PrijavljenUporabnik?> LoadUserAsync()
        {
            if (CurrentUser != null) return CurrentUser;
            var res = await _session.GetAsync<PrijavljenUporabnik>("PrijavljenUporabnik");
            CurrentUser = res.Success ? res.Value : null;
            return CurrentUser;
        }

        public async Task EnsurePogojiAsync()
        {
            if (CurrentUser == null) return;
            CurrentUser.OcenjevalniModel ??= new OcenjevalniModel();
            if (CurrentUser.OcenjevalniModel.PogojSeznam == null)
                CurrentUser.OcenjevalniModel.PogojSeznam = await _data.LoadPogojSeznamAsync();
        }

        public async Task PrepareZaPogojAsync(string pogojId)
        {
            if (CurrentUser == null) return;
            var m = CurrentUser.OcenjevalniModel ??= new OcenjevalniModel();
            m.SegmentSeznam = await _data.LoadSegmentSeznamAsync();
            await _data.PreberiInPoveziAtributeDBAsync(m);
            await CurrentUser.SetIzbranPogoj(pogojId);
            await _data.LoadStopnjeAsync(m, pogojId);
            await SaveUserAsync(CurrentUser);
        }

        public async Task SaveUserAsync(PrijavljenUporabnik user)
        {
            CurrentUser = user;
            await _session.SetAsync("PrijavljenUporabnik", user);
        }

        public async Task SaveSessionToStorage()
        {
            if (CurrentUser != null)
                await _session.SetAsync("PrijavljenUporabnik", CurrentUser);
        }
        public async Task ClearSessionFromStorage()
        {
            CurrentUser = null;
            await _session.DeleteAsync("PrijavljenUporabnik");
        }

    }
}
