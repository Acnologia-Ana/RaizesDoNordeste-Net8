using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Application.Interfaces;

namespace RaizesDoNordeste.Api.Controllers;

[ApiController]
[Route("api/unidades")]
public class UnidadesController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public UnidadesController(
        ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var unidades =
            await _catalogoService.ListarUnidadesAsync();

        return Ok(unidades);
    }

    [HttpGet("{unidadeId:int}/cardapio")]
    public async Task<IActionResult> Cardapio(
        int unidadeId)
    {
        var cardapio =
            await _catalogoService
                .ObterCardapioAsync(unidadeId);

        if (cardapio is null)
        {
            return NotFound(new
            {
                erro = "Unidade nao encontrada."
            });
        }

        return Ok(cardapio);
    }
}
