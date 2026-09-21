namespace RaizesDoNordeste.Application.DTOs;

public record ProdutoDto(
    int Id,
    string Nome,
    string? Descricao,
    decimal Preco,
    bool Ativo
);
