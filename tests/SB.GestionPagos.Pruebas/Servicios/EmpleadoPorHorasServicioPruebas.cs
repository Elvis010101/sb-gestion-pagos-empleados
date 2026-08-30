using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Enumeraciones;
using SB.GestionPagos.Dominio.Excepciones;
using SB.GestionPagos.Dominio.Repositorios;
using SB.GestionPagos.Pruebas.Comunes;
using SB.GestionPagos.Servicios.Empleados;
using Xunit;

namespace SB.GestionPagos.Pruebas.Servicios;

/// <summary>
/// Alta y edición de empleados por horas (RF-02 y RF-05).
/// </summary>
/// <remarks>
/// Aunque la clase bajo prueba se llame <see cref="EmpleadoPorHorasServicio"/>, casi todo lo
/// que se verifica aquí vive en <c>EmpleadoServicioBase</c>: la comprobación del número de
/// seguro social, el rechazo del tipo de contrato equivocado y el recálculo del pago son pasos
/// del método plantilla, escritos una sola vez para los cuatro tipos. Probarlos a través de
/// una subclase concreta es lo correcto: es como se ejecutan en producción, y de paso verifica
/// que los dos pasos que la subclase sí aporta —construir la entidad y aplicar los datos del
/// contrato— están bien enganchados.
/// </remarks>
public sealed class EmpleadoPorHorasServicioPruebas
{
    private const int IDENTIFICADOR_EXISTENTE = 7;

    private const int IDENTIFICADOR_INEXISTENTE = 999;

    private const string NUMERO_SEGURO_SOCIAL = "402-1234567-8";

    private readonly IEmpleadoRepositorio _empleadoRepositorio = Substitute.For<IEmpleadoRepositorio>();

    private EmpleadoPorHorasServicio CrearServicio()
        => new(_empleadoRepositorio, NullLogger<EmpleadoPorHorasServicio>.Instance);

    private static CrearEmpleadoPorHorasDto SolicitudDeAlta(
        decimal sueldoPorHora = 20m,
        decimal horasTrabajadas = 45m,
        string numeroSeguroSocial = NUMERO_SEGURO_SOCIAL)
        => new(
            PrimerNombre: "Ana",
            ApellidoPaterno: "Rodríguez",
            NumeroSeguroSocial: numeroSeguroSocial,
            Departamento: "Tecnología",
            SueldoPorHora: sueldoPorHora,
            HorasTrabajadas: horasTrabajadas);

    private static ActualizarEmpleadoPorHorasDto SolicitudDeEdicion(
        decimal sueldoPorHora = 20m,
        decimal horasTrabajadas = 45m,
        EstadoEmpleado estado = EstadoEmpleado.Activo)
        => new(
            PrimerNombre: "Ana",
            ApellidoPaterno: "Rodríguez",
            NumeroSeguroSocial: NUMERO_SEGURO_SOCIAL,
            Departamento: "Tecnología",
            Estado: estado,
            SueldoPorHora: sueldoPorHora,
            HorasTrabajadas: horasTrabajadas);

    // ---------------------------------------------------------------------------------
    // Alta
    // ---------------------------------------------------------------------------------

