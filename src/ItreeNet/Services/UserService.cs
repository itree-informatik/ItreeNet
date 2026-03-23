using AutoMapper;
using ItreeNet.Data.Enums;
using ItreeNet.Data.Extensions;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ItreeNet.Services
{
    public class UserService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly IMapper _mapper;
        public Benutzer? CurrentUser { get; private set; }

        public UserService(IDbContextFactory<ZeiterfassungContext> dbFactory, IMapper mapper)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
        }

        public async Task<Benutzer> CreateBenutzer(Benutzer benutzer, List<string> groups)
        {
            if (benutzer.Email!.EndsWith("@itree.ch") && benutzer.Uid != null && benutzer.Uid != Guid.Empty)
            {
                await using var context = await _dbFactory.CreateDbContextAsync();

                var azureId = benutzer.Uid;

                var mitarbeiter =
                    await context.TMitarbeiter.SingleOrDefaultAsync(m => m.AzureId == azureId);

                if (mitarbeiter != null)
                {
                    benutzer.MitarbeiterId = mitarbeiter.Id;
                    benutzer.IsMitarbeiter = true;
                    benutzer.IsAuthorized = true;
                    benutzer.IsAuthenticated = true;
                    benutzer.IsIntern = mitarbeiter.Intern;

                    var mitarbeiterAzureId = mitarbeiter.AzureId.ToString();

                    if (!string.IsNullOrEmpty(mitarbeiterAzureId) && Globals.BossList.Contains(mitarbeiterAzureId, StringComparer.OrdinalIgnoreCase))
                    {
                        benutzer.IsAdmin = true;
                    }

                    if(mitarbeiter.Austritt != null && mitarbeiter.Austritt > DateOnly.FromDateTime(DateTime.Now))
                    {
                        benutzer.IsMitarbeiter = false;
                        benutzer.IsAuthorized = false;
                        benutzer.IsAuthenticated = false;
                    }

                    var profil = await context.TProfil.SingleOrDefaultAsync(m => m.MitarbeiterId == mitarbeiter.Id);

                    if (profil != null)
                    {
                        benutzer.Profil = _mapper.Map<Profil>(profil);

                        if (benutzer.Profil != null)
                        {
                            var themeModeString = benutzer.Profil?.Einstellungen?.Mode;
                            if (!string.IsNullOrEmpty(themeModeString))
                                benutzer.ThemeMode = Enum.Parse<EnumThemeMode>(themeModeString);
                            else
                            {
                                benutzer.ThemeMode = EnumThemeMode.Light;
                            }
                        }
                        else
                        {
                            benutzer.ThemeMode = EnumThemeMode.Light;
                        }
                    }
                    else
                    {
                        var profilEinstellungen = BaseExtension.GenereteProfilSettings();
                        var profilEinstellungenString = JsonSerializer.Serialize(profilEinstellungen);

                        var newProfile = new Profil()
                        {
                            Id = Guid.NewGuid(),
                            Mitarbeiterid = mitarbeiter.Id,
                            Wert = profilEinstellungenString
                        };

                        var newTprofil = _mapper.Map<TProfil>(newProfile);
                        context.TProfil.Add(newTprofil);
                        await context.SaveChangesAsync();
                    }

                    CurrentUser = benutzer;
                    return benutzer;
                }
            }

            benutzer.Groups = groups;

            if (!groups.Any() && !benutzer.IsMitarbeiter)
            {
                benutzer.IsAuthorized = false;
            }
            else
            {
                benutzer.IsAuthorized = true;
                benutzer.IsAuthenticated = true;
            }

            CurrentUser = benutzer;
            return benutzer;
        }
    }
}
