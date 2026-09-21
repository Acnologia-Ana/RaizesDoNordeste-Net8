namespace RaizesDoNordeste.Application.DTOs;

public record CardapioItemDto(
    int ProdutoId,
    string Nome,
    string? Descricao,
    decimal Preco,
    int QuantidadeDisponivel
);