    [Fact]
    public async Task CrearAsync_ConDatosValidos_GuardaLaEntidadDelTipoCorrectoYDevuelveSuPago()
    {
        // Arrange
        Empleado? empleadoAgregado = null;

        _empleadoRepositorio
            .AgregarAsync(
                Arg.Do<Empleado>(empleado => empleadoAgregado = empleado),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        Resultado<EmpleadoDto> resultado = await CrearServicio()
            .CrearAsync(SolicitudDeAlta(sueldoPorHora: 20m, horasTrabajadas: 45m), CancellationToken.None);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor!.PagoSemanalCalculado.Should().Be(950m);
        resultado.Valor.Estado.Should().Be(EstadoEmpleado.Activo, "un empleado se da de alta activo");

        EmpleadoPorHoras entidadGuardada = empleadoAgregado.Should().BeOfType<EmpleadoPorHoras>().Subject;
        entidadGuardada.SueldoPorHora.Should().Be(20m);
        entidadGuardada.HorasTrabajadas.Should().Be(45m);
        entidadGuardada.NumeroSeguroSocial.Should().Be(NUMERO_SEGURO_SOCIAL);
    }

    [Fact]
    public async Task CrearAsync_ConNumeroSeguroSocialYaRegistrado_DevuelveConflictoYNoGuarda()
    {
        // Arrange
        _empleadoRepositorio
            .ExisteNumeroSeguroSocialAsync(
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Resultado<EmpleadoDto> resultado = await CrearServicio()
            .CrearAsync(SolicitudDeAlta(), CancellationToken.None);

        // Assert
        resultado.EsExitoso.Should().BeFalse();
        resultado.TipoError.Should().Be(TipoErrorAplicacion.Conflicto);

        await _empleadoRepositorio.DidNotReceive()
            .AgregarAsync(Arg.Any<Empleado>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// La unicidad se consulta con el número YA normalizado por el Dominio.
    /// </summary>
    /// <remarks>
    /// Esta prueba es la que le da sentido a una decisión que en el código parece arbitraria:
    /// el servicio construye la entidad ANTES de consultar la base. Como el constructor
    /// recorta los espacios, a partir de ahí se trabaja con "402-1234567-8" y no con lo que
    /// llegó por la red. Si el orden se invirtiera, " 402-1234567-8 " no coincidiría con el
    /// registro existente, la comprobación pasaría, y el choque lo reportaría el índice único
    /// de la base como un error 500 en vez de como un mensaje entendible.
    /// </remarks>
    [Fact]
    public async Task CrearAsync_ConEspaciosEnElNumeroSeguroSocial_ConsultaLaUnicidadConElNumeroRecortado()
    {
        // Arrange
        string? numeroConsultado = null;
        int? identificadorExcluido = -1;

        _empleadoRepositorio
            .ExisteNumeroSeguroSocialAsync(
                Arg.Do<string>(numero => numeroConsultado = numero),
                Arg.Do<int?>(identificador => identificadorExcluido = identificador),
                Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await CrearServicio().CrearAsync(
            SolicitudDeAlta(numeroSeguroSocial: "  402-1234567-8  "),
            CancellationToken.None);

        // Assert
        numeroConsultado.Should().Be(NUMERO_SEGURO_SOCIAL);

        // Al dar de alta no hay ningún empleado que excluir: nadie puede chocar consigo mismo.
        identificadorExcluido.Should().BeNull();
    }

    /// <summary>
    /// Un dato que el Dominio rechaza no llega a tocar el repositorio.
    /// </summary>
    [Fact]
    public async Task CrearAsync_ConHorasInvalidas_FallaAntesDeConsultarElRepositorio()
    {
        // Act
        Func<Task> alta = () => CrearServicio()
            .CrearAsync(SolicitudDeAlta(horasTrabajadas: -8m), CancellationToken.None);

        // Assert
        await alta.Should().ThrowAsync<ExcepcionValorFueraDeRango>();

        await _empleadoRepositorio.DidNotReceive().ExisteNumeroSeguroSocialAsync(
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<CancellationToken>());

        await _empleadoRepositorio.DidNotReceive()
            .AgregarAsync(Arg.Any<Empleado>(), Arg.Any<CancellationToken>());
    }

    // ---------------------------------------------------------------------------------
    // Edición
    // ---------------------------------------------------------------------------------

    /// <summary>
    /// El pago del RF-05 se "recalcula" sin que exista ningún paso de recálculo.
    /// </summary>
    /// <remarks>
    /// El empleado entra con 40 horas —800— y sale con 45 —950—. En el servicio no hay ninguna
    /// línea que diga "recalcular": el pago no está almacenado, se pide al proyectar. Esta
    /// prueba verifica esa propiedad del diseño, no una instrucción concreta del código.
    /// </remarks>
    [Fact]
    public async Task ActualizarAsync_AlCambiarLasHoras_DevuelveElPagoRecalculado()
    {
        // Arrange
        EmpleadoPorHoras empleado = EmpleadoDePrueba.PorHoras(sueldoPorHora: 20m, horasTrabajadas: 40m);
        empleado.CalcularPagoSemanal().Should().Be(800m, "es el pago antes de la edición");

        _empleadoRepositorio
            .ObtenerPorIdAsync(IDENTIFICADOR_EXISTENTE, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Empleado?>(empleado));

        // Act
        Resultado<EmpleadoDto> resultado = await CrearServicio().ActualizarAsync(
            IDENTIFICADOR_EXISTENTE,
            SolicitudDeEdicion(sueldoPorHora: 20m, horasTrabajadas: 45m, estado: EstadoEmpleado.Inactivo),
            CancellationToken.None);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor!.PagoSemanalCalculado.Should().Be(950m);
        resultado.Valor.HorasTrabajadas.Should().Be(45m);
        resultado.Valor.Estado.Should().Be(EstadoEmpleado.Inactivo);

        await _empleadoRepositorio.Received(1)
            .ActualizarAsync(empleado, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Un empleado no cambia de tipo de contrato editándolo.
    /// </summary>
    /// <remarks>
    /// Si el identificador 7 corresponde a un asalariado, editarlo con el formulario de por
    /// horas es dirigirse a la operación equivocada. Devolver una regla de negocio —400— y no
    /// un 404 es lo correcto: el empleado existe, lo que no encaja es la operación elegida.
    /// </remarks>
    [Fact]
    public async Task ActualizarAsync_SobreUnEmpleadoDeOtroTipo_DevuelveReglaDeNegocioYNoGuarda()
    {
        // Arrange
        _empleadoRepositorio
            .ObtenerPorIdAsync(IDENTIFICADOR_EXISTENTE, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Empleado?>(EmpleadoDePrueba.Asalariado(1_500m)));

        // Act
        Resultado<EmpleadoDto> resultado = await CrearServicio().ActualizarAsync(
            IDENTIFICADOR_EXISTENTE,
            SolicitudDeEdicion(),
            CancellationToken.None);

        // Assert
        resultado.EsExitoso.Should().BeFalse();
        resultado.TipoError.Should().Be(TipoErrorAplicacion.ReglaDeNegocio);
        resultado.Mensaje.Should().Contain("Empleado Asalariado");

        await _empleadoRepositorio.DidNotReceive()
            .ActualizarAsync(Arg.Any<Empleado>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Al editar, el empleado no debe chocar consigo mismo.
    /// </summary>
    /// <remarks>
    /// Sin el parámetro <c>identificadorExcluido</c>, guardar un empleado sin tocarle el
    /// número de seguro social daría conflicto contra su propio registro: sería imposible
    /// corregirle el departamento a nadie.
    /// </remarks>
    [Fact]
    public async Task ActualizarAsync_ComprobandoLaUnicidad_SeExcluyeASiMismo()
    {
        // Arrange
        int? identificadorExcluido = null;

        _empleadoRepositorio
            .ObtenerPorIdAsync(IDENTIFICADOR_EXISTENTE, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Empleado?>(EmpleadoDePrueba.PorHoras(20m, 40m)));

        _empleadoRepositorio
            .ExisteNumeroSeguroSocialAsync(
                Arg.Any<string>(),
                Arg.Do<int?>(identificador => identificadorExcluido = identificador),
                Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await CrearServicio().ActualizarAsync(
            IDENTIFICADOR_EXISTENTE,
            SolicitudDeEdicion(),
            CancellationToken.None);

        // Assert
        identificadorExcluido.Should().Be(IDENTIFICADOR_EXISTENTE);
    }

    [Fact]
    public async Task ActualizarAsync_ConIdentificadorInexistente_DevuelveNoEncontrado()
    {
        // Arrange
        _empleadoRepositorio
            .ObtenerPorIdAsync(IDENTIFICADOR_INEXISTENTE, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Empleado?>(null));

        // Act
        Resultado<EmpleadoDto> resultado = await CrearServicio().ActualizarAsync(
            IDENTIFICADOR_INEXISTENTE,
            SolicitudDeEdicion(),
            CancellationToken.None);

        // Assert
        resultado.EsExitoso.Should().BeFalse();
        resultado.TipoError.Should().Be(TipoErrorAplicacion.NoEncontrado);

        await _empleadoRepositorio.DidNotReceive()
            .ActualizarAsync(Arg.Any<Empleado>(), Arg.Any<CancellationToken>());
    }
}
