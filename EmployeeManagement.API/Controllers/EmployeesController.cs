
using EmployeeManagement.Application.DTOs.Employees;
using EmployeeManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagement.API.Authorization;

namespace EmployeeManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    // GET: api/employees
    [HttpGet]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.HR + "," + AppRoles.Employee)]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetAll()
    {
        var employees = await _employeeService.GetAllAsync();
        return Ok(employees);
    }

    [HttpGet("search")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.HR + "," + AppRoles.Employee)]
    public async Task<ActionResult<PagedEmployeesDto>> Search(
        [FromQuery] EmployeeSearchRequest request)
    {
        return Ok(await _employeeService.SearchAsync(request));
    }

    [HttpGet("statistics")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.HR)]
    public async Task<ActionResult<EmployeeStatisticsDto>> GetStatistics()
    {
        return Ok(await _employeeService.GetStatisticsAsync());
    }

    // GET: api/employees/1
    [HttpGet("{id}")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.HR + "," + AppRoles.Employee)]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);

        if (employee == null)
            return NotFound();

        return Ok(employee);
    }

    // POST: api/employees
    [HttpPost]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.HR)]
    public async Task<ActionResult<EmployeeDto>> Create(
        CreateEmployeeDto employeeDto)
    {
        try
        {
            var employee = await _employeeService.CreateAsync(employeeDto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = employee.Id },
                employee
            );
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: ex.Message);
        }
    }

    // PUT: api/employees/1
    [HttpPut("{id}")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.HR)]
    public async Task<ActionResult<EmployeeDto>> Update(
        int id,
        UpdateEmployeeDto employeeDto)
    {
        try
        {
            var employee = await _employeeService.UpdateAsync(id, employeeDto);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: ex.Message);
        }
    }

    // DELETE: api/employees/1
    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.HR)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _employeeService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
