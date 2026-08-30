using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Infraestructura.Persistencia.Configuraciones;

/// <summary>
/// Columnas propias del empleado por horas.
/// </summary>
internal sealed class EmpleadoPorHorasConfiguracion : IEntityTypeConfiguration<EmpleadoPorHoras>
{
    public void Configure(EntityTypeBuilder<EmpleadoPorHoras> constructorDeEntidad)
    {
        constructorDeEntidad
            .Property(empleado => empleado.SueldoPorHora)
            .HasPrecision(EsquemaBaseDeDatos.PRECISION_MONETARIA, EsquemaBaseDeDatos.ESCALA_MONETARIA);

        // Las horas NO son dinero. Van con su propia precisión porque el rango que el
        // Dominio admite (0 a 168) no necesita dieciocho dígitos.
        constructorDeEntidad
            .Property(empleado => empleado.HorasTrabajadas)
            .HasPrecision(
                EsquemaBaseDeDatos.PRECISION_HORAS_TRABAJADAS,
                EsquemaBaseDeDatos.ESCALA_HORAS_TRABAJADAS);
    }
}
