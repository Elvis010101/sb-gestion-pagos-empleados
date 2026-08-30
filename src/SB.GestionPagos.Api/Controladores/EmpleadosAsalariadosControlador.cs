using Microsoft.AspNetCore.Mvc;
using SB.GestionPagos.Aplicacion.Empleados;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;

namespace SB.GestionPagos.Api.Controladores;

/// <summary>
/// Alta y edición de Empleados Asalariados: cobran un salario fijo semanal.
/// </summary>
/// <remarks>
/// Esta clase es todo lo que cuesta publicar un tipo de empleado en la API: la ruta y el par
/// de DTOs. Las dos acciones las hereda de
/// <see cref="ControladorEmpleadosPorTipo{TSolicitudCreacion, TSolicitudActualizacion}"/>.
/// </remarks>
[Route("api/empleados/asalariados")]
public sealed class EmpleadosAsalariadosControlador
    : ControladorEmpleadosPorTipo<CrearEmpleadoAsalariadoDto, ActualizarEmpleadoAsalariadoDto>
{
    public EmpleadosAsalariadosControlador(
        IEmpleadoServicio<CrearEmpleadoAsalariadoDto, ActualizarEmpleadoAsalariadoDto> empleadoServicio)
        : base(empleadoServicio)
    {
    }
}
