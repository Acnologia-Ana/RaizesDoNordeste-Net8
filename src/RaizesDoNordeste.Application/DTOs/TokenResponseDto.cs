namespace RaizesDoNordeste.Application.DTOs;

public record TokenResponseDto(
    string AccessToken,
    DateTime ExpiraEm,
    UsuarioAutenticadoDto Usuario
);
