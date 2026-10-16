using ClinicApi.DTOs;

namespace ClinicApi.Services
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterRequestDto request);
        Task<string> LoginAsync(LoginRequestDto request);
    }
}