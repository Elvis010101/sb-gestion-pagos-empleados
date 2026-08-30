using FluentAssertions;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.ObjetosDeValor;
using SB.GestionPagos.Pruebas.Comunes;
using Xunit;

namespace SB.GestionPagos.Pruebas.Dominio;

/// <summary>
/// Verifica las cuatro fórmulas de pago semanal de la p. 5 del PDF de la prueba (RF-04).
/// </summary>
/// <remarks>
/// Todos los valores esperados están escritos como literales, a mano. Es deliberado y va en
/// contra de la tentación de reutilizar las constantes del Dominio: una prueba que dijera
/// <c>salarioBase * EmpleadoAsalariadoPorComision.PORCENTAJE_BONIFICACION_SALARIO_BASE</c>
/// seguiría pasando si alguien cambiara esa constante de 0.10 a 0.25. Dejaría de verificar la
/// regla para limitarse a repetirla. El número literal ES la especificación del negocio.
///
/// No hay ningún <c>new EmpleadoServicio(...)</c> ni <c>DbContext</c> en este archivo: el
/// cálculo vive en el Dominio, que no referencia a nadie, así que probarlo no requiere
/// levantar nada. Esa facilidad no es suerte, es la consecuencia directa de la Onion.
/// </remarks>
public sealed class CalculoPagoSemanalPruebas
{
    /// <summary>
    /// Un empleado de cada tipo con su pago semanal conocido, para las pruebas que deben
    /// valer para los cuatro por igual.
    /// </summary>
    public static TheoryData<string, Empleado, decimal> NominaDeLosCuatroTipos() => new()
    {
        { "Empleado Asalariado", EmpleadoDePrueba.Asalariado(1_500m), 1_500m },
        { "Empleado por Horas", EmpleadoDePrueba.PorHoras(20m, 45m), 950m },
        { "Empleado por Comisión", EmpleadoDePrueba.PorComision(10_000m, 0.10m), 1_000m },
        {
            "Empleado Asalariado por Comisión",
            EmpleadoDePrueba.AsalariadoPorComision(10_000m, 0.05m, 1_000m),
            1_600m
        }
    };

    // ---------------------------------------------------------------------------------
    // Fórmula 1: Empleado Asalariado -> pago = salarioSemanal
    // ---------------------------------------------------------------------------------

    [Theory]
    [InlineData(1500, 1500)]
    [InlineData(2750.55, 2750.55)]
    [InlineData(0, 0)]
    public void EmpleadoAsalariado_CalcularPagoSemanal_DevuelveElSalarioFijo(
        decimal salarioSemanal,
        decimal pagoEsperado)
    {
        // Arrange
        EmpleadoAsalariado empleado = EmpleadoDePrueba.Asalariado(salarioSemanal);

        // Act
        decimal pagoSemanal = empleado.CalcularPagoSemanal();

        // Assert
        pagoSemanal.Should().Be(pagoEsperado);
    }

    // ---------------------------------------------------------------------------------
    // Fórmula 2: Empleado por Horas -> ordinarias + recargo sobre el excedente de 40
    // (la frontera de las 40 horas tiene su propio archivo: HorasExtraPruebas)
    // ---------------------------------------------------------------------------------

    [Theory]
    [InlineData(20, 35, 700)]
    [InlineData(18.75, 40, 750)]
    [InlineData(20, 45, 950)]
    [InlineData(0, 50, 0)]
    public void EmpleadoPorHoras_CalcularPagoSemanal_AplicaElRecargoSoloSobreElExcedente(
        decimal sueldoPorHora,
        decimal horasTrabajadas,
        decimal pagoEsperado)
    {
        // Arrange
        EmpleadoPorHoras empleado = EmpleadoDePrueba.PorHoras(sueldoPorHora, horasTrabajadas);

        // Act
        decimal pagoSemanal = empleado.CalcularPagoSemanal();

        // Assert
        pagoSemanal.Should().Be(pagoEsperado);
    }

    // ---------------------------------------------------------------------------------
    // Fórmula 3: Empleado por Comisión -> pago = ventasBrutas * tarifaComision
    // ---------------------------------------------------------------------------------

    [Theory]
    [InlineData(10000, 0.10, 1000)]
    [InlineData(8500.40, 0.05, 425.02)]
    [InlineData(25000, 0, 0)]
    [InlineData(0, 0.15, 0)]
    [InlineData(12000, 1, 12000)]
    public void EmpleadoPorComision_CalcularPagoSemanal_EsElPorcentajeDeLasVentas(
        decimal ventasBrutas,
        decimal tarifaComision,
        decimal pagoEsperado)
    {
        // Arrange
        EmpleadoPorComision empleado = EmpleadoDePrueba.PorComision(ventasBrutas, tarifaComision);

        // Act
        decimal pagoSemanal = empleado.CalcularPagoSemanal();

        // Assert
        pagoSemanal.Should().Be(pagoEsperado);
    }

