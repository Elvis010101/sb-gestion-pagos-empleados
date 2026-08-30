using Microsoft.Extensions.Logging;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Repositorios;

namespace SB.GestionPagos.Servicios.Empleados;

/// <summary>
/// Alta y edición de empleados asalariados.
/// </summary>
/// <remarks>
/// Todo lo que esta clase agrega es qué constructor invocar y qué datos de contrato aplicar.
/// El flujo —comprobar duplicados, guardar, registrar en el log, proyectar al DTO— lo aporta
/// la clase base. Esto es lo que cuesta agregar un tipo nuevo de empleado.
/// </remarks>
public sealed class EmpleadoAsalariadoServicio
    : EmpleadoServicioBase<EmpleadoAsalariado, CrearEmpleadoAsalariadoDto, ActualizarEmpleadoAsalariadoDto>
{
    public EmpleadoAsalariadoServicio(
        IEmpleadoRepositorio empleadoRepositorio,
        ILogger<EmpleadoAsalariadoServicio> registrador)
        : base(empleadoRepositorio, registrador)
    {
    }

    protected override EmpleadoAsalariado ConstruirEmpleado(CrearEmpleadoAsalariadoDto solicitud)
        => new(
            solicitud.PrimerNombre,
            solicitud.ApellidoPaterno,
            solicitud.NumeroSeguroSocial,
            solicitud.Departamento,
            solicitud.SalarioSemanal);

    protected override void AplicarDatosDeContrato(
        EmpleadoAsalariado empleado,
        ActualizarEmpleadoAsalariadoDto solicitud)
        => empleado.ActualizarDatosDeContrato(solicitud.SalarioSemanal);
}
