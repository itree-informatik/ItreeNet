using AutoMapper;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;

namespace ItreeNet.Data.Extensions
{
    public class Mappings : Profile
    {
        public Mappings()
        {
            CreateMap<TAnwesenheit, Anwesenheit>()
                .ReverseMap()
                .ForMember(a => a.Mitarbeiter, opt => opt.Ignore());

            CreateMap<TArbeitszeit, Arbeitszeit>()
                .ForMember(a => a.Zeit, opt => opt.MapFrom(a => a.Arbeitszeit))
                .ReverseMap()
                .ForMember(a => a.Arbeitszeit, opt => opt.MapFrom(a => a.Zeit));

            CreateMap<TArbeitszeitReduktion, ArbeitszeitReduktion>()
                .ReverseMap();

            CreateMap<TBuchung, Buchung>()
                .ForMember(b => b.KundenName, opt => opt.Ignore())
                .ForMember(b => b.ProjektName, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(b => b.ChangedByNavigation, opt => opt.Ignore())
                .ForMember(b => b.Mitarbeiter, opt => opt.Ignore())
                .ForMember(b => b.Vorgang, opt => opt.Ignore());

            CreateMap<TFerienArbeitspensum, FerienArbeitspensum>().ReverseMap();

            CreateMap<TKunde, Kunde>()
                .ReverseMap()
                .ForMember(k => k.TProjekt, opt => opt.Ignore());

            CreateMap<TMitarbeiter, Mitarbeiter>()
                .ReverseMap();

            CreateMap<TMitarbeiterSaldo, MitarbeiterSaldo>().ReverseMap();

            CreateMap<TMitarbeiterSaldoKorrektur, MitarbeiterSaldoKorrektur>()
                .ReverseMap()
                .ForMember(b => b.CreatedByNavigation, opt => opt.Ignore())
                .ForMember(b => b.Mitarbeiter, opt => opt.Ignore());

            CreateMap<TPipelineRuns, PipelineRuns>().ReverseMap();

            CreateMap<TProfil, Profil>()
                .ReverseMap();

            CreateMap<TProjekt, Projekt>()
                .ReverseMap()
                .ForMember(p => p.Kunde, opt => opt.Ignore())
                .ForMember(p => p.TVorgang, opt => opt.Ignore());

            CreateMap<TSpesen, Spesen>()
                .ReverseMap()
                .ForMember(s => s.Mitarbeiter, opt => opt.Ignore());

            CreateMap<TTeam, Team>()
                .ReverseMap()
                .ForMember(t => t.TKunde, opt => opt.Ignore());
            
            CreateMap<TVorgang, Vorgang>()
                .ReverseMap()
                .ForMember(p => p.Projekt, opt => opt.Ignore())
                .ForMember(p => p.TBuchungVorgang, opt => opt.Ignore())
                .ForMember(p => p.TBuchungOriginalVorgang, opt => opt.Ignore());

            CreateMap<TFrontendtest, Frontendtest>()
                .ForMember(x => x.FrontendtestDetailListe, opt => opt.MapFrom(x => x.TFrontendtestDetail))
                .ReverseMap()
                .ForMember(x => x.TFrontendtestDetail, opt => opt.Ignore());

            CreateMap<TFrontendtest, FrontendtestOverview>()
                .ReverseMap()
                .ForMember(x => x.TFrontendtestDetail, opt => opt.Ignore());

            CreateMap<TFrontendtestDetail, FrontendtestDetail>()
                .ForMember(x => x.FrontendtestBildListe, opt => opt.MapFrom(x => x.TFrontendtestBild))
                .ReverseMap()
                .ForMember(x => x.TFrontendtestBild, opt => opt.Ignore());

            CreateMap<TFrontendtestBild, FrontendtestBild>()
                .ReverseMap();

        }
    }
}