    // ---------------------------------------------------------------------------------
    // Fórmula 4: Empleado Asalariado por Comisión
    //            pago = (ventas * tarifa) + salarioBase + (salarioBase * 0.10)
    // ---------------------------------------------------------------------------------

    [Theory]
    [InlineData(10000, 0.05, 1000, 1600)]
    [InlineData(0, 0.10, 2000, 2200)]
    [InlineData(50000, 0.02, 1500, 2650)]
    [InlineData(30000, 0.03, 0, 900)]
    public void EmpleadoAsalariadoPorComision_CalcularPagoSemanal_SumaComisionSalarioYBonificacion(
        decimal ventasBrutas,
        decimal tarifaComision,
        decimal salarioBase,
        decimal pagoEsperado)
    {
        // Arrange
        EmpleadoAsalariadoPorComision empleado =
            EmpleadoDePrueba.AsalariadoPorComision(ventasBrutas, tarifaComision, salarioBase);

        // Act
        decimal pagoSemanal = empleado.CalcularPagoSemanal();

        // Assert
        pagoSemanal.Should().Be(pagoEsperado);
    }

    // ---------------------------------------------------------------------------------
    // Invariantes que deben cumplirse para CUALQUIER tipo de empleado, presente o futuro
    // ---------------------------------------------------------------------------------

    /// <summary>
    /// El parámetro se declara como <see cref="Empleado"/>, la clase base abstracta, y no como
    /// el tipo concreto. Eso es lo que hace que esta prueba verifique el polimorfismo: si el
    /// cálculo se resolviera con un <c>switch</c> sobre el tipo en algún servicio en vez de con
    /// despacho dinámico, aquí no habría forma de obtener el número correcto.
    /// </summary>
    [Theory]
    [MemberData(nameof(NominaDeLosCuatroTipos))]
    public void CalcularPagoSemanal_InvocadoSobreLaClaseBase_DespachaAlTipoConcreto(
        string tipoContratoEsperado,
        Empleado empleado,
        decimal pagoEsperado)
    {
        // Act
        decimal pagoSemanal = empleado.CalcularPagoSemanal();

        // Assert
        empleado.TipoContrato.Should().Be(tipoContratoEsperado);
        pagoSemanal.Should().Be(pagoEsperado);
    }

    /// <summary>
    /// El total del reporte y su desglose no pueden contradecirse: el RF-06 muestra los dos
    /// juntos, y un usuario que sume las líneas a mano tiene que llegar al total impreso.
    /// </summary>
    [Theory]
    [MemberData(nameof(NominaDeLosCuatroTipos))]
    public void CalcularDesglosePagoSemanal_ParaCualquierTipo_SuTotalEsLaSumaDeSusLineas(
        string tipoContratoEsperado,
        Empleado empleado,
        decimal pagoEsperado)
    {
        // El nombre del tipo no participa de esta aserción; lo recibe porque comparte la
        // fuente de datos con la prueba anterior. Se descarta de forma explícita.
        _ = tipoContratoEsperado;

        // Act
        ResultadoPago desglose = empleado.CalcularDesglosePagoSemanal();

        // Assert
        desglose.Lineas.Should().NotBeEmpty();
        desglose.Lineas.Sum(linea => linea.Monto).Should().Be(pagoEsperado);
        desglose.Total.Should().Be(empleado.CalcularPagoSemanal());
    }

    /// <summary>
    /// El desglose no es un adorno: es lo que el reporte semanal imprime concepto por concepto.
    /// </summary>
    /// <remarks>
    /// Se comparan las líneas completas —concepto y monto— aprovechando que
    /// <c>LineaCalculo</c> es un <c>record</c>: su igualdad es estructural, así que basta una
    /// aserción para verificar el desglose entero, en orden.
    /// </remarks>
    [Fact]
    public void EmpleadoAsalariadoPorComision_Desglose_SeparaComisionSalarioYBonificacion()
    {
        // Arrange
        EmpleadoAsalariadoPorComision empleado =
            EmpleadoDePrueba.AsalariadoPorComision(ventasBrutas: 10_000m, tarifaComision: 0.05m, salarioBase: 1_000m);

        // Act
        ResultadoPago desglose = empleado.CalcularDesglosePagoSemanal();

        // Assert
        desglose.Lineas.Should().Equal(
            new LineaCalculo("Comisión sobre ventas brutas", 500m),
            new LineaCalculo("Salario base", 1_000m),
            new LineaCalculo("Bonificación sobre el salario base", 100m));
    }
}
