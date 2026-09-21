namespace RaizesDoNordeste.Application.DTOs;

public record EstoqueDto(
    int Id,
    int UnidadeId,
    string Unidade,
    int ProdutoId,
    string Produto,
    int Quantidade
);
