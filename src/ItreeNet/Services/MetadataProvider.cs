using ItreeNet.Data.Models;

namespace ItreeNet.Services
{
    public class MetadataProvider
    {
        public Dictionary<string, MetadataValue> RouteDetailMapping { get; set; } = new()
        {
            {
                "/",
                new()
                {
                    Title = "Itree Informatik GmbH",
                    Description = "Visit more at https://itree.ch"
                }
            },
            {
                "/contact",
                new()
                {
                    Title = "Contact",
                    Description = "Email us: info@itree.ch"
                }
            },
            {
                "/services",
                new()
                {
                    Title = "Services",
                    Description = ".net consulting projektmanagement mssql oracle datenbanken database"
                }
            },
            {
                "/avg2",
                new ()
                {
                    Title = "AVG2",
                    Description = "pflegeinitiative gesundheitsberufe ausbildungspotential"
                }
            }
        };
    }
}
