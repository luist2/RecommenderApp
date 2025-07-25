﻿using RecommenderApp_API.Entities;
using RecommenderApp_API.Models;

namespace RecommenderApp_API.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDTO request);
        Task<TokenResponseDTO?> LoginAsync(UserDTO request);
        Task<TokenResponseDTO> RefreshTokensAsync(RefreshTokenRequestDTO request);
    }
}
