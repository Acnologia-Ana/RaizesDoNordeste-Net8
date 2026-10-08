using RaizesDoNordeste.Application.DTOs;

namespace RaizesDoNordeste.Application.Interfaces;

public interface IAuthService
{
    Task<UsuarioAutenticadoDto?> ValidarCredenciaisAsync(
        LoginRequestDto request);

    Task<UsuarioAutenticadoDto?> CadastrarClienteAsync(
        CadastroClienteRequestDto request);
}
