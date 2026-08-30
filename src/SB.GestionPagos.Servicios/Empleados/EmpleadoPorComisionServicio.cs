using Microsoft.Extensions.Logging;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Repositorios;

namespace SB.GestionPagos.Servicios.Empleados;

/// <summary>Alta y edición de empleados que cobran solo comisión.</summary>
public sealed class EmpleadoPorComisionServicio
    : EmpleadoServicioBase<EmpleadoPorComision, CrearEmpleadoPorComisionDto, ActualizarEmpleadoPorComisionDto>
{
    public EmpleadoPorComisionServicio(
        IEmpleadoRepositorio empleadoRepositorio,
        ILogger<EmpleadoPorComisionServicio> registrador)
        : base(empleadoRepositorio, registrador)
    {
    }

    protected override EmpleadoPorComision ConstruirEmpleado(CrearEmpleadoPorComisionDto solicitud)
        => new(
            solicitud.PrimerNombre,
            solicitud.ApellidoPaterno,
            solicitud.NumeroSeguroSocial,
            solicitud.Departamento,
            solicitud.VentasBrutas,
            solicitud.TarifaComision);

    protected override void AplicarDatosDeContrato(
        EmpleadoPorComision empleado,
        ActualizarEmpleadoPorComisionDto solicitud)
        => empleado.ActualizarDatosDeContrato(solicitud.VentasBrutas, solicitud.TarifaComision);
}
