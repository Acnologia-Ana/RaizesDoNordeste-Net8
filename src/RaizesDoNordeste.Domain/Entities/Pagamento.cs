using RaizesDoNordeste.Domain.Enums;

namespace RaizesDoNordeste.Domain.Entities;

public class Pagamento
{
    public int Id { get; set; }

    public int PedidoId { get; set; }

    public decimal Valor { get; set; }

    public StatusPagamento Status { get; set; }
        = StatusPagamento.Pendente;

    public string? CodigoTransacao { get; set; }

    public string? MensagemRetorno { get; set; }

    public DateTime CriadoEm { get; set; }
        = DateTime.UtcNow;

    public Pedido Pedido { get; set; } = null!;
}
