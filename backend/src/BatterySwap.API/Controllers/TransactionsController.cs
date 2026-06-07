using BatterySwap.Application.Services.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace BatterySwap.API.Controllers;

public class TransactionsController : BaseApiController
{
    private readonly ITransactionService _service;
    public TransactionsController(ITransactionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TransactionQueryParameters query, CancellationToken ct)
        => Success(await _service.GetPagedAsync(query, ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
        => Success(await _service.GetByIdAsync(id, ct));
}
