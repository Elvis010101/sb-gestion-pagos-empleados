using FluentAssertions;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.ObjetosDeValor;
using SB.GestionPagos.Pruebas.Comunes;
using Xunit;

namespace SB.GestionPagos.Pruebas.Dominio;

/// <summary>
/// La frontera de las 40 horas: el caso borde central de esta prueba técnica.
/// </summary>
/// <remarks>
/// La fórmula dice "si horasTrabajadas &gt; 40". El error clásico es escribir <c>&gt;=</c>, y
/// no lo detecta ninguna prueba con valores redondos: con 35 horas y con 45 horas, ambas
/// versiones dan el mismo resultado. Solo falla exactamente en 40, y solo por 10 pesos sobre
/// un sueldo de 20 la hora. Es el tipo de defecto que llega a producción y aparece meses
/// después como un descuadre de nómina que nadie sabe explicar.
///
/// Por eso este archivo existe aparte, y por eso ataca la frontera desde los dos lados
/// (39.5 y 40.5) además de justo encima de ella (40).
/// </remarks>
public sealed class HorasExtraPruebas
{
    private const decimal SUELDO_POR_HORA = 20m;

    /// <summary>
    /// Los tres valores que pide la prueba, más los extremos del rango admitido.
    /// </summary>
    /// <remarks>
    /// Con 40 horas: 20 × 40 = 800, SIN recargo.
    /// Con 41 horas: 800 + (20 × 1.5 × 1) = 830.
    /// Con 168 horas —la semana completa—: 800 + (20 × 1.5 × 128) = 4.640.
    /// </remarks>
    [Theory]
    [InlineData(39.5, 790)]
    [InlineData(40, 800)]
    [InlineData(41, 830)]
    [InlineData(40.5, 815)]
    [InlineData(0, 0)]
    [InlineData(168, 4640)]
    public void CalcularPagoSemanal_AlrededorDeLasCuarentaHoras_AplicaLaFormulaCorrecta(
        decimal horasTrabajadas,
        decimal pagoEsperado)
    {
        // Arrange
        EmpleadoPorHoras empleado = EmpleadoDePrueba.PorHoras(SUELDO_POR_HORA, horasTrabajadas);

        // Act
        decimal pagoSemanal = empleado.CalcularPagoSemanal();

        // Assert
        pagoSemanal.Should().Be(pagoEsperado);
    }

    /// <summary>
    /// Con exactamente 40 horas el desglose debe traer UNA sola línea.
    /// </summary>
    /// <remarks>
    /// Esta es la prueba que distingue de verdad <c>&gt;</c> de <c>&gt;=</c>. Comprobar solo el
    /// total dejaría pasar una implementación que calculara una línea de horas extra de monto
    /// cero: el total saldría bien, pero el reporte del RF-06 imprimiría un renglón
    /// "Horas extra: 0.00" que confundiría a quien lo lea.
    /// </remarks>
    [Fact]
    public void CalcularDesglose_ConExactamenteCuarentaHoras_NoGeneraLineaDeHorasExtra()
    {
        // Arrange
        EmpleadoPorHoras empleado = EmpleadoDePrueba.PorHoras(SUELDO_POR_HORA, horasTrabajadas: 40m);

        // Act
        ResultadoPago desglose = empleado.CalcularDesglosePagoSemanal();

        // Assert
        desglose.Lineas.Should().ContainSingle();
        desglose.Lineas.Should().Equal(new LineaCalculo("Horas ordinarias", 800m));
    }

    /// <summary>
    /// Media hora por encima de la frontera ya se paga con recargo.
    /// </summary>
    [Fact]
    public void CalcularDesglose_ConCuarentaHorasYMedia_SeparaOrdinariasDeExtra()
    {
        // Arrange
        EmpleadoPorHoras empleado = EmpleadoDePrueba.PorHoras(SUELDO_POR_HORA, horasTrabajadas: 40.5m);

        // Act
        ResultadoPago desglose = empleado.CalcularDesglosePagoSemanal();

        // Assert
        desglose.Lineas.Should().Equal(
            new LineaCalculo("Horas ordinarias", 800m),
            new LineaCalculo("Horas extra", 15m));
    }

    /// <summary>
    /// Comprueba la frontera por su efecto marginal en lugar de por el total.
    /// </summary>
    /// <remarks>
    /// La hora número 40 vale el sueldo ordinario; la número 41 vale una vez y media. Escrito
    /// así, el test expresa la regla de negocio tal como la enunciaría un usuario —"a partir de
    /// la hora 41 me pagan hora y media"— y no depende de ningún total intermedio. Es la
    /// formulación que mejor resiste que mañana cambie el sueldo por hora del ejemplo.
    /// </remarks>
    [Fact]
    public void CalcularPagoSemanal_LaHoraCuarentaYUno_ValeUnaVezYMediaLaHoraOrdinaria()
    {
        // Arrange
        decimal pagoCon39Horas = EmpleadoDePrueba.PorHoras(SUELDO_POR_HORA, 39m).CalcularPagoSemanal();
        decimal pagoCon40Horas = EmpleadoDePrueba.PorHoras(SUELDO_POR_HORA, 40m).CalcularPagoSemanal();
        decimal pagoCon41Horas = EmpleadoDePrueba.PorHoras(SUELDO_POR_HORA, 41m).CalcularPagoSemanal();

        // Act
        decimal valorDeLaHora40 = pagoCon40Horas - pagoCon39Horas;
        decimal valorDeLaHora41 = pagoCon41Horas - pagoCon40Horas;

        // Assert
        valorDeLaHora40.Should().Be(20m, "la hora 40 todavía es ordinaria");
        valorDeLaHora41.Should().Be(30m, "la hora 41 se paga a 1.5 veces el sueldo por hora");
    }
}
