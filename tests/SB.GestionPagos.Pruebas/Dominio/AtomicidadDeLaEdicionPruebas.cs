using FluentAssertions;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Excepciones;
using SB.GestionPagos.Pruebas.Comunes;
using Xunit;

namespace SB.GestionPagos.Pruebas.Dominio;

/// <summary>
/// Una edición rechazada no deja la entidad a medio actualizar.
/// </summary>
/// <remarks>
/// Es la diferencia entre "el Dominio valida" y "el Dominio protege sus invariantes". Un
/// método que valida y asigna intercalado —valida el primer campo, lo asigna, valida el
/// segundo, revienta— deja el objeto en un estado que nadie pidió: con el sueldo nuevo y las
/// horas viejas. El llamador recibe su excepción y cree que no pasó nada.
///
/// Hoy ese estado corrupto no llega a persistirse, porque la excepción sale por el middleware
/// antes de que el servicio invoque a <c>ActualizarAsync</c>. Pero eso es una propiedad del
/// camino HTTP actual, no una garantía de la entidad: basta que alguien atrape la excepción
/// de dominio para reintentar, o que una futura operación edite dos veces dentro de la misma
/// unidad de trabajo, para que el objeto medio actualizado sí acabe en la base.
///
/// La regla que verifica este archivo es: VALIDAR TODO PRIMERO, ASIGNAR DESPUÉS. O la edición
/// entera surte efecto, o no surte ninguno.
/// </remarks>
public sealed class AtomicidadDeLaEdicionPruebas
{
    [Fact]
    public void EmpleadoPorHoras_AlRechazarLasHoras_NoAlteraElSueldoPorHora()
    {
        // Arrange
        EmpleadoPorHoras empleado = EmpleadoDePrueba.PorHoras(sueldoPorHora: 20m, horasTrabajadas: 40m);

        // Act
        // El sueldo que llega es válido; las horas, no. El sueldo se valida primero.
        Action edicion = () => empleado.ActualizarDatosDeContrato(sueldoPorHora: 99m, horasTrabajadas: -8m);

        // Assert
        edicion.Should().Throw<ExcepcionValorFueraDeRango>();

        empleado.SueldoPorHora.Should().Be(20m, "la edición completa se rechazó, no solo su última mitad");
        empleado.HorasTrabajadas.Should().Be(40m);
        empleado.CalcularPagoSemanal().Should().Be(800m);
    }

    [Fact]
    public void EmpleadoPorComision_AlRechazarLaTarifa_NoAlteraLasVentasBrutas()
    {
        // Arrange
        EmpleadoPorComision empleado =
            EmpleadoDePrueba.PorComision(ventasBrutas: 10_000m, tarifaComision: 0.10m);

        // Act
        Action edicion = () => empleado.ActualizarDatosDeContrato(ventasBrutas: 99_999m, tarifaComision: 2m);

        // Assert
        edicion.Should().Throw<ExcepcionValorFueraDeRango>();

        empleado.VentasBrutas.Should().Be(10_000m);
        empleado.TarifaComision.Should().Be(0.10m);
        empleado.CalcularPagoSemanal().Should().Be(1_000m);
    }

    [Fact]
    public void EmpleadoAsalariadoPorComision_AlRechazarElSalarioBase_NoAlteraVentasNiTarifa()
    {
        // Arrange
        EmpleadoAsalariadoPorComision empleado = EmpleadoDePrueba.AsalariadoPorComision(
            ventasBrutas: 10_000m,
            tarifaComision: 0.05m,
            salarioBase: 1_000m);

        // Act
        // Aquí fallan DOS de los tres campos que ya se habrían asignado: es el caso con más
        // margen para dejar basura si la validación y la asignación van intercaladas.
        Action edicion = () => empleado.ActualizarDatosDeContrato(
            ventasBrutas: 99_999m,
            tarifaComision: 0.99m,
            salarioBase: -1m);

        // Assert
        edicion.Should().Throw<ExcepcionValorFueraDeRango>()
            .Which.NombrePropiedad.Should().Be(nameof(EmpleadoAsalariadoPorComision.SalarioBase));

        empleado.VentasBrutas.Should().Be(10_000m);
        empleado.TarifaComision.Should().Be(0.05m);
        empleado.SalarioBase.Should().Be(1_000m);
        empleado.CalcularPagoSemanal().Should().Be(1_600m);
    }

    /// <summary>
    /// Los datos personales se editan igual en los cuatro tipos, así que basta probarlo en uno.
    /// </summary>
    /// <remarks>
    /// El departamento, que es el último de los cuatro campos, es el que se rechaza: los tres
    /// anteriores ya estarían asignados si el método no fuera atómico.
    /// </remarks>
    [Fact]
    public void Empleado_AlRechazarElDepartamento_NoAlteraNingunOtroDatoPersonal()
    {
        // Arrange
        EmpleadoAsalariado empleado = EmpleadoDePrueba.Asalariado(1_500m);

        // Act
        Action edicion = () => empleado.ActualizarDatosPersonales(
            primerNombre: "Luis",
            apellidoPaterno: "Pérez",
            numeroSeguroSocial: "001-0000000-1",
            departamento: "   ");

        // Assert
        edicion.Should().Throw<ExcepcionValorRequerido>()
            .Which.NombrePropiedad.Should().Be(nameof(Empleado.Departamento));

        empleado.PrimerNombre.Should().Be(EmpleadoDePrueba.PRIMER_NOMBRE);
        empleado.ApellidoPaterno.Should().Be(EmpleadoDePrueba.APELLIDO_PATERNO);
        empleado.NumeroSeguroSocial.Should().Be(EmpleadoDePrueba.NUMERO_SEGURO_SOCIAL);
        empleado.Departamento.Should().Be(EmpleadoDePrueba.DEPARTAMENTO);
    }

    /// <summary>
    /// El catálogo de entidades gubernamentales tiene el mismo contrato, y merece la misma
    /// garantía: su repositorio reescribe el archivo de texto plano completo, así que una
    /// entidad medio actualizada en memoria se convertiría en una línea corrupta en disco.
    /// </summary>
    [Fact]
    public void EntidadGubernamental_AlRechazarElSector_NoAlteraNingunOtroDato()
    {
        // Arrange
        EntidadGubernamental entidad = new(
            nombre: "Superintendencia de Bancos",
            categoria: "Organismo Regulador",
            poderDelEstado: "Ejecutivo",
            sector: "Financiero");

        // Act
        Action edicion = () => entidad.Actualizar(
            nombre: "Nombre alterado",
            categoria: "Categoría alterada",
            poderDelEstado: "Poder alterado",
            sector: "");

        // Assert
        edicion.Should().Throw<ExcepcionValorRequerido>()
            .Which.NombrePropiedad.Should().Be(nameof(EntidadGubernamental.Sector));

        entidad.Nombre.Should().Be("Superintendencia de Bancos");
        entidad.Categoria.Should().Be("Organismo Regulador");
        entidad.PoderDelEstado.Should().Be("Ejecutivo");
        entidad.Sector.Should().Be("Financiero");
    }
}
