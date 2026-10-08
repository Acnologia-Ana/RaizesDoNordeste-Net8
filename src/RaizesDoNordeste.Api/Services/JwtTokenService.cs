using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RaizesDoNordeste.Application.DTOs;
using RaizesDoNordeste.Application.Interfaces;

namespace RaizesDoNordeste.Api.Services;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TokenResponseDto GerarToken(
        UsuarioAutenticadoDto usuario)
    {
        var key =
            _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "Jwt:Key nao configurada.");

        var issuer =
            _configuration["Jwt:Issuer"]
            ?? "RaizesDoNordeste.Api";

        var audience =
            _configuration["Jwt:Audience"]
            ?? "RaizesDoNordeste.Client";

        var expiraEm =
            DateTime.UtcNow.AddHours(2);

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                usuario.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                usuario.Nome),

            new Claim(
                ClaimTypes.Email,
                usuario.Email),

            new Claim(
                ClaimTypes.Role,
                usuario.Role)
        };

        var securityKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

        var credentials =
            new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiraEm,
                signingCredentials: credentials);

        var accessToken =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return new TokenResponseDto(
            accessToken,
            expiraEm,
            usuario);
    }
}
