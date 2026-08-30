using Microsoft.Extensions.Logging;
using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Aplicacion.Reportes;
using SB.GestionPagos.Aplicacion.Reportes.Dtos;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Enumeraciones;
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

    private const string POBLACION_SOLO_ACTIVOS = "Empleados activos";

    private const string POBLACION_ACTIVOS_E_INACTIVOS = "Empleados activos e inactivos";

    private const string ALCANCE_TODOS_LOS_DEPARTAMENTOS = "de todos los departamentos";

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
        // Regla de negocio: a un empleado dado de baja no se le paga la semana. Por eso el
        // reporte se restringe a los activos salvo que se pida lo contrario de forma explícita.
        EstadoEmpleado? estadoRequerido = filtro.IncluirInactivos ? null : EstadoEmpleado.Activo;

        FiltroBusquedaEmpleado criterios = new(
            Nombre: null,
            Departamento: filtro.Departamento,
            Estado: estadoRequerido);

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
            // El desglose se pide UNA sola vez por empleado y se reutiliza.
            ResultadoPago desglose = empleado.CalcularDesglosePagoSemanal();

            // Y su total también se lee una sola vez: ResultadoPago.Total es una propiedad
            // calculada que recorre y suma las líneas en cada lectura, no un valor guardado.
            decimal pagoSemanal = desglose.Total;

            totalNominaSemanal += pagoSemanal;

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
                pagoSemanal,
                desgloseDto));
        }

        string poblacionIncluida = DescribirPoblacion(filtro);

        _registrador.LogInformation(
            "Reporte semanal generado. Población: {PoblacionIncluida}. " +
            "Empleados incluidos: {CantidadEmpleados}. Total de la nómina: {TotalNominaSemanal}.",
            poblacionIncluida,
            lineas.Count,
            totalNominaSemanal);

        ReporteSemanalDto reporte = new(
            DateTime.UtcNow,
            poblacionIncluida,
            filtro.Departamento,
            filtro.IncluirInactivos,
            lineas.Count,
            totalNominaSemanal,
            lineas);

        return Resultado<ReporteSemanalDto>.Exitoso(reporte);
    }

    /// <summary>
    /// Arma la frase del encabezado que dice a quiénes cubre el reporte.
    /// </summary>
    /// <remarks>
    /// Se construye en el servidor, junto al número, y no en la interfaz: el total y la
    /// descripción de a quiénes corresponde tienen que viajar como una sola unidad.
    /// </remarks>
    private static string DescribirPoblacion(FiltroReporteSemanal filtro)
    {
        string estado = filtro.IncluirInactivos
            ? POBLACION_ACTIVOS_E_INACTIVOS
            : POBLACION_SOLO_ACTIVOS;

        string alcance = string.IsNullOrWhiteSpace(filtro.Departamento)
            ? ALCANCE_TODOS_LOS_DEPARTAMENTOS
            : $"del departamento de {filtro.Departamento.Trim()}";

        return $"{estado} {alcance}";
    }
}
