using RaizesDoNordeste.Application.DTOs;

namespace RaizesDoNordeste.Application.Interfaces;

public interface ITokenService
{
    TokenResponseDto GerarToken(
        UsuarioAutenticadoDto usuario);
}
