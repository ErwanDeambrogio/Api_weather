using api1_deambrogio.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace api1_deambrogio.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<Root> GetWeatherAsync(string city)
        {
            try
            {
                string url = $"https://www.prevision-meteo.ch/services/json/{city}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var root = JsonConvert.DeserializeObject<Root>(content);
                    return root;
                }
                else
                {
                    throw new Exception($"Ville '{city}' non trouvée.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur WeatherService: {ex.Message}");
                throw;
            }
        }
    }
}