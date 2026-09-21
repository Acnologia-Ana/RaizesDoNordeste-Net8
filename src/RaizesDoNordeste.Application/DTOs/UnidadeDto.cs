namespace RaizesDoNordeste.Application.DTOs;

public record UnidadeDto(
    int Id,
    string Nome,
    string Cidade,
    string Estado,
    bool Ativa
);
