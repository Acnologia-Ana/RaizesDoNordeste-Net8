using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Application.DTOs;
using RaizesDoNordeste.Application.Interfaces;

namespace RaizesDoNordeste.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public AuthController(
        IAuthService authService,
        ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Senha))
        {
            return BadRequest(new
            {
                erro = "Email e senha sao obrigatorios."
            });
        }

        var usuario =
            await _authService
                .ValidarCredenciaisAsync(request);

        if (usuario is null)
        {
            return Unauthorized(new
            {
                erro = "Email ou senha invalidos."
            });
        }

        var token =
            _tokenService.GerarToken(usuario);

        return Ok(token);
    }

    [AllowAnonymous]
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar(
        CadastroClienteRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Senha))
        {
            return BadRequest(new
            {
                erro = "Nome, email e senha sao obrigatorios."
            });
        }

        if (request.Senha.Length < 8)
        {
            return BadRequest(new
            {
                erro = "A senha deve possuir pelo menos 8 caracteres."
            });
        }

        var usuario =
            await _authService
                .CadastrarClienteAsync(request);

        if (usuario is null)
        {
            return Conflict(new
            {
                erro = "Email ou CPF ja cadastrado."
            });
        }

        var token =
            _tokenService.GerarToken(usuario);

        return StatusCode(
            StatusCodes.Status201Created,
            token);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            id = User.FindFirstValue(
                ClaimTypes.NameIdentifier),

            nome = User.FindFirstValue(
                ClaimTypes.Name),

            email = User.FindFirstValue(
                ClaimTypes.Email),

            role = User.FindFirstValue(
                ClaimTypes.Role)
        });
    }
}
