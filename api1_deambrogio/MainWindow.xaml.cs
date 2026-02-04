using api1_deambrogio.Services;
using api1_deambrogio.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace api1_deambrogio
{
    /// <summary>
    /// MainWindow.xaml.cs - Fenêtre principale de l'application météo
    /// 
    /// RÔLE:
    /// Cette classe gère la logique de l'interface principale incluant:
    /// - La navigation vers la page météo (MeteoPage)
    /// - Les appels API pour récupérer les données météorologiques
    /// - La gestion des villes favorites
    /// - L'affichage de la date actuelle
    /// </summary>
    public partial class MainWindow : Window
    {
        // Service pour récupérer les données météorologiques depuis l'API
        private readonly WeatherService _weatherService;
        
        // Service pour gérer la liste des villes favorites (sauvegarde/chargement)
        private readonly FavoritesService _favoritesService;
        
        // La ville actuellement sélectionnée (utilisée pour les recherches)
        private string _currentCity = "Paris";
        
        // Référence à la page météo pour y transmettre les données
        private MeteoPage _meteoPage;

        /// <summary>
        /// Constructeur - Initialise la fenêtre principale
        /// ÉTAPES:
        /// 1. Appelle InitializeComponent() pour créer l'interface XAML
        /// 2. Initialise les services (WeatherService et FavoritesService)
        /// 3. Crée et charge la MeteoPage dans le Frame
        /// 4. Charge les favoris depuis le fichier de sauvegarde
        /// 5. Charge la météo initiale pour "Paris"
        /// 6. Affiche la date actuelle
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Initialiser les services
            _weatherService = new WeatherService();
            _favoritesService = new FavoritesService();

            // Créer la page météo et la naviguer dans le Frame
            _meteoPage = new MeteoPage();
            MainFrame.Navigate(_meteoPage);

            // Charger les favoris et la météo initiale
            LoadFavorites();
            _ = LoadWeatherForCity(_currentCity);

            // Afficher la date actuelle au format français
            DateActuelle.Text = DateTime.Now.ToString("dddd d MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"));
        }

        /// <summary>
        /// Charge la liste des villes favorites depuis la sauvegarde
        /// - Récupère les favoris via FavoritesService
        /// - Met à jour le ComboBox pour afficher la liste
        /// </summary>
        private void LoadFavorites()
        {
            try
            {
                var favorites = _favoritesService.GetFavorites();
                ComboBoxFavorites.ItemsSource = null; // Réinitialiser la source
                ComboBoxFavorites.ItemsSource = favorites; // Assigner les nouvelles données
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des favoris : {ex.Message}");
            }
        }

        /// <summary>
        /// Charge la météo pour une ville spécifique
        /// PROCESSUS:
        /// 1. Appelle l'API WeatherService pour récupérer les données
        /// 2. Si succès: Transmet les données à MeteoPage via LoadWeather()
        /// 3. Met à jour l'affichage du nom de la ville et de l'heure
        /// 4. Si erreur: Affiche un message d'erreur
        /// </summary>
        private async System.Threading.Tasks.Task LoadWeatherForCity(string city)
        {
            try
            {
                // Appel asynchrone à l'API pour récupérer les données météorologiques
                var weatherData = await _weatherService.GetWeatherAsync(city);

                if (weatherData != null)
                {
                    // Mettre à jour l'interface avec les infos de la ville
                    TxtCurrentCity.Text = $"📍 {weatherData.city_info?.name ?? city}";
                    DerniereMiseAJour.Text = weatherData.current_condition?.hour ?? "N/A";

                    // Charger les données dans la page météo (qui s'affichera dans le Frame)
                    await _meteoPage.LoadWeather(weatherData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}");
            }
        }

        /// <summary>
        /// Événement clic du bouton "Rechercher"
        /// - Récupère le texte de la TextBox TxtCity
        /// - Charge la météo pour cette ville
        /// </summary>
        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string city = TxtCity.Text.Trim();
            if (!string.IsNullOrEmpty(city))
            {
                _currentCity = city;
                await LoadWeatherForCity(city);
            }
            else
            {
                MessageBox.Show("Veuillez entrer un nom de ville.");
            }
        }

        /// <summary>
        /// Événement clic du bouton "Ajouter aux favoris"
        /// - Ajoute la ville actuelle à la liste des favoris
        /// - Sauvegarde la liste
        /// - Met à jour le ComboBox
        /// </summary>
        private void BtnAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            string city = TxtCity.Text.Trim();
            if (!string.IsNullOrEmpty(city))
            {
                if (!_favoritesService.IsFavorite(city))
                {
                    _favoritesService.AddFavorite(city);
                    LoadFavorites();
                    MessageBox.Show($"{city} ajouté aux favoris !");
                }
                else
                {
                    MessageBox.Show($"{city} est déjà dans les favoris.");
                }
            }
            else
            {
                MessageBox.Show("Veuillez entrer un nom de ville.");
            }
        }

        /// <summary>
        /// Événement clic du bouton "Retirer des favoris"
        /// - Supprime la ville sélectionnée du ComboBox des favoris
        /// - Sauvegarde la liste mise à jour
        /// - Rafraîchit le ComboBox
        /// </summary>
        private void BtnRemoveFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxFavorites.SelectedItem != null)
            {
                string selectedCity = ComboBoxFavorites.SelectedItem.ToString();
                _favoritesService.RemoveFavorite(selectedCity);
                LoadFavorites();
                MessageBox.Show($"{selectedCity} retiré des favoris.");
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une ville favorite à retirer.");
            }
        }

        /// <summary>
        /// Événement de changement du ComboBox des favoris
        /// - Quand l'utilisateur sélectionne une ville dans la liste déroulante
        /// - Remplit la TextBox avec le nom de la ville
        /// - Charge la météo pour cette ville
        /// </summary>
        private async void ComboBoxFavorites_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxFavorites.SelectedItem != null)
            {
                string selectedCity = ComboBoxFavorites.SelectedItem.ToString();
                TxtCity.Text = selectedCity; // Afficher dans la TextBox
                _currentCity = selectedCity;
                await LoadWeatherForCity(selectedCity); // Charger la météo
            }
        }

        /// <summary>
        /// Événement clavier sur la TextBox
        /// - Détecte quand l'utilisateur appuie sur la touche Entrée (Key.Enter)
        /// - Lance la recherche météo pour la ville saisie
        /// - Pratique pour chercher rapidement sans cliquer le bouton
        /// </summary>
        private async void TxtCity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) // Si la touche Entrée est pressée
            {
                string city = TxtCity.Text.Trim();
                if (!string.IsNullOrEmpty(city))
                {
                    _currentCity = city;
                    await LoadWeatherForCity(city);
                }
            }
        }

        private void TxtCity_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}