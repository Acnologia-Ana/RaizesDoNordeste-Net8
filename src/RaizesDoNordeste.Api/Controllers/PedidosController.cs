using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Application.DTOs;
using RaizesDoNordeste.Application.Interfaces;

namespace RaizesDoNordeste.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly IPedidoService _service;

    public PedidosController(IPedidoService service)
    {
        _service = service;
    }

    private int UsuarioId =>
        int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var id) ? id : 0;

    private string Role =>
        User.FindFirstValue(ClaimTypes.Role) ?? "";

    private IActionResult Responder<T>(OperacaoResultado<T> resultado)
    {
        if (resultado.Erro is not null)
            return StatusCode(
                resultado.Codigo,
                new { erro = resultado.Erro });

        return StatusCode(resultado.Codigo, resultado.Dados);
    }

    [HttpPost]
    [Authorize(Roles = "Cliente,Atendente,Gerente,Admin")]
    public async Task<IActionResult> Criar(CriarPedidoDto request) =>
        Responder(await _service.CriarAsync(
            request, UsuarioId, Role));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id) =>
        Responder(await _service.ObterAsync(
            id, UsuarioId, Role));

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? canal,
        [FromQuery] string? status) =>
        Responder(await _service.ListarAsync(
            canal, status, UsuarioId, Role));

    [HttpPost("{id:int}/pagamento/mock")]
    [Authorize(Roles = "Cliente,Atendente,Gerente,Admin")]
    public async Task<IActionResult> Pagamento(
        int id, PagamentoMockDto request) =>
        Responder(await _service.PagarAsync(
            id, request, UsuarioId, Role));

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Cozinha,Atendente,Gerente,Admin")]
    public async Task<IActionResult> Status(
        int id, AlterarStatusDto request) =>
        Responder(await _service.AlterarStatusAsync(
            id, request, UsuarioId));
}
