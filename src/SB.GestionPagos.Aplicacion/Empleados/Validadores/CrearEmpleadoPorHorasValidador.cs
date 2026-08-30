using FluentValidation;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Aplicacion.Validaciones;
using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Aplicacion.Empleados.Validadores;

/// <summary>
/// Valida la forma del DTO de alta de un Empleado por Horas.
/// </summary>
/// <remarks>
/// Los límites de horas se toman de las constantes de <see cref="EmpleadoPorHoras"/> en vez
/// de repetirse aquí: el validador y la invariante del Dominio comparten el mismo número, así
/// que no pueden discrepar.
/// </remarks>
public sealed class CrearEmpleadoPorHorasValidador : AbstractValidator<CrearEmpleadoPorHorasDto>
{
    public CrearEmpleadoPorHorasValidador()
    {
        RuleFor(solicitud => solicitud.PrimerNombre).TextoObligatorio(LongitudMaxima.PRIMER_NOMBRE);
        RuleFor(solicitud => solicitud.ApellidoPaterno).TextoObligatorio(LongitudMaxima.APELLIDO_PATERNO);
        RuleFor(solicitud => solicitud.NumeroSeguroSocial).TextoObligatorio(LongitudMaxima.NUMERO_SEGURO_SOCIAL);
        RuleFor(solicitud => solicitud.Departamento).TextoObligatorio(LongitudMaxima.DEPARTAMENTO);
        RuleFor(solicitud => solicitud.SueldoPorHora).MontoNoNegativo();
        RuleFor(solicitud => solicitud.HorasTrabajadas).EnRangoInclusivo(
            EmpleadoPorHoras.HORAS_MINIMAS_SEMANALES,
            EmpleadoPorHoras.HORAS_MAXIMAS_SEMANALES);
    }
}
