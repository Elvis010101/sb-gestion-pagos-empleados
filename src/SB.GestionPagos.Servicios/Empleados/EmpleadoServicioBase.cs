using Microsoft.Extensions.Logging;
using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Aplicacion.Empleados;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Repositorios;
using SB.GestionPagos.Servicios.Mapeos;

namespace SB.GestionPagos.Servicios.Empleados;

/// <summary>
/// Alta y edición de un tipo concreto de empleado.
/// </summary>
/// <typeparam name="TEmpleado">La entidad del Dominio que este servicio administra.</typeparam>
/// <typeparam name="TSolicitudCreacion">DTO de alta de ese tipo.</typeparam>
/// <typeparam name="TSolicitudActualizacion">DTO de edición de ese tipo.</typeparam>
/// <remarks>
/// Patrón Método Plantilla: el flujo de alta y de edición es idéntico para los cuatro tipos
/// —buscar, comprobar, aplicar, guardar, registrar, proyectar— y solo cambian dos pasos:
/// cómo se construye la entidad y qué datos de contrato se le aplican. Esos dos pasos son
/// abstractos; el resto se escribe una sola vez.
///
/// La consecuencia práctica es que un tipo de empleado nuevo cuesta una clase de unas veinte
/// líneas, y que ninguno de los cuatro servicios puede olvidarse de comprobar el número de
/// seguro social ni de registrar la operación en el log: no está en su código, está aquí.
/// </remarks>
public abstract class EmpleadoServicioBase<TEmpleado, TSolicitudCreacion, TSolicitudActualizacion>
    : IEmpleadoServicio<TSolicitudCreacion, TSolicitudActualizacion>
    where TEmpleado : Empleado
    where TSolicitudCreacion : class
    where TSolicitudActualizacion : class, ISolicitudActualizacionEmpleado
{
    private readonly IEmpleadoRepositorio _empleadoRepositorio;
    private readonly ILogger _registrador;

    protected EmpleadoServicioBase(IEmpleadoRepositorio empleadoRepositorio, ILogger registrador)
    {
        _empleadoRepositorio = empleadoRepositorio;
        _registrador = registrador;
    }

    public async Task<Resultado<EmpleadoDto>> CrearAsync(
        TSolicitudCreacion solicitud,
        CancellationToken cancelacion)
    {
        // Se construye ANTES de consultar la base a propósito: el constructor del Dominio
        // valida las invariantes y normaliza los textos, así que a partir de aquí se trabaja
        // con el número de seguro social ya recortado, no con el que llegó por la red.
        TEmpleado empleado = ConstruirEmpleado(solicitud);

        bool numeroYaRegistrado = await _empleadoRepositorio.ExisteNumeroSeguroSocialAsync(
            empleado.NumeroSeguroSocial,
            identificadorExcluido: null,
            cancelacion);

        if (numeroYaRegistrado)
        {
            _registrador.LogWarning(
                "Alta rechazada: número de seguro social duplicado. Tipo de contrato: {TipoContrato}.",
                empleado.TipoContrato);

            return Resultado<EmpleadoDto>.Conflicto(MensajesEmpleado.NumeroSeguroSocialDuplicado());
        }

        await _empleadoRepositorio.AgregarAsync(empleado, cancelacion);

        // El identificador ya viene asignado: lo puso el motor de persistencia al guardar.
        // El número de seguro social NO se registra en el log: es dato personal (A6).
        _registrador.LogInformation(
            "Empleado creado. Identificador: {IdentificadorEmpleado}. Tipo de contrato: {TipoContrato}. " +
            "Departamento: {Departamento}. Pago semanal calculado: {PagoSemanal}.",
            empleado.Id,
            empleado.TipoContrato,
            empleado.Departamento,
            empleado.CalcularPagoSemanal());

        return Resultado<EmpleadoDto>.Exitoso(MapeadorEmpleado.AEmpleadoDto(empleado));
    }

    /// <summary>
    /// Actualiza al empleado y devuelve su pago recalculado (RF-05).
    /// </summary>
    /// <remarks>
    /// No hay ningún paso de "recalcular". El pago no está almacenado: al proyectar la
    /// entidad al DTO se le pide que lo calcule, y como los datos del contrato ya cambiaron,
    /// el número que sale es el nuevo. El recálculo es una consecuencia del diseño, no una
    /// operación que alguien pueda olvidarse de invocar.
    /// </remarks>
    public async Task<Resultado<EmpleadoDto>> ActualizarAsync(
        int identificador,
        TSolicitudActualizacion solicitud,
        CancellationToken cancelacion)
    {
        Empleado? empleado = await _empleadoRepositorio.ObtenerPorIdAsync(identificador, cancelacion);

        if (empleado is null)
        {
            return Resultado<EmpleadoDto>.NoEncontrado(MensajesEmpleado.NoEncontrado(identificador));
        }

        // Un empleado no cambia de tipo de contrato editándolo. Si el identificador apunta a
        // otro tipo, la petición se dirigió a la operación equivocada.
        if (empleado is not TEmpleado empleadoDelTipo)
        {
            return Resultado<EmpleadoDto>.ReglaDeNegocio(
                MensajesEmpleado.TipoDeContratoDistinto(identificador, empleado.TipoContrato));
        }

        // Se recorta igual que lo hará el Dominio al asignarlo: comparar el texto crudo
        // dejaría pasar " 001 " como si fuera distinto de "001". La garantía dura de la
        // unicidad es el índice único de la base (Bloque 5); esto es lo que convierte ese
        // choque en un mensaje entendible en vez de un error del motor.
        bool numeroYaRegistrado = await _empleadoRepositorio.ExisteNumeroSeguroSocialAsync(
            solicitud.NumeroSeguroSocial.Trim(),
            identificadorExcluido: identificador,
            cancelacion);

        if (numeroYaRegistrado)
        {
            _registrador.LogWarning(
                "Edición rechazada: número de seguro social duplicado. Identificador: {IdentificadorEmpleado}.",
                identificador);

            return Resultado<EmpleadoDto>.Conflicto(MensajesEmpleado.NumeroSeguroSocialDuplicado());
        }

        empleadoDelTipo.ActualizarDatosPersonales(
            solicitud.PrimerNombre,
            solicitud.ApellidoPaterno,
            solicitud.NumeroSeguroSocial,
            solicitud.Departamento);

        empleadoDelTipo.CambiarEstado(solicitud.Estado);

        AplicarDatosDeContrato(empleadoDelTipo, solicitud);

        await _empleadoRepositorio.ActualizarAsync(empleadoDelTipo, cancelacion);

        _registrador.LogInformation(
            "Empleado actualizado. Identificador: {IdentificadorEmpleado}. Tipo de contrato: {TipoContrato}. " +
            "Estado: {EstadoEmpleado}. Pago semanal recalculado: {PagoSemanal}.",
            empleadoDelTipo.Id,
            empleadoDelTipo.TipoContrato,
            empleadoDelTipo.Estado,
            empleadoDelTipo.CalcularPagoSemanal());

        return Resultado<EmpleadoDto>.Exitoso(MapeadorEmpleado.AEmpleadoDto(empleadoDelTipo));
    }

    /// <summary>
    /// Crea la entidad del Dominio a partir del DTO de alta de este tipo.
    /// </summary>
    /// <remarks>
    /// Es lo único que el servicio concreto sabe hacer y la base no: qué constructor invocar
    /// y con qué argumentos. Toda la validación de los valores ocurre dentro de ese
    /// constructor, en el Dominio.
    /// </remarks>
    protected abstract TEmpleado ConstruirEmpleado(TSolicitudCreacion solicitud);

    /// <summary>
    /// Traslada a la entidad los datos propios del contrato de este tipo.
    /// </summary>
    /// <remarks>
    /// Los datos personales y el estado ya los aplicó la clase base: aquí solo van el salario,
    /// las horas, las ventas o la comisión, según el tipo.
    /// </remarks>
    protected abstract void AplicarDatosDeContrato(TEmpleado empleado, TSolicitudActualizacion solicitud);
}
