using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize(Roles = "Admin")]
public sealed class AuditController : ControllerBase
{
    private readonly ApplicationDbContext _database;

    public AuditController(ApplicationDbContext database)
    {
        _database = database;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> GetRecent(
        [FromQuery, Range(1, 200)] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var entries = await _database.AuditLogs
            .AsNoTracking()
            .OrderByDescending(entry => entry.Id)
            .Take(limit)
            .Select(entry => new AuditLogDto(
                entry.Id,
                entry.ActorId,
                entry.ActorEmail,
                entry.Method,
                entry.Path,
                entry.StatusCode,
                entry.TimestampUtc))
            .ToListAsync(cancellationToken);

        return Ok(entries);
    }
}
