using api1_deambrogio.Models;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace api1_deambrogio.Views
{
    public partial class MeteoPage : Page
    {
        public MeteoPage()
        {
            InitializeComponent();
        }

        // Méthode pour télécharger et afficher une image météo
        public async void SetWeatherImage(System.Windows.Controls.Image imageControl, string iconUrl)
        {
            try
            {
                if (imageControl == null || string.IsNullOrEmpty(iconUrl))
                    return;

                string fullUrl = iconUrl;

                // Si l'URL ne commence pas par http, ajouter le domaine de base
                if (!iconUrl.StartsWith("http"))
                {
                    fullUrl = $"https://www.prevision-meteo.ch{iconUrl}";
                }

                System.Diagnostics.Debug.WriteLine($"Chargement de l'icône depuis: {fullUrl}");

                // Télécharger l'image de manière asynchrone
                using (HttpClient client = new HttpClient())
                {
                    var imageBytes = await client.GetByteArrayAsync(fullUrl);

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(imageBytes);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    // Mettre à jour l'image sur le thread UI
                    imageControl.Dispatcher.Invoke(() =>
                    {
                        imageControl.Source = bitmap;
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur chargement image depuis {iconUrl}: {ex.Message}");
            }
        }

        public async Task LoadWeather(Root weatherData)
        {
            if (weatherData == null) return;

            try
            {
                // Today
                var today = weatherData.fcst_day_0;
                if (today != null)
                {
                    Température_actuelle.Text = weatherData.current_condition.tmp.ToString() + "°C";
                    Vent.Text = weatherData.current_condition.wnd_spd.ToString() + " km/h";
                    Température_max.Text = today.tmax.ToString() + " °C";
                    Température_min.Text = today.tmin.ToString() + " °C";
                    Condition.Text = (weatherData.current_condition?.humidity ?? 0).ToString() + " %";
                    Temps.Text = weatherData.current_condition.condition ?? "";
                    Precipitation.Text = SumPrecipitation(today.hourly_data).ToString("F1") + " mm";
                    SetWeatherImage(WeatherImageAujourdhui, weatherData.current_condition.icon_big ?? weatherData.current_condition.icon);
                }

                // jour1 (fcst_day_1)
                var jour1 = weatherData.fcst_day_1;
                if (jour1 != null)
                {
                    Day.Text = jour1.day_long ?? "";
                    Date.Text = jour1.date ?? "";
                    string iconToUse = !string.IsNullOrEmpty(jour1.icon_big) ? jour1.icon_big : jour1.icon;
                    SetWeatherImage(Icon, iconToUse);
                    Temp.Text = jour1.condition ?? "";
                    temp_max.Text = jour1.tmax.ToString() + "°";
                    temp_min.Text = " / " + jour1.tmin.ToString() + "°";
                    precipitation.Text = "💧 " + SumPrecipitation(jour1.hourly_data).ToString("F1") + " mm";
                }

                // jour2 (fcst_day_2)
                var jour2 = weatherData.fcst_day_2;
                if (jour2 != null)
                {
                    DAY.Text = jour2.day_long ?? "";
                    DATE.Text = jour2.date ?? "";
                    string iconToUse = !string.IsNullOrEmpty(jour2.icon_big) ? jour2.icon_big : jour2.icon;
                    SetWeatherImage(ICON, iconToUse);
                    TEMP.Text = jour2.condition ?? "";
                    Temp_max.Text = jour2.tmax.ToString() + "°";
                    Temp_min.Text = " / " + jour2.tmin.ToString() + "°";
                    Précipitation.Text = "💧 " + SumPrecipitation(jour2.hourly_data).ToString("F1") + " mm";
                }

                // jour3 (fcst_day_3)
                var jour3 = weatherData.fcst_day_3;
                if (jour3 != null)
                {
                    jour.Text = jour3.day_long ?? "";
                    dates.Text = jour3.date ?? "";
                    string iconToUse = !string.IsNullOrEmpty(jour3.icon_big) ? jour3.icon_big : jour3.icon;
                    SetWeatherImage(Icons, iconToUse);
                    temperature.Text = jour3.condition ?? "";
                    TEMP_max.Text = jour3.tmax.ToString() + "°";
                    TEMP_min.Text = " / " + jour3.tmin.ToString() + "°";
                    Precip.Text = "💧 " + SumPrecipitation(jour3.hourly_data).ToString("F1") + " mm";
                }

                // jour4 (fcst_day_4)
                var jour4 = weatherData.fcst_day_4;
                if (jour4 != null)
                {
                    Jour.Text = jour4.day_long ?? "";
                    date.Text = jour4.date ?? "";
                    string iconToUse = !string.IsNullOrEmpty(jour4.icon_big) ? jour4.icon_big : jour4.icon;
                    SetWeatherImage(Images, iconToUse);
                    Tempé.Text = jour4.condition ?? "";
                    tempé_max.Text = jour4.tmax.ToString() + "°";
                    tempé_min.Text = " / " + jour4.tmin.ToString() + "°";
                    précipitation.Text = "💧 " + SumPrecipitation(jour4.hourly_data).ToString("F1") + " mm";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des données météo: {ex.Message}");
            }
        }

        // Somme des précipitations horaires EN MILLIMÈTRES
        private double SumPrecipitation(HourlyData hd)
        {
            if (hd == null) return 0;
            double total = 0;
            var props = hd.GetType().GetProperties();

            foreach (var p in props)
            {
                var hourObj = p.GetValue(hd);
                if (hourObj == null) continue;

                var apcpProp = hourObj.GetType().GetProperty("APCPsfc");
                if (apcpProp == null) continue;

                var val = apcpProp.GetValue(hourObj);
                if (val == null) continue;

                double parsed = 0;
                if (val is double dv) parsed = dv;
                else if (val is float fv) parsed = fv;
                else if (val is int iv) parsed = iv;
                else if (val is long lv) parsed = lv;
                else if (val is decimal decv) parsed = (double)decv;
                else
                {
                    var s = val.ToString();
                    double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed);
                }

                total += parsed;
            }
            return total;
        }
    }
}
