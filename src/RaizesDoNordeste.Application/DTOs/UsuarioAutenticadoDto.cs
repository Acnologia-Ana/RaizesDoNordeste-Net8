namespace RaizesDoNordeste.Application.DTOs;

public record UsuarioAutenticadoDto(
    int Id,
    string Nome,
    string Email,
    string Role
);
