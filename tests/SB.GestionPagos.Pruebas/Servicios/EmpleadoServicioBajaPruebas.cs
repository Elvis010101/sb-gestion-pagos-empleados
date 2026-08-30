using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Enumeraciones;
using SB.GestionPagos.Dominio.Repositorios;
using SB.GestionPagos.Pruebas.Comunes;
using SB.GestionPagos.Servicios.Empleados;
using Xunit;

namespace SB.GestionPagos.Pruebas.Servicios;

/// <summary>
/// Consulta puntual y baja lógica de un empleado, con el repositorio simulado.
/// </summary>
/// <remarks>
/// Estas pruebas son la demostración de que <see cref="EmpleadoServicio"/> no depende de la
/// base de datos: se ejecutan con el contenedor de SQL Server apagado, sin cadena de conexión
/// y sin migraciones aplicadas. Eso es posible porque el servicio recibe
/// <see cref="IEmpleadoRepositorio"/> por constructor y nunca hace <c>new</c> de nada.
///
/// El caso de la baja es además el que mejor se presta a un doble: verificar que el empleado
/// que ya estaba inactivo NO se vuelve a guardar es una afirmación sobre una llamada que no
/// ocurre, y una llamada que no ocurre no deja rastro en ninguna base de datos.
/// </remarks>
public sealed class EmpleadoServicioBajaPruebas
{
    private const int IDENTIFICADOR_EXISTENTE = 7;

    private const int IDENTIFICADOR_INEXISTENTE = 999;

    private readonly IEmpleadoRepositorio _empleadoRepositorio = Substitute.For<IEmpleadoRepositorio>();

    private EmpleadoServicio CrearServicio()
        => new(_empleadoRepositorio, NullLogger<EmpleadoServicio>.Instance);

