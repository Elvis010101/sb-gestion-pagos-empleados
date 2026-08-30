using Microsoft.AspNetCore.Mvc;
using SB.GestionPagos.Aplicacion.Empleados;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;

namespace SB.GestionPagos.Api.Controladores;

/// <summary>
/// Alta y edición de Empleados Asalariados por Comisión: salario base más comisión, con la
/// bonificación que fija el Dominio.
/// </summary>
[Route("api/empleados/asalariados-por-comision")]
public sealed class EmpleadosAsalariadosPorComisionControlador
    : ControladorEmpleadosPorTipo<CrearEmpleadoAsalariadoPorComisionDto, ActualizarEmpleadoAsalariadoPorComisionDto>
{
    public EmpleadosAsalariadosPorComisionControlador(
        IEmpleadoServicio<CrearEmpleadoAsalariadoPorComisionDto, ActualizarEmpleadoAsalariadoPorComisionDto>
            empleadoServicio)
        : base(empleadoServicio)
    {
    }
}
