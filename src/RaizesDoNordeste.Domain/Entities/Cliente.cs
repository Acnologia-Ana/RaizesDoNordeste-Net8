namespace RaizesDoNordeste.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string? Cpf { get; set; }

    public string? Telefone { get; set; }

    public bool ConsentimentoFidelidade { get; set; }

    public DateTime? DataConsentimento { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public Fidelidade? Fidelidade { get; set; }

    public ICollection<Pedido> Pedidos { get; set; }
        = new List<Pedido>();
}
