using Microsoft.Extensions.Logging;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Repositorios;

namespace SB.GestionPagos.Servicios.Empleados;

/// <summary>Alta y edición de empleados pagados por hora.</summary>
public sealed class EmpleadoPorHorasServicio
    : EmpleadoServicioBase<EmpleadoPorHoras, CrearEmpleadoPorHorasDto, ActualizarEmpleadoPorHorasDto>
{
    public EmpleadoPorHorasServicio(
        IEmpleadoRepositorio empleadoRepositorio,
        ILogger<EmpleadoPorHorasServicio> registrador)
        : base(empleadoRepositorio, registrador)
    {
    }

    protected override EmpleadoPorHoras ConstruirEmpleado(CrearEmpleadoPorHorasDto solicitud)
        => new(
            solicitud.PrimerNombre,
            solicitud.ApellidoPaterno,
            solicitud.NumeroSeguroSocial,
            solicitud.Departamento,
            solicitud.SueldoPorHora,
            solicitud.HorasTrabajadas);

    protected override void AplicarDatosDeContrato(
        EmpleadoPorHoras empleado,
        ActualizarEmpleadoPorHorasDto solicitud)
        => empleado.ActualizarDatosDeContrato(solicitud.SueldoPorHora, solicitud.HorasTrabajadas);
}
