using Microsoft.AspNetCore.Mvc;
using SB.GestionPagos.Aplicacion.Empleados;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;

namespace SB.GestionPagos.Api.Controladores;

/// <summary>
/// Alta y edición de Empleados por Comisión: cobran un porcentaje de sus ventas brutas.
/// </summary>
[Route("api/empleados/por-comision")]
public sealed class EmpleadosPorComisionControlador
    : ControladorEmpleadosPorTipo<CrearEmpleadoPorComisionDto, ActualizarEmpleadoPorComisionDto>
{
    public EmpleadosPorComisionControlador(
        IEmpleadoServicio<CrearEmpleadoPorComisionDto, ActualizarEmpleadoPorComisionDto> empleadoServicio)
        : base(empleadoServicio)
    {
    }
}
