namespace RaizesDoNordeste.Domain.Entities;

public class Produto
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public decimal Preco { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<Estoque> Estoques { get; set; }
        = new List<Estoque>();

    public ICollection<ItemPedido> ItensPedido { get; set; }
        = new List<ItemPedido>();
}
