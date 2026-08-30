using Microsoft.AspNetCore.Mvc;
using SB.GestionPagos.Aplicacion.Empleados;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;

namespace SB.GestionPagos.Api.Controladores;

/// <summary>
/// Alta y edición de Empleados por Horas: cobran por hora, con recargo a partir de la
/// cuadragésima.
/// </summary>
[Route("api/empleados/por-horas")]
public sealed class EmpleadosPorHorasControlador
    : ControladorEmpleadosPorTipo<CrearEmpleadoPorHorasDto, ActualizarEmpleadoPorHorasDto>
{
    public EmpleadosPorHorasControlador(
        IEmpleadoServicio<CrearEmpleadoPorHorasDto, ActualizarEmpleadoPorHorasDto> empleadoServicio)
        : base(empleadoServicio)
    {
    }
}
