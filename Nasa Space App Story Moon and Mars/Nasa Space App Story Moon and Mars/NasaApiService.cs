using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Nasa_Space_App_Story_Moon_and_Mars
{
    
    public class NasaApiService
    {

        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public async Task<List<BookPage>> FetchEquipmentPagesAsync(string searchTerm)
        {
            int year_start = 1960;
            int year_end = DateTime.UtcNow.Year;
            if (searchTerm == "APOLLO 11")
            {
                year_start = 1968;
                year_end = 1970;
            } else if (searchTerm == "MARS ROVER")
            {
                year_start = 2000;
                year_end = 2024;
            } else if (searchTerm == "MOON")
            {
                year_start = 1960;
                year_end = 2024;
            }
            var pages = new List<BookPage>();
            string url = $"https://images-api.nasa.gov/search?q={Uri.EscapeDataString(searchTerm)}&media_type=image&year_start={year_start}&year_end={year_end}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return GetFallbackPage("API Error", "Could not connect to the NASA servers.");

                string jsonResponse = await response.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(jsonResponse);

                JArray items = (JArray)data["collection"]?["items"];
                if (items == null || items.Count == 0)
                {
                    return GetFallbackPage("No Results", $"No equipment found for '{searchTerm}'.");
                }

                // Grab up to 6 fascinating records from the query
                int count = 0;
                foreach (var item in items)
                {
                    if (count >= 6) break;

                    var dataObj = item["data"]?[0];
                    var linksArr = item["links"];

                    string title = dataObj?["title"]?.ToString() ?? "NASA Equipment";
                    string description = dataObj?["description"]?.ToString();
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        description = "NASA has not provided a summary for this image.";
                    }
                    string imageUrl = linksArr?[0]?["href"]?.ToString();

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        pages.Add(new BookPage
                        {
                            Title = title,
                            Story = description,
                            ImageUrl = imageUrl
                        });
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                return GetFallbackPage("Connection Exception", $"An error occurred while fetching data: {ex.Message}");
            }

            return pages.Count > 0 ? pages : GetFallbackPage("No Images", "Found records, but no valid image links were returned.");
        }

        private List<BookPage> GetFallbackPage(string title, string message)
        {
            return new List<BookPage>
            {
                new BookPage
                {
                    Title = title,
                    Story = message,
                    ImageUrl = ""
                }
            };
        }
    }
}
