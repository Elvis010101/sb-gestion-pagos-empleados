using Microsoft.Extensions.Logging;
using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Aplicacion.Reportes;
using SB.GestionPagos.Aplicacion.Reportes.Dtos;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.ObjetosDeValor;
using SB.GestionPagos.Dominio.Repositorios;

namespace SB.GestionPagos.Servicios.Reportes;

/// <summary>
/// Reporte semanal de pagos con el desglose del cálculo por tipo de contrato (RF-06).
/// </summary>
/// <remarks>
/// El servicio no sabe sumar horas extra ni aplicar comisiones. Le pide a cada empleado su
/// propio desglose y se limita a acumular. Si mañana aparece un quinto tipo de contrato con
/// una fórmula distinta, esta clase no cambia ni una línea: ya funciona.
/// </remarks>
public sealed class ReporteServicio : IReporteServicio
{
    private const decimal TOTAL_INICIAL_NOMINA = 0m;

    private readonly IEmpleadoRepositorio _empleadoRepositorio;
    private readonly ILogger<ReporteServicio> _registrador;

    public ReporteServicio(IEmpleadoRepositorio empleadoRepositorio, ILogger<ReporteServicio> registrador)
    {
        _empleadoRepositorio = empleadoRepositorio;
        _registrador = registrador;
    }

    public async Task<Resultado<ReporteSemanalDto>> GenerarReporteSemanalAsync(
        FiltroReporteSemanal filtro,
        CancellationToken cancelacion)
    {
        FiltroBusquedaEmpleado criterios = new(
            Nombre: null,
            Departamento: filtro.Departamento,
            Estado: filtro.Estado);

        // ListarAsync y no BuscarPaginaAsync: el total de una nómina tiene que ser el total.
        // Este es el único camino del sistema que trae la colección completa, y es justo el
        // escenario que cronometra el RNF-04.
        IReadOnlyList<Empleado> empleados = await _empleadoRepositorio.ListarAsync(criterios, cancelacion);

        // Se dimensiona la lista de antemano para que no tenga que crecer y recopiarse varias
        // veces mientras se llenan mil filas.
        List<LineaReporteEmpleadoDto> lineas = new(empleados.Count);
        decimal totalNominaSemanal = TOTAL_INICIAL_NOMINA;

        foreach (Empleado empleado in empleados)
        {
            // El desglose se pide UNA sola vez por empleado y se reutiliza. ResultadoPago.Total
            // recorre y suma las líneas cada vez que se lee, así que consultarlo dentro del
            // bucle y otra vez al armar la fila duplicaría el trabajo sin necesidad.
            ResultadoPago desglose = empleado.CalcularDesglosePagoSemanal();

            totalNominaSemanal += desglose.Total;

            List<LineaCalculoDto> desgloseDto = new(desglose.Lineas.Count);
            foreach (LineaCalculo linea in desglose.Lineas)
            {
                desgloseDto.Add(new LineaCalculoDto(linea.Concepto, linea.Monto));
            }

            lineas.Add(new LineaReporteEmpleadoDto(
                empleado.Id,
                $"{empleado.PrimerNombre} {empleado.ApellidoPaterno}",
                empleado.Departamento,
                empleado.TipoContrato,
                desglose.Total,
                desgloseDto));
        }

        _registrador.LogInformation(
            "Reporte semanal generado. Empleados incluidos: {CantidadEmpleados}. " +
            "Total de la nómina: {TotalNominaSemanal}. Filtro de departamento: {Departamento}. " +
            "Filtro de estado: {EstadoEmpleado}.",
            lineas.Count,
            totalNominaSemanal,
            filtro.Departamento,
            filtro.Estado);

        ReporteSemanalDto reporte = new(
            DateTime.UtcNow,
            lineas.Count,
            totalNominaSemanal,
            lineas);

        return Resultado<ReporteSemanalDto>.Exitoso(reporte);
    }
}