    /// <summary>
    /// La baja es lógica: cambia el estado y persiste, no borra la fila.
    /// </summary>
    [Fact]
    public async Task EliminarAsync_ConEmpleadoActivo_LoMarcaInactivoYPideGuardarlo()
    {
        // Arrange
        EmpleadoAsalariado empleado = EmpleadoDePrueba.Asalariado(1_500m);
        empleado.Estado.Should().Be(EstadoEmpleado.Activo, "un empleado nace activo");

        Empleado? empleadoGuardado = null;

        _empleadoRepositorio
            .ObtenerPorIdAsync(IDENTIFICADOR_EXISTENTE, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Empleado?>(empleado));

        _empleadoRepositorio
            .ActualizarAsync(
                Arg.Do<Empleado>(guardado => empleadoGuardado = guardado),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        Resultado resultado = await CrearServicio()
            .EliminarAsync(IDENTIFICADOR_EXISTENTE, CancellationToken.None);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        empleado.Estado.Should().Be(EstadoEmpleado.Inactivo);

        await _empleadoRepositorio.Received(1).ActualizarAsync(
            Arg.Any<Empleado>(),
            Arg.Any<CancellationToken>());

        // Se guarda la MISMA instancia que se leyó, no una copia: es lo que permite que el
        // seguimiento de cambios de EF Core reconozca la modificación.
        empleadoGuardado.Should().BeSameAs(empleado);
    }

    /// <summary>
    /// Dar de baja a quien ya está de baja no es un error, y tampoco una segunda escritura.
    /// </summary>
    /// <remarks>
    /// Es la definición de idempotencia, y aquí tiene una causa concreta: el doble clic en el
    /// botón de la maqueta. La primera petición da la baja; la segunda llega cuando el estado
    /// ya cambió. Responder 404 o 409 ahí sería mostrarle un error al usuario por haber
    /// conseguido exactamente lo que pidió.
    /// </remarks>
    [Fact]
    public async Task EliminarAsync_ConEmpleadoYaInactivo_InformaExitoYNoVuelveAGuardar()
    {
        // Arrange
        EmpleadoAsalariado empleado = EmpleadoDePrueba.Asalariado(1_500m);
        empleado.CambiarEstado(EstadoEmpleado.Inactivo);

        _empleadoRepositorio
            .ObtenerPorIdAsync(IDENTIFICADOR_EXISTENTE, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Empleado?>(empleado));

        // Act
        Resultado resultado = await CrearServicio()
            .EliminarAsync(IDENTIFICADOR_EXISTENTE, CancellationToken.None);

        // Assert
        resultado.EsExitoso.Should().BeTrue();

        await _empleadoRepositorio.DidNotReceive().ActualizarAsync(
            Arg.Any<Empleado>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EliminarAsync_ConIdentificadorInexistente_DevuelveNoEncontradoSinEscribir()
    {
        // Arrange
        _empleadoRepositorio
            .ObtenerPorIdAsync(IDENTIFICADOR_INEXISTENTE, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Empleado?>(null));

        // Act
        Resultado resultado = await CrearServicio()
            .EliminarAsync(IDENTIFICADOR_INEXISTENTE, CancellationToken.None);

        // Assert
        resultado.EsExitoso.Should().BeFalse();

        // Se afirma sobre el TIPO de error, no sobre el texto del mensaje: el tipo es lo que
        // la capa Api traduce a un código HTTP —404—, y el texto puede reescribirse o
        // traducirse sin que eso sea una regresión.
        resultado.TipoError.Should().Be(TipoErrorAplicacion.NoEncontrado);
        resultado.Mensaje.Should().NotBeEmpty();

        await _empleadoRepositorio.DidNotReceive().ActualizarAsync(
            Arg.Any<Empleado>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConIdentificadorInexistente_DevuelveNoEncontrado()
    {
        // Arrange
        _empleadoRepositorio
            .ObtenerPorIdAsync(IDENTIFICADOR_INEXISTENTE, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Empleado?>(null));

        // Act
        Resultado<EmpleadoDto> resultado = await CrearServicio()
            .ObtenerPorIdAsync(IDENTIFICADOR_INEXISTENTE, CancellationToken.None);

        // Assert
        resultado.EsExitoso.Should().BeFalse();
        resultado.TipoError.Should().Be(TipoErrorAplicacion.NoEncontrado);
        resultado.Valor.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConEmpleadoExistente_DevuelveSuDtoConElPagoCalculado()
    {
        // Arrange
        _empleadoRepositorio
            .ObtenerPorIdAsync(IDENTIFICADOR_EXISTENTE, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Empleado?>(EmpleadoDePrueba.PorHoras(20m, 45m)));

        // Act
        Resultado<EmpleadoDto> resultado = await CrearServicio()
            .ObtenerPorIdAsync(IDENTIFICADOR_EXISTENTE, CancellationToken.None);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        resultado.TipoError.Should().Be(TipoErrorAplicacion.Ninguno);
        resultado.Valor!.TipoContrato.Should().Be("Empleado por Horas");
        resultado.Valor.PagoSemanalCalculado.Should().Be(950m);
        resultado.Valor.SueldoPorHora.Should().Be(20m);
        resultado.Valor.HorasTrabajadas.Should().Be(45m);
    }

    /// <summary>
    /// El testigo de cancelación llega hasta el repositorio.
    /// </summary>
    /// <remarks>
    /// Es un descuido habitual: se declara el parámetro <c>CancellationToken</c>, se propaga
    /// en la firma y luego se llama al repositorio pasando <c>default</c>. Todo compila, todo
    /// funciona, y el día que un cliente cierre el navegador a mitad de una consulta pesada la
    /// consulta seguirá corriendo en el servidor hasta terminar.
    /// </remarks>
    [Fact]
    public async Task EliminarAsync_PropagaElTestigoDeCancelacionAlRepositorio()
    {
        // Arrange
        using CancellationTokenSource fuenteDeCancelacion = new();
        CancellationToken cancelacion = fuenteDeCancelacion.Token;

        _empleadoRepositorio
            .ObtenerPorIdAsync(IDENTIFICADOR_EXISTENTE, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Empleado?>(EmpleadoDePrueba.Asalariado(1_500m)));

        // Act
        await CrearServicio().EliminarAsync(IDENTIFICADOR_EXISTENTE, cancelacion);

        // Assert
        await _empleadoRepositorio.Received(1)
            .ObtenerPorIdAsync(IDENTIFICADOR_EXISTENTE, cancelacion);

        await _empleadoRepositorio.Received(1)
            .ActualizarAsync(Arg.Any<Empleado>(), cancelacion);
    }
}
