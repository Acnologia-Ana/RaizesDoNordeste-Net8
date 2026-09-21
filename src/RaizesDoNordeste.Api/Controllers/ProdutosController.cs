using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Application.Interfaces;

namespace RaizesDoNordeste.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public ProdutosController(
        ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var produtos =
            await _catalogoService.ListarProdutosAsync();

        return Ok(produtos);
    }
}
