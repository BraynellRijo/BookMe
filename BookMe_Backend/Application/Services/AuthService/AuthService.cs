using Application.DTOs.AuthDTOs;
using Application.DTOs.TokensDTOs;
using Application.Interfaces.Repositories.Users;
using Application.Interfaces.Services.AuthServices;
using Application.Interfaces.Services.EmailServices;
using AutoMapper;
using Domain.Entities.Users;
using Domain.Enums.Users;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services.AuthService 
{
    public class AuthService(IQueryUserRepository queryUseRepository,
        ICommandUserRepository commandUserRepository,
        ITokenService tokenService,
        IMapper mapper,
        IPasswordHasher<User> passwordHasher,
        IMemoryCache cache,
        IEmailServices emailServices,
        IValidator<RegisterDTO> registerValidator,
        IValidator<LoginDTO> loginValidator) : IAuthService
    {
        private readonly IQueryUserRepository _queryUserRepository = queryUseRepository;
        private readonly ICommandUserRepository _commandUserRepository = commandUserRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IMapper _mapper = mapper;
        private readonly IMemoryCache _cache = cache;
        private readonly IEmailServices _emailServices = emailServices;
        private readonly IPasswordHasher<User> _passwordHasher = passwordHasher; 
        private readonly IValidator<RegisterDTO> _registerValidator = registerValidator;
        private readonly IValidator<LoginDTO> _loginValidator = loginValidator;

        public async Task<User?> RegisterAsync(RegisterDTO request)
        {
            var validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var user = _mapper.Map<User>(request); 
            user.HashedPassword = _passwordHasher.HashPassword(user, request.Password);

            await _commandUserRepository.CreateAsync(user);
            await SendVerificationEmailAsync(user.Email);

            return user;
        }


        public async Task<TokenResponseDTO?> LoginAsync(LoginDTO request)
        {
            var validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var user = await _queryUserRepository.GetUserByEmail(request.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password.");
            
            var passwordResult = (_passwordHasher.VerifyHashedPassword(
                    user,
                    user.HashedPassword,
                    request.Password));

            if (passwordResult == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Invalid email or password.");

            if(!user.IsVerified)
                throw new UnauthorizedAccessException("Email not verified. Please check your email to verify your account.");

            return await _tokenService.CreateTokenResponse(user);
        }

        public async Task<TokenResponseDTO?> RefreshTokenAsync(RequestRefreshTokenDTO request)
        {
            var user = await _tokenService.ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);

            return user is not null ? await _tokenService.CreateTokenResponse(user) : null;
        }
        public async Task LogoutAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID for logout.", nameof(userId));

            await _tokenService.RevokeSessionAsync(userId);

            // Here you could log the logout event
            // _logger.LogInformation($"User {userId} logged out successfully at {DateTime.UtcNow}");
        }
        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDTO request)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID.", nameof(userId));

            if (string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
                throw new ArgumentException("Passwords cannot be empty.");

            if (request.OldPassword == request.NewPassword)
                throw new ArgumentException("The new password must be different from the old one.");

            var user = await _queryUserRepository.GetByIdAsync(userId);
            if (user is null)
                throw new InvalidOperationException("User not found.");

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.HashedPassword, request.OldPassword);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("The old password is incorrect.");
            await _commandUserRepository.UpdatePasswordAsync(userId, _passwordHasher.HashPassword(user, request.NewPassword));
        }

        public async Task SendVerificationEmailAsync(string email)
        {
            var user = await _queryUserRepository.GetUserByEmail(email);
            if (user is null)
                throw new InvalidOperationException("User not found.");

            if(!user.IsVerified)
            {
                string code = Random.Shared.Next(100000, 999999).ToString();
                int expirationTime = 15;

                _cache.Set($"EmailVerify_{user.Id}", code, TimeSpan.FromMinutes(expirationTime));
                await _emailServices.SendVerificationEmailAsync(user.Email, code, expirationTime);
            }
        }

        public async Task<bool> VerifyCode(VerifyEmailDTO emailDTO)
        {
            var user = await _queryUserRepository.GetUserByEmail(emailDTO.Email)
                ?? throw new InvalidOperationException("User not found.");

            if (user.IsVerified)
                return true;


            if (_cache.TryGetValue($"EmailVerify_{user.Id}", out string? storedCode))
            {
                if (storedCode == emailDTO.Code)
                {
                    _cache.Remove($"EmailVerify_{user.Id}");
                    user.IsVerified = true;
                    await _commandUserRepository.UpdateEmailVerification(user.Id, true);
                    return true;
                }
            }

            throw new UnauthorizedAccessException("The verification code is invalid or has expired.");
        }

        public async Task<TokenResponseDTO> BecomeHostAsync(Guid userId)
        {
            var user = await _queryUserRepository.GetByIdAsync(userId)
                    ?? throw new KeyNotFoundException("User not found.");

            if (user.Roles.Contains(RolesType.Host))
                throw new InvalidOperationException("You are already registered as a Host.");

            if (!user.IsVerified)
                throw new InvalidOperationException("You must verify your email address before becoming a Host.");

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                throw new InvalidOperationException("A phone number is required to become a Host. Please update your profile.");

            await _commandUserRepository.AddRoleAsync(userId, RolesType.Host);
            var updatedUser = await _queryUserRepository.GetByIdAsync(userId);

            return await _tokenService.CreateTokenResponse(updatedUser);
        }

    }
}