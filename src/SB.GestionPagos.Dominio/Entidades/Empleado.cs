using System.Diagnostics.CodeAnalysis;
using SB.GestionPagos.Dominio.Enumeraciones;
using SB.GestionPagos.Dominio.ObjetosDeValor;
using SB.GestionPagos.Dominio.Validaciones;

namespace SB.GestionPagos.Dominio.Entidades;

/// <summary>
/// Raíz de la jerarquía de empleados.
/// </summary>
/// <remarks>
/// Es abstracta porque "un empleado" sin tipo de contrato no existe en el negocio: no
/// habría forma de calcular su pago. El compilador impide instanciarla, de modo que todo
/// empleado que llegue a existir tiene, por construcción, una fórmula de pago propia.
/// </remarks>
public abstract class Empleado
{
    protected Empleado(
        string primerNombre,
        string apellidoPaterno,
        string numeroSeguroSocial,
        string departamento)
    {
        EstablecerDatosPersonales(primerNombre, apellidoPaterno, numeroSeguroSocial, departamento);

        // Un empleado nace activo: no hay ningún caso de negocio en el que se dé de alta
        // a alguien que ya está inactivo. Cambiar el estado es una operación posterior.
        Estado = EstadoEmpleado.Activo;

        // UTC y no hora local: el servidor puede cambiar de zona horaria o de servidor,
        // y el instante en que se creó el registro no debería cambiar con él.
        FechaCreacion = DateTime.UtcNow;
    }

    /// <summary>
    /// Identificador asignado por el motor de persistencia. El Dominio nunca lo fija.
    /// </summary>
    public int Id { get; private set; }

    public string PrimerNombre { get; private set; }

    public string ApellidoPaterno { get; private set; }

    public string NumeroSeguroSocial { get; private set; }

    public string Departamento { get; private set; }

    public EstadoEmpleado Estado { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    /// <summary>
    /// Nombre del tipo de contrato tal como debe aparecer en el reporte semanal (RF-06).
    /// </summary>
    /// <remarks>
    /// Es abstracto para que la capa de reportes no tenga que preguntar por el tipo del
    /// objeto: cada nuevo tipo de empleado trae su propia etiqueta.
    /// </remarks>
    public abstract string TipoContrato { get; }

    /// <summary>
    /// Único punto de extensión del cálculo: cada tipo de empleado declara CÓMO se compone
    /// su pago semanal, concepto por concepto.
    /// </summary>
    public abstract ResultadoPago CalcularDesglosePagoSemanal();

    /// <summary>
    /// Pago semanal total del empleado.
    /// </summary>
    /// <remarks>
    /// No es abstracto a propósito: se deriva del desglose, de modo que el total y el detalle
    /// del reporte salen de una sola fórmula y no pueden desincronizarse.
    /// </remarks>
    public decimal CalcularPagoSemanal() => CalcularDesglosePagoSemanal().Total;

    /// <summary>
    /// Corrige los datos personales del empleado, revalidando las mismas invariantes que
    /// exige el constructor (RF-05).
    /// </summary>
    public void ActualizarDatosPersonales(
        string primerNombre,
        string apellidoPaterno,
        string numeroSeguroSocial,
        string departamento)
        => EstablecerDatosPersonales(primerNombre, apellidoPaterno, numeroSeguroSocial, departamento);

    public void CambiarEstado(EstadoEmpleado nuevoEstado) => Estado = nuevoEstado;

    // El constructor y la actualización comparten exactamente las mismas reglas. Escribirlas
    // una sola vez evita que un empleado creado y un empleado editado se validen distinto.
    // [MemberNotNull] le dice al compilador que este método deja las cuatro propiedades
    // con valor; sin él, el análisis de nulabilidad no cruza la frontera del método.
    [MemberNotNull(
        nameof(PrimerNombre),
        nameof(ApellidoPaterno),
        nameof(NumeroSeguroSocial),
        nameof(Departamento))]
    private void EstablecerDatosPersonales(
        string primerNombre,
        string apellidoPaterno,
        string numeroSeguroSocial,
        string departamento)
    {
        PrimerNombre = ValidacionDominio.TextoRequerido(primerNombre, nameof(PrimerNombre));
        ApellidoPaterno = ValidacionDominio.TextoRequerido(apellidoPaterno, nameof(ApellidoPaterno));
        NumeroSeguroSocial = ValidacionDominio.TextoRequerido(numeroSeguroSocial, nameof(NumeroSeguroSocial));
        Departamento = ValidacionDominio.TextoRequerido(departamento, nameof(Departamento));
    }
}
