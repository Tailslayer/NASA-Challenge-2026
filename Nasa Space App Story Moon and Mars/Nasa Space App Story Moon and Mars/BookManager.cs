using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Nasa_Space_App_Story_Moon_and_Mars
{
    public class BookManager
    {
        private List<BookPage> _pages = new List<BookPage>();
        private int _currentIndex = 0;
        private readonly NasaApiService _nasaApi = new NasaApiService();

        public int CurrentIndex => _currentIndex;
        public int TotalPages => _pages?.Count ?? 0;
        public bool HasNext => _currentIndex < TotalPages - 1;
        public bool HasPrevious => _currentIndex > 0;

        public async Task InitializeBookAsync()
        {
            // These are locally curated summaries of the linked NASA sources.
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MissionStories.json");
            _pages = JsonConvert.DeserializeObject<List<BookPage>>(File.ReadAllText(path));
            if (_pages == null || _pages.Count == 0)
            {
                throw new InvalidDataException("The mission story catalog is empty.");
            }
            _currentIndex = 0;

            await Task.WhenAll(_pages.Select(async page =>
            {
                var images = await _nasaApi.FetchEquipmentPagesAsync(page.SearchTerm);
                var image = images.FirstOrDefault(item => !string.IsNullOrEmpty(item.ImageUrl));
                // Keep the story available even if NASA's image service is unavailable.
                page.ImageUrl = image?.ImageUrl;
                page.ImageTitle = image?.Title;
            }));
        }

        public BookPage GetCurrentPage()
        {
            if (_pages == null || _pages.Count == 0) return null;
            return _pages[_currentIndex];
        }

        public BookPage NextPage()
        {
            if (HasNext) _currentIndex++;
            return GetCurrentPage();
        }

        public BookPage PreviousPage()
        {
            if (HasPrevious) _currentIndex--;
            return GetCurrentPage();
        }
    }
}
