using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Servicios.Mapeos;

/// <summary>
/// Traduce entidades <see cref="Empleado"/> al DTO unificado que publica la Api.
/// </summary>
/// <remarks>
/// Aquí SÍ hay un <c>switch</c> sobre el tipo, y es una decisión consciente, no un descuido.
///
/// Lo que el RNF-02 exige es agregar tipos de empleado sin modificar el CÁLCULO, y eso se
/// cumple: la fórmula es polimórfica y vive en el Dominio; ningún servicio pregunta de qué
/// tipo es un empleado para saber cuánto pagarle.
///
/// Esto otro es una proyección de presentación. Se resolvió con coincidencia de patrones y no
/// con un registro de proyectores por tipo porque el registro escondería la ramificación sin
/// quitar el acoplamiento: <see cref="EmpleadoDto"/> ya enumera los cuatro tipos en sus campos
/// anulables, de modo que un quinto tipo obligaría a editarlo igual. La ramificación está
/// aislada en un único método, y el brazo final falla ruidosamente si aparece un tipo sin
/// contemplar, en vez de devolver un DTO con los campos del contrato en blanco.
/// </remarks>
internal static class MapeadorEmpleado
{
    internal static EmpleadoDto AEmpleadoDto(Empleado empleado)
    {
        EmpleadoDto datosComunes = new()
        {
            Id = empleado.Id,
            PrimerNombre = empleado.PrimerNombre,
            ApellidoPaterno = empleado.ApellidoPaterno,
            NumeroSeguroSocial = empleado.NumeroSeguroSocial,
            Departamento = empleado.Departamento,
            Estado = empleado.Estado,
            TipoContrato = empleado.TipoContrato,

            // El pago no se lee de la entidad: se le pide que lo calcule. No hay ningún
            // campo "pago" almacenado que pueda haber quedado desactualizado.
            PagoSemanalCalculado = empleado.CalcularPagoSemanal(),
            FechaCreacion = empleado.FechaCreacion
        };

        return empleado switch
        {
            EmpleadoAsalariado asalariado => datosComunes with
            {
                SalarioSemanal = asalariado.SalarioSemanal
            },

            EmpleadoPorHoras porHoras => datosComunes with
            {
                SueldoPorHora = porHoras.SueldoPorHora,
                HorasTrabajadas = porHoras.HorasTrabajadas
            },

            EmpleadoPorComision porComision => datosComunes with
            {
                VentasBrutas = porComision.VentasBrutas,
                TarifaComision = porComision.TarifaComision
            },

            EmpleadoAsalariadoPorComision asalariadoPorComision => datosComunes with
            {
                VentasBrutas = asalariadoPorComision.VentasBrutas,
                TarifaComision = asalariadoPorComision.TarifaComision,
                SalarioBase = asalariadoPorComision.SalarioBase
            },

            _ => throw new InvalidOperationException(
                $"No hay proyección definida para el tipo de empleado '{empleado.GetType().Name}'. " +
                "Al agregar un tipo nuevo hay que extender EmpleadoDto y este mapeo.")
        };
    }
}
