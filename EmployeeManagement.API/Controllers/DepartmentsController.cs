using EmployeeManagement.Application.DTOs.Departments;
using EmployeeManagement.Application.DTOs.Employees;
using EmployeeManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagement.API.Authorization;


namespace EmployeeManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly IEmployeeService _employeeService;

    public DepartmentsController(
        IDepartmentService departmentService,
        IEmployeeService employeeService)
    {
        _departmentService = departmentService;
        _employeeService = employeeService;
    }

    // GET: api/departments
    [HttpGet]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.HR)]
    public async Task<ActionResult<IReadOnlyList<DepartmentDto>>> GetAll()
    {
        var departments = await _departmentService.GetAllAsync();

        return Ok(departments);
    }

    // GET: api/departments/5
    [HttpGet("{id}")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.HR)]
    public async Task<ActionResult<DepartmentDto>> GetById(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);

        if (department == null)
            return NotFound();

        return Ok(department);
    }

    [HttpGet("{id:int}/employees")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.HR)]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetEmployees(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        return Ok(await _employeeService.GetByDepartmentAsync(id));
    }

    // POST: api/departments
    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<DepartmentDto>> Create(
        CreateDepartmentDto departmentDto)
    {
        try
        {
            var department = await _departmentService.CreateAsync(
                departmentDto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = department.Id },
                department);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: ex.Message);
        }
    }

    // PUT: api/departments/5
    [HttpPut("{id}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<DepartmentDto>> Update(
        int id,
        UpdateDepartmentDto departmentDto)
    {
        try
        {
            var department = await _departmentService.UpdateAsync(
                id,
                departmentDto);

            if (department == null)
                return NotFound();

            return Ok(department);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: ex.Message);
        }
    }

    // DELETE: api/departments/5
    [HttpDelete("{id}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _departmentService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: ex.Message);
        }
    }
}