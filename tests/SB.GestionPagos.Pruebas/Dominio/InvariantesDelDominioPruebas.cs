using FluentAssertions;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Excepciones;
using SB.GestionPagos.Dominio.ObjetosDeValor;
using SB.GestionPagos.Dominio.Repositorios;
using SB.GestionPagos.Pruebas.Comunes;
using Xunit;

namespace SB.GestionPagos.Pruebas.Dominio;

/// <summary>
/// Verifica que el Dominio se niegue a construir empleados imposibles.
/// </summary>
/// <remarks>
/// La pregunta que responde este archivo es "¿puede existir en memoria un empleado con
/// −5 horas trabajadas?". La respuesta tiene que ser no, y no porque FluentValidation lo
/// rechace en la frontera HTTP —eso protege solo el camino que entra por la red—, sino
/// porque el constructor de la entidad lo impide. Un empleado inválido no llega a existir.
///
/// Cada prueba comprueba además el NOMBRE de la propiedad que viaja en la excepción: es el
/// dato con el que la capa Api señala el campo exacto en la respuesta de error, así que
/// forma parte del contrato y no del mensaje decorativo.
/// </remarks>
public sealed class InvariantesDelDominioPruebas
{
    private const string TEXTO_VALIDO = "Valor";

    // ---------------------------------------------------------------------------------
    // Rangos numéricos
    // ---------------------------------------------------------------------------------

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.5)]
    [InlineData(-1000)]
    public void EmpleadoPorHoras_ConHorasNegativas_RechazaLaConstruccion(decimal horasTrabajadas)
    {
        // Arrange
        Action alta = () => EmpleadoDePrueba.PorHoras(sueldoPorHora: 20m, horasTrabajadas: horasTrabajadas);

        // Act + Assert
        alta.Should().Throw<ExcepcionValorFueraDeRango>()
            .Which.NombrePropiedad.Should().Be(nameof(EmpleadoPorHoras.HorasTrabajadas));
    }

    /// <summary>
    /// Una semana tiene 168 horas. Registrar 200 no es un empleado muy trabajador: es un
    /// error de captura, y conviene que muera aquí y no en el reporte de nómina.
    /// </summary>
    [Theory]
    [InlineData(168.01)]
    [InlineData(200)]
    public void EmpleadoPorHoras_ConMasHorasDeLasQueTieneLaSemana_RechazaLaConstruccion(
        decimal horasTrabajadas)
    {
        // Arrange
        Action alta = () => EmpleadoDePrueba.PorHoras(sueldoPorHora: 20m, horasTrabajadas: horasTrabajadas);

        // Act + Assert
        alta.Should().Throw<ExcepcionValorFueraDeRango>()
            .Which.NombrePropiedad.Should().Be(nameof(EmpleadoPorHoras.HorasTrabajadas));
    }

    /// <summary>
    /// Los extremos del rango SÍ son válidos: el rango es inclusivo en ambos lados.
    /// </summary>
    /// <remarks>
    /// Sin esta prueba, alguien podría "arreglar" las dos anteriores cambiando la validación a
    /// un rango exclusivo y dejar de admitir a quien no trabajó ninguna hora esa semana.
    /// Probar solo lo que debe fallar deja libre la mitad del comportamiento.
    /// </remarks>
    [Theory]
    [InlineData(0)]
    [InlineData(168)]
    public void EmpleadoPorHoras_EnLosExtremosDelRango_AceptaLaConstruccion(decimal horasTrabajadas)
    {
        // Arrange
        Action alta = () => EmpleadoDePrueba.PorHoras(sueldoPorHora: 20m, horasTrabajadas: horasTrabajadas);

        // Act + Assert
        alta.Should().NotThrow();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(1.01)]
    [InlineData(2)]
    public void EmpleadoPorComision_ConTarifaFueraDelRango_RechazaLaConstruccion(decimal tarifaComision)
    {
        // Arrange
        Action alta = () => EmpleadoDePrueba.PorComision(ventasBrutas: 10_000m, tarifaComision: tarifaComision);

        // Act + Assert
        alta.Should().Throw<ExcepcionValorFueraDeRango>()
            .Which.NombrePropiedad.Should().Be(nameof(EmpleadoPorComision.TarifaComision));
    }

    /// <summary>
    /// 0 % y 100 % son tarifas admitidas: la fracción va de 0 a 1, ambos incluidos.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void EmpleadoPorComision_EnLosExtremosDeLaTarifa_AceptaLaConstruccion(decimal tarifaComision)
    {
        // Arrange
        Action alta = () => EmpleadoDePrueba.PorComision(ventasBrutas: 10_000m, tarifaComision: tarifaComision);

        // Act + Assert
        alta.Should().NotThrow();
    }

    /// <summary>
    /// Ningún importe monetario del sistema puede ser negativo, en ninguno de los cuatro tipos.
    /// </summary>
    /// <remarks>
    /// Se agrupan en una sola prueba parametrizada porque la regla es una sola —"no negativo"—
    /// aplicada a cinco propiedades distintas. Escribirla cinco veces no verificaría nada más.
    /// </remarks>
    public static TheoryData<string, Action> ImportesNegativos() => new()
    {
        {
            nameof(EmpleadoAsalariado.SalarioSemanal),
            () => EmpleadoDePrueba.Asalariado(salarioSemanal: -1m)
        },
        {
            nameof(EmpleadoPorHoras.SueldoPorHora),
            () => EmpleadoDePrueba.PorHoras(sueldoPorHora: -1m, horasTrabajadas: 40m)
        },
        {
            nameof(EmpleadoPorComision.VentasBrutas),
            () => EmpleadoDePrueba.PorComision(ventasBrutas: -1m, tarifaComision: 0.10m)
        },
        {
            nameof(EmpleadoAsalariadoPorComision.SalarioBase),
            () => EmpleadoDePrueba.AsalariadoPorComision(
                ventasBrutas: 10_000m,
                tarifaComision: 0.05m,
                salarioBase: -1m)
        }
    };

    [Theory]
    [MemberData(nameof(ImportesNegativos))]
    public void Empleado_ConUnImporteNegativo_RechazaLaConstruccion(string propiedadEsperada, Action alta)
    {
        // Act + Assert
        alta.Should().Throw<ExcepcionValorFueraDeRango>()
            .Which.NombrePropiedad.Should().Be(propiedadEsperada);
    }

    // ---------------------------------------------------------------------------------
    // Datos personales obligatorios
    // ---------------------------------------------------------------------------------

    /// <summary>
    /// Nulo, cadena vacía y solo espacios significan lo mismo para el negocio: no hay dato.
    /// </summary>
    public static TheoryData<string?, string?, string?, string?, string> DatosPersonalesAusentes() => new()
    {
        { null, TEXTO_VALIDO, TEXTO_VALIDO, TEXTO_VALIDO, nameof(Empleado.PrimerNombre) },
        { "", TEXTO_VALIDO, TEXTO_VALIDO, TEXTO_VALIDO, nameof(Empleado.PrimerNombre) },
        { "   ", TEXTO_VALIDO, TEXTO_VALIDO, TEXTO_VALIDO, nameof(Empleado.PrimerNombre) },
        { TEXTO_VALIDO, null, TEXTO_VALIDO, TEXTO_VALIDO, nameof(Empleado.ApellidoPaterno) },
        { TEXTO_VALIDO, "  ", TEXTO_VALIDO, TEXTO_VALIDO, nameof(Empleado.ApellidoPaterno) },
        { TEXTO_VALIDO, TEXTO_VALIDO, null, TEXTO_VALIDO, nameof(Empleado.NumeroSeguroSocial) },
        { TEXTO_VALIDO, TEXTO_VALIDO, "", TEXTO_VALIDO, nameof(Empleado.NumeroSeguroSocial) },
        { TEXTO_VALIDO, TEXTO_VALIDO, TEXTO_VALIDO, null, nameof(Empleado.Departamento) },
        { TEXTO_VALIDO, TEXTO_VALIDO, TEXTO_VALIDO, "\t", nameof(Empleado.Departamento) }
    };

    [Theory]
    [MemberData(nameof(DatosPersonalesAusentes))]
    public void Empleado_SinUnDatoPersonalObligatorio_RechazaLaConstruccion(
        string? primerNombre,
        string? apellidoPaterno,
        string? numeroSeguroSocial,
        string? departamento,
        string propiedadEsperada)
    {
        // Arrange
        // El `!` silencia la advertencia de nulabilidad a propósito: esta prueba existe
        // justamente para verificar qué hace el Dominio cuando alguien ignora el contrato de
        // tipos —por ejemplo, un JSON deserializado sin el campo—.
        Action alta = () => EmpleadoDePrueba.Asalariado(
            salarioSemanal: 1_000m,
            primerNombre: primerNombre!,
            apellidoPaterno: apellidoPaterno!,
            numeroSeguroSocial: numeroSeguroSocial!,
            departamento: departamento!);

        // Act + Assert
        alta.Should().Throw<ExcepcionValorRequerido>()
            .Which.NombrePropiedad.Should().Be(propiedadEsperada);
    }

    /// <summary>
    /// El Dominio normaliza los textos al asignarlos, para que " Ana " y "Ana" no acaben
    /// siendo dos empleados distintos en la base.
    /// </summary>
    [Fact]
    public void Empleado_ConEspaciosSobrantes_GuardaElTextoRecortado()
    {
        // Arrange + Act
        EmpleadoAsalariado empleado = EmpleadoDePrueba.Asalariado(
            salarioSemanal: 1_000m,
            primerNombre: "  Ana  ",
            apellidoPaterno: "Rodríguez ",
            numeroSeguroSocial: " 402-1234567-8 ",
            departamento: " Tecnología");

        // Assert
        empleado.PrimerNombre.Should().Be("Ana");
        empleado.ApellidoPaterno.Should().Be("Rodríguez");
        empleado.NumeroSeguroSocial.Should().Be("402-1234567-8");
        empleado.Departamento.Should().Be("Tecnología");
    }

    // ---------------------------------------------------------------------------------
    // Las mismas reglas en la EDICIÓN, no solo en el alta
    // ---------------------------------------------------------------------------------

    /// <summary>
    /// Un empleado válido no puede volverse inválido editándolo (RF-05).
    /// </summary>
    /// <remarks>
    /// Es un agujero clásico: se validan los constructores, se deja el <c>setter</c> de la
    /// edición sin validar, y el sistema termina admitiendo por la puerta de atrás justo lo
    /// que rechaza por la de entrada.
    /// </remarks>
    [Fact]
    public void EmpleadoPorHoras_AlEditarConHorasInvalidas_RechazaElCambio()
    {
        // Arrange
        EmpleadoPorHoras empleado = EmpleadoDePrueba.PorHoras(sueldoPorHora: 20m, horasTrabajadas: 40m);

        // Act
        Action edicion = () => empleado.ActualizarDatosDeContrato(sueldoPorHora: 20m, horasTrabajadas: -8m);

        // Assert
        edicion.Should().Throw<ExcepcionValorFueraDeRango>()
            .Which.NombrePropiedad.Should().Be(nameof(EmpleadoPorHoras.HorasTrabajadas));
    }

    [Fact]
    public void Empleado_AlEditarSinApellido_RechazaElCambio()
    {
        // Arrange
        EmpleadoAsalariado empleado = EmpleadoDePrueba.Asalariado(salarioSemanal: 1_000m);

        // Act
        Action edicion = () => empleado.ActualizarDatosPersonales(
            primerNombre: TEXTO_VALIDO,
            apellidoPaterno: "   ",
            numeroSeguroSocial: TEXTO_VALIDO,
            departamento: TEXTO_VALIDO);

        // Assert
        edicion.Should().Throw<ExcepcionValorRequerido>()
            .Which.NombrePropiedad.Should().Be(nameof(Empleado.ApellidoPaterno));
    }

    // ---------------------------------------------------------------------------------
    // Invariantes de los objetos de valor del Dominio
    // ---------------------------------------------------------------------------------

    /// <summary>
    /// Un pago sin ningún concepto no es un pago de cero: es un cálculo que no se hizo.
    /// </summary>
    [Fact]
    public void ResultadoPago_SinNingunaLinea_RechazaLaConstruccion()
    {
        // Arrange
        Action construccion = () => _ = new ResultadoPago();

        // Act + Assert
        construccion.Should().Throw<ExcepcionValorRequerido>();
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void Paginacion_ConValoresFueraDeRango_RechazaLaConstruccion(int pagina, int tamanoPagina)
    {
        // Arrange
        Action construccion = () => _ = new Paginacion(pagina, tamanoPagina);

        // Act + Assert
        construccion.Should().Throw<ExcepcionValorFueraDeRango>();
    }

    /// <summary>
    /// El desplazamiento se calcula una sola vez y en el Dominio, no en cada repositorio.
    /// </summary>
    [Theory]
    [InlineData(1, 20, 0)]
    [InlineData(2, 20, 20)]
    [InlineData(5, 10, 40)]
    public void Paginacion_RegistrosOmitidos_CuentaLasPaginasDesdeUno(
        int pagina,
        int tamanoPagina,
        int registrosOmitidosEsperados)
    {
        // Arrange
        Paginacion paginacion = new(pagina, tamanoPagina);

        // Act + Assert
        paginacion.RegistrosOmitidos.Should().Be(registrosOmitidosEsperados);
    }

    /// <summary>
    /// Toda excepción del Dominio hereda de <see cref="ExcepcionDominio"/>.
    /// </summary>
    /// <remarks>
    /// De esto depende el middleware de la capa Api: atrapa la clase raíz UNA sola vez para
    /// traducirla a HTTP 400, en lugar de enumerar cada excepción concreta. Si mañana alguien
    /// agrega una excepción de negocio que herede directamente de <c>Exception</c>, el
    /// middleware la clasificaría como error 500 y el usuario vería "error interno" ante un
    /// dato mal escrito. Esta prueba es la que avisa.
    /// </remarks>
    [Fact]
    public void ExcepcionesDelDominio_TodasHeredanDeLaRaizComun()
    {
        // Arrange
        Action horasNegativas = () => EmpleadoDePrueba.PorHoras(20m, -1m);
        Action nombreVacio = () => EmpleadoDePrueba.Asalariado(1_000m, primerNombre: "");

        // Act + Assert
        horasNegativas.Should().Throw<ExcepcionDominio>();
        nombreVacio.Should().Throw<ExcepcionDominio>();
    }
}
