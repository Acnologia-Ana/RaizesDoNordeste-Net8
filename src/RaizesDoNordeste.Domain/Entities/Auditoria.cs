namespace RaizesDoNordeste.Domain.Entities;

public class Auditoria
{
    public int Id { get; set; }

    public int? UsuarioId { get; set; }

    public string Acao { get; set; } = string.Empty;

    public string Entidade { get; set; } = string.Empty;

    public string? EntidadeId { get; set; }

    public string? Dados { get; set; }

    public DateTime CriadoEm { get; set; }
        = DateTime.UtcNow;

    public Usuario? Usuario { get; set; }
}
