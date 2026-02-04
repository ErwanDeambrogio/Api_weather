using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace api1_deambrogio.Services
{
    public class FavoritesService
    {
        private const string FavoritesFileName = "favorites.txt";
        private List<string> _favoritesCities;

        public FavoritesService()
        {
            _favoritesCities = new List<string>();
            LoadFavorites();
        }

        public List<string> GetFavorites()
        {
            return new List<string>(_favoritesCities);
        }

        public void AddFavorite(string city)
        {
            if (!_favoritesCities.Contains(city))
            {
                _favoritesCities.Add(city);
                SaveFavorites();
            }
        }

        public void RemoveFavorite(string city)
        {
            if (_favoritesCities.Contains(city))
            {
                _favoritesCities.Remove(city);
                SaveFavorites();
            }
        }

        public bool IsFavorite(string city)
        {
            return _favoritesCities.Contains(city);
        }

        private void LoadFavorites()
        {
            try
            {
                if (File.Exists(FavoritesFileName))
                {
                    _favoritesCities = File.ReadAllLines(FavoritesFileName).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des favoris : {ex.Message}");
            }
        }

        private void SaveFavorites()
        {
            try
            {
                File.WriteAllLines(FavoritesFileName, _favoritesCities);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la sauvegarde des favoris : {ex.Message}");
            }
        }
    }
}