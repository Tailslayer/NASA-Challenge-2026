using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task InitializeBookAsync(string searchTerm)
        {
            _pages = await _nasaApi.FetchEquipmentPagesAsync(searchTerm);
            _currentIndex = 0;
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
