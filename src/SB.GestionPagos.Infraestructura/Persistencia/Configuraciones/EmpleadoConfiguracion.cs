using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.GestionPagos.Aplicacion.Validaciones;
using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Infraestructura.Persistencia.Configuraciones;

/// <summary>
/// Mapeo de la raíz de la jerarquía: la tabla, la clave, el discriminador, los datos
/// personales comunes y los índices que sostienen el filtro del RF-03.
/// </summary>
/// <remarks>
/// Al configurar el tipo base en TPH se configura la tabla entera: las cuatro subclases
/// heredan tabla, clave e índices, y sus propias clases solo añaden sus columnas.
/// </remarks>
internal sealed class EmpleadoConfiguracion : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> constructorDeEntidad)
    {
        constructorDeEntidad.ToTable(EsquemaBaseDeDatos.TABLA_EMPLEADOS);

        constructorDeEntidad.HasKey(empleado => empleado.Id);

        // `TipoContrato` es una propiedad calculada del Dominio: cada subclase devuelve una
        // constante ("Empleado por Horas"). No tiene setter ni campo de respaldo, así que EF
        // no podría materializarla; y aunque pudiera, guardarla sería redundante con el
        // discriminador y podría quedar desincronizada de él.
        constructorDeEntidad.Ignore(empleado => empleado.TipoContrato);

        // -------------------------------------------------------------------
        // Herencia de tabla única (TPH)
        // -------------------------------------------------------------------
        constructorDeEntidad
            .HasDiscriminator<string>(EsquemaBaseDeDatos.COLUMNA_DISCRIMINADORA_TIPO_EMPLEADO)
            .HasValue<EmpleadoAsalariado>(EsquemaBaseDeDatos.DISCRIMINADOR_ASALARIADO)
            .HasValue<EmpleadoPorHoras>(EsquemaBaseDeDatos.DISCRIMINADOR_POR_HORAS)
            .HasValue<EmpleadoPorComision>(EsquemaBaseDeDatos.DISCRIMINADOR_POR_COMISION)
            .HasValue<EmpleadoAsalariadoPorComision>(
                EsquemaBaseDeDatos.DISCRIMINADOR_ASALARIADO_POR_COMISION);

        // Sin esta línea el discriminador sería nvarchar(max), que SQL Server no puede
        // indexar y almacena fuera de la fila.
        constructorDeEntidad
            .Property(EsquemaBaseDeDatos.COLUMNA_DISCRIMINADORA_TIPO_EMPLEADO)
            .HasMaxLength(EsquemaBaseDeDatos.LONGITUD_COLUMNA_DISCRIMINADORA)
            .IsRequired();

        // -------------------------------------------------------------------
        // Datos personales
        // -------------------------------------------------------------------
        constructorDeEntidad
            .Property(empleado => empleado.PrimerNombre)
            .HasMaxLength(LongitudMaxima.PRIMER_NOMBRE)
            .IsRequired();

        constructorDeEntidad
            .Property(empleado => empleado.ApellidoPaterno)
            .HasMaxLength(LongitudMaxima.APELLIDO_PATERNO)
            .IsRequired();

        constructorDeEntidad
            .Property(empleado => empleado.NumeroSeguroSocial)
            .HasMaxLength(LongitudMaxima.NUMERO_SEGURO_SOCIAL)
            .IsRequired();

        constructorDeEntidad
            .Property(empleado => empleado.Departamento)
            .HasMaxLength(LongitudMaxima.DEPARTAMENTO)
            .IsRequired();

        // El enum se guarda como entero, con los valores que el propio Dominio fijó de forma
        // explícita (Activo = 1, Inactivo = 2). La conversión se declara aunque sea la que EF
        // aplicaría por omisión: deja constancia de que el formato persistido es una decisión,
        // no un accidente, y de que renombrar un miembro no altera los datos guardados.
        constructorDeEntidad
            .Property(empleado => empleado.Estado)
            .HasConversion<int>()
            .IsRequired();

        constructorDeEntidad
            .Property(empleado => empleado.FechaCreacion)
            .IsRequired();

        // -------------------------------------------------------------------
        // Índices
        // -------------------------------------------------------------------

        // D-03: el número de seguro social identifica de forma única al empleado. Esta es la
        // garantía DURA. La comprobación previa de EmpleadoServicioBase da un mensaje
        // entendible en el caso normal, pero dos peticiones simultáneas pueden pasarla las
        // dos antes de que cualquiera guarde: solo el motor puede arbitrar esa carrera.
        constructorDeEntidad
            .HasIndex(empleado => empleado.NumeroSeguroSocial)
            .IsUnique()
            .HasDatabaseName(EsquemaBaseDeDatos.INDICE_EMPLEADOS_NUMERO_SEGURO_SOCIAL);

        // Los tres criterios del RF-03. Sin índice, cada filtro obliga a recorrer la tabla
        // entera; con él, el motor salta directamente a las filas que coinciden.
        constructorDeEntidad
            .HasIndex(empleado => empleado.Departamento)
            .HasDatabaseName(EsquemaBaseDeDatos.INDICE_EMPLEADOS_DEPARTAMENTO);

        constructorDeEntidad
            .HasIndex(empleado => empleado.Estado)
            .HasDatabaseName(EsquemaBaseDeDatos.INDICE_EMPLEADOS_ESTADO);

        // Este índice no sostiene un filtro sino el ORDEN. La paginación con OFFSET/FETCH
        // exige un ORDER BY, y el listado se ordena por apellido: sin índice, cada página
        // pedida obligaría al motor a ordenar la tabla completa antes de descartar filas.
        constructorDeEntidad
            .HasIndex(empleado => empleado.ApellidoPaterno)
            .HasDatabaseName(EsquemaBaseDeDatos.INDICE_EMPLEADOS_APELLIDO_PATERNO);
    }
}
