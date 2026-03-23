using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ItreeNet.Services
{
    public class MetadataTransferService : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string? _title;
        
        private string? _description;
        private readonly NavigationManager _navigationManager;
        private readonly MetadataProvider _metadataProvider;

        public MetadataTransferService(NavigationManager navigationManager, MetadataProvider metadataProvider)
        {
            _navigationManager = navigationManager;
            _metadataProvider = metadataProvider;
            _navigationManager.LocationChanged += UpdateMetadata!;
            UpdateMetadata(_navigationManager.Uri);
        }

        public string? Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public string? Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }

        public void Dispose()
        {
            _navigationManager.LocationChanged -= UpdateMetadata!;
        }

        private void UpdateMetadata(object sender, LocationChangedEventArgs e)
        {
            UpdateMetadata(e.Location);
        }


        private void UpdateMetadata(string url)
        {
            var metadataValue = _metadataProvider.RouteDetailMapping.FirstOrDefault(vp => url.EndsWith(vp.Key)).Value;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (metadataValue == null)
            {
                metadataValue = new()
                {
                    Title = "itree informatik GmbH",
                    Description = ".net consulting projektmanagement mssql oracle datenbanken database"
                };
            }

            Title = metadataValue.Title;
            Description = metadataValue.Description;
        }


    }
}
