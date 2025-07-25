﻿using System.ComponentModel.DataAnnotations;

namespace RecommenderApp_API.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? RefreshToken { get; set; } = null;
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public ICollection<UserMovie> UserMovies { get; set; } = new List<UserMovie>();
    }
}
