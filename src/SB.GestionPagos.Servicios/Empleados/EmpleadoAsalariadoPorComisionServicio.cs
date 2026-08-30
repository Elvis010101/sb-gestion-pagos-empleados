using Microsoft.Extensions.Logging;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Repositorios;

namespace SB.GestionPagos.Servicios.Empleados;

/// <summary>Alta y edición de empleados con salario base más comisión.</summary>
public sealed class EmpleadoAsalariadoPorComisionServicio
    : EmpleadoServicioBase<
        EmpleadoAsalariadoPorComision,
        CrearEmpleadoAsalariadoPorComisionDto,
        ActualizarEmpleadoAsalariadoPorComisionDto>
{
    public EmpleadoAsalariadoPorComisionServicio(
        IEmpleadoRepositorio empleadoRepositorio,
        ILogger<EmpleadoAsalariadoPorComisionServicio> registrador)
        : base(empleadoRepositorio, registrador)
    {
    }

    protected override EmpleadoAsalariadoPorComision ConstruirEmpleado(
        CrearEmpleadoAsalariadoPorComisionDto solicitud)
        => new(
            solicitud.PrimerNombre,
            solicitud.ApellidoPaterno,
            solicitud.NumeroSeguroSocial,
            solicitud.Departamento,
            solicitud.VentasBrutas,
            solicitud.TarifaComision,
            solicitud.SalarioBase);

    protected override void AplicarDatosDeContrato(
        EmpleadoAsalariadoPorComision empleado,
        ActualizarEmpleadoAsalariadoPorComisionDto solicitud)
        => empleado.ActualizarDatosDeContrato(
            solicitud.VentasBrutas,
            solicitud.TarifaComision,
            solicitud.SalarioBase);
}
