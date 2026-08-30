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
/// Filtrado y paginación de la consulta de empleados (RF-03).
/// </summary>
/// <remarks>
/// No hay ninguna base de datos detrás de estas pruebas, y sin embargo el filtrado se verifica
/// de verdad. Lo que se comprueba no es que SQL Server sepa hacer un WHERE —eso se da por
/// hecho—, sino lo único que puede romperse aquí: que el servicio traslade los criterios
/// íntegros al repositorio y que NO vuelva a filtrar ni a contar por su cuenta.
///
/// El repositorio es un MOCK (se verifican las llamadas que recibe). El registrador es un
/// STUB (<c>NullLogger</c>): existe solo para que el constructor se satisfaga, y ninguna
/// aserción habla de él.
/// </remarks>
public sealed class EmpleadoServicioBusquedaPruebas
{
    private readonly IEmpleadoRepositorio _empleadoRepositorio = Substitute.For<IEmpleadoRepositorio>();

    private EmpleadoServicio CrearServicio()
        => new(_empleadoRepositorio, NullLogger<EmpleadoServicio>.Instance);

    /// <summary>
    /// Los tres criterios del RF-03 tienen que llegar al repositorio tal como entraron.
    /// </summary>
    /// <remarks>
    /// El fallo que esta prueba caza es el más aburrido y el más frecuente: cruzar dos
    /// parámetros al construir el filtro del Dominio. Con <c>Nombre</c> y <c>Departamento</c>
    /// intercambiados, la aplicación compila, arranca, responde 200 y devuelve la lista
    /// equivocada sin decir nada.
    /// </remarks>
    [Fact]
    public async Task BuscarAsync_ConTodosLosCriterios_LosTrasladaIntactosAlRepositorio()
    {
        // Arrange
        FiltroBusquedaEmpleado? criteriosRecibidos = null;
        Paginacion? paginacionRecibida = null;

        _empleadoRepositorio
            .BuscarPaginaAsync(
                Arg.Do<FiltroBusquedaEmpleado>(criterios => criteriosRecibidos = criterios),
                Arg.Do<Paginacion>(paginacion => paginacionRecibida = paginacion),
                Arg.Any<CancellationToken>())
            .Returns(new PaginaDeRegistros<Empleado>(Array.Empty<Empleado>(), 0));

        FiltroEmpleados filtro = new()
        {
            Nombre = "Ana",
            Departamento = "Tecnología",
            Estado = EstadoEmpleado.Activo,
            Pagina = 2,
            TamanoPagina = 25
        };

        // Act
        await CrearServicio().BuscarAsync(filtro, CancellationToken.None);

        // Assert
        // FiltroBusquedaEmpleado es un record: su igualdad es estructural, así que una sola
        // aserción cubre los tres criterios y falla nombrando el que se desvió.
        criteriosRecibidos.Should().Be(
            new FiltroBusquedaEmpleado("Ana", "Tecnología", EstadoEmpleado.Activo));

        paginacionRecibida.Should().NotBeNull();
        paginacionRecibida!.Pagina.Should().Be(2);
        paginacionRecibida.TamanoPagina.Should().Be(25);
        paginacionRecibida.RegistrosOmitidos.Should().Be(25);

        await _empleadoRepositorio.Received(1).BuscarPaginaAsync(
            Arg.Any<FiltroBusquedaEmpleado>(),
            Arg.Any<Paginacion>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Un criterio ausente significa "no filtrar por esto", no "filtrar por vacío".
    /// </summary>
    [Fact]
    public async Task BuscarAsync_SinCriterios_PideAlRepositorioQueNoFiltre()
    {
        // Arrange
        FiltroBusquedaEmpleado? criteriosRecibidos = null;
        Paginacion? paginacionRecibida = null;

        _empleadoRepositorio
            .BuscarPaginaAsync(
                Arg.Do<FiltroBusquedaEmpleado>(criterios => criteriosRecibidos = criterios),
                Arg.Do<Paginacion>(paginacion => paginacionRecibida = paginacion),
                Arg.Any<CancellationToken>())
            .Returns(new PaginaDeRegistros<Empleado>(Array.Empty<Empleado>(), 0));

        // Act
        await CrearServicio().BuscarAsync(new FiltroEmpleados(), CancellationToken.None);

        // Assert
        criteriosRecibidos.Should().Be(new FiltroBusquedaEmpleado(null, null, null));

        // Y aunque no haya criterios, SIEMPRE hay paginación: una consulta sin página es una
        // consulta sin límite, y el RNF-04 no sobreviviría a eso.
        paginacionRecibida!.Pagina.Should().Be(FiltroEmpleados.PAGINA_PREDETERMINADA);
        paginacionRecibida.TamanoPagina.Should().Be(FiltroEmpleados.TAMANO_PAGINA_PREDETERMINADO);
    }

    /// <summary>
    /// El total de la respuesta es el que informa el repositorio, no la cantidad de filas
    /// que trajo la página.
    /// </summary>
    /// <remarks>
    /// Es la prueba que sostiene el paginador de la interfaz. Si alguien "simplificara" el
    /// servicio devolviendo <c>Elementos.Count</c> como total, la pantalla mostraría siempre
    /// una sola página y los 54 empleados restantes quedarían invisibles, sin ningún error a
    /// la vista.
    /// </remarks>
    [Fact]
    public async Task BuscarAsync_TotalDeRegistros_LoInformaElRepositorioYNoElTamanoDeLaPagina()
    {
        // Arrange
        Empleado[] tresEmpleados =
        {
            EmpleadoDePrueba.Asalariado(1_500m),
            EmpleadoDePrueba.PorHoras(20m, 45m),
            EmpleadoDePrueba.PorComision(10_000m, 0.10m)
        };

        _empleadoRepositorio
            .BuscarPaginaAsync(
                Arg.Any<FiltroBusquedaEmpleado>(),
                Arg.Any<Paginacion>(),
                Arg.Any<CancellationToken>())
            .Returns(new PaginaDeRegistros<Empleado>(tresEmpleados, TotalRegistros: 57));

        FiltroEmpleados filtro = new() { Pagina = 1, TamanoPagina = 20 };

        // Act
        Resultado<PaginaDto<EmpleadoDto>> resultado =
            await CrearServicio().BuscarAsync(filtro, CancellationToken.None);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor!.Elementos.Should().HaveCount(3);
        resultado.Valor.TotalRegistros.Should().Be(57);
        resultado.Valor.Pagina.Should().Be(1);
        resultado.Valor.TamanoPagina.Should().Be(20);

        // 57 registros en páginas de 20 son 3 páginas: la división redondea hacia arriba.
        resultado.Valor.TotalPaginas.Should().Be(3);
    }

    /// <summary>
    /// El servicio no vuelve a filtrar en memoria lo que ya filtró el repositorio.
    /// </summary>
    /// <remarks>
    /// Parece una prueba extraña —el repositorio devuelve empleados de un departamento que no
    /// es el pedido y se espera que el servicio los entregue igual—, y es precisamente el
    /// punto. Un "por si acaso" que volviera a aplicar el filtro sobre la página ya recortada
    /// rompería la paginación en silencio: una página de 20 podría devolver 4 elementos
    /// diciendo que hay 57. El filtrado se delega, y delegarlo significa confiar.
    /// </remarks>
    [Fact]
    public async Task BuscarAsync_DevuelveExactamenteLoQueEntregaElRepositorio_SinRefiltrar()
    {
        // Arrange
        Empleado[] empleadosDelRepositorio =
        {
            EmpleadoDePrueba.Asalariado(1_500m, departamento: "Finanzas"),
            EmpleadoDePrueba.Asalariado(2_000m, departamento: "Recursos Humanos")
        };

        _empleadoRepositorio
            .BuscarPaginaAsync(
                Arg.Any<FiltroBusquedaEmpleado>(),
                Arg.Any<Paginacion>(),
                Arg.Any<CancellationToken>())
            .Returns(new PaginaDeRegistros<Empleado>(empleadosDelRepositorio, TotalRegistros: 2));

        FiltroEmpleados filtro = new() { Departamento = "Tecnología" };

        // Act
        Resultado<PaginaDto<EmpleadoDto>> resultado =
            await CrearServicio().BuscarAsync(filtro, CancellationToken.None);

        // Assert
        resultado.Valor!.Elementos.Select(empleado => empleado.Departamento)
            .Should().Equal("Finanzas", "Recursos Humanos");
    }

    /// <summary>
    /// Cada fila de la consulta llega con su pago ya calculado por el Dominio (RF-04).
    /// </summary>
    /// <remarks>
    /// La página mezcla tres tipos de contrato a propósito: verifica de paso que la proyección
    /// resuelve el pago por polimorfismo y no por el tipo declarado de la colección, que es
    /// <see cref="Empleado"/> para las tres filas.
    /// </remarks>
    [Fact]
    public async Task BuscarAsync_ProyectaCadaEmpleadoConSuPagoSemanalYaCalculado()
    {
        // Arrange
        Empleado[] empleadosDelRepositorio =
        {
            EmpleadoDePrueba.Asalariado(1_500m),
            EmpleadoDePrueba.PorHoras(20m, 45m),
            EmpleadoDePrueba.AsalariadoPorComision(10_000m, 0.05m, 1_000m)
        };

        _empleadoRepositorio
            .BuscarPaginaAsync(
                Arg.Any<FiltroBusquedaEmpleado>(),
                Arg.Any<Paginacion>(),
                Arg.Any<CancellationToken>())
            .Returns(new PaginaDeRegistros<Empleado>(empleadosDelRepositorio, TotalRegistros: 3));

        // Act
        Resultado<PaginaDto<EmpleadoDto>> resultado =
            await CrearServicio().BuscarAsync(new FiltroEmpleados(), CancellationToken.None);

        // Assert
        resultado.Valor!.Elementos.Select(empleado => empleado.TipoContrato)
            .Should().Equal("Empleado Asalariado", "Empleado por Horas", "Empleado Asalariado por Comisión");

        resultado.Valor.Elementos.Select(empleado => empleado.PagoSemanalCalculado)
            .Should().Equal(1_500m, 950m, 1_600m);

        // Los campos propios de cada contrato solo viajan en la fila que corresponde.
        resultado.Valor.Elementos[0].SalarioSemanal.Should().Be(1_500m);
        resultado.Valor.Elementos[0].HorasTrabajadas.Should().BeNull();
        resultado.Valor.Elementos[1].HorasTrabajadas.Should().Be(45m);
        resultado.Valor.Elementos[1].SalarioSemanal.Should().BeNull();
    }

    /// <summary>
    /// Una paginación imposible se rechaza antes de llegar al motor de base de datos.
    /// </summary>
    /// <remarks>
    /// FluentValidation ya la rechaza en la frontera HTTP. Esta es la segunda red: cubre a
    /// cualquier otro llamador del servicio —una tarea programada, una prueba, un futuro
    /// controlador— que no pase por esa frontera.
    /// </remarks>
    [Fact]
    public async Task BuscarAsync_ConPaginaFueraDeRango_FallaSinConsultarAlRepositorio()
    {
        // Arrange
        FiltroEmpleados filtro = new() { Pagina = 0 };

        // Act
        Func<Task> busqueda = () => CrearServicio().BuscarAsync(filtro, CancellationToken.None);

        // Assert
        await busqueda.Should().ThrowAsync<ExcepcionValorFueraDeRango>();

        await _empleadoRepositorio.DidNotReceive().BuscarPaginaAsync(
            Arg.Any<FiltroBusquedaEmpleado>(),
            Arg.Any<Paginacion>(),
            Arg.Any<CancellationToken>());
    }
}
