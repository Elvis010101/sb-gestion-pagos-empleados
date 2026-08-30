using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.GestionPagos.Aplicacion.Validaciones;
using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Infraestructura.Persistencia.Configuraciones;

/// <summary>
/// Mapeo del usuario que se autentica contra la API (RF-07).
/// </summary>
internal sealed class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> constructorDeEntidad)
    {
        constructorDeEntidad.ToTable(EsquemaBaseDeDatos.TABLA_USUARIOS);

        constructorDeEntidad.HasKey(usuario => usuario.Id);

        constructorDeEntidad
            .Property(usuario => usuario.NombreUsuario)
            .HasMaxLength(LongitudMaxima.NOMBRE_USUARIO)
            .IsRequired();

        // La columna guarda el hash, nunca la contraseña. El nombre de la propiedad del
        // Dominio ya lo dice, y la columna hereda ese nombre: quien abra la tabla con un
        // cliente de SQL no tiene que adivinar qué está viendo.
        constructorDeEntidad
            .Property(usuario => usuario.HashContrasena)
            .HasMaxLength(EsquemaBaseDeDatos.LONGITUD_HASH_CONTRASENA)
            .IsRequired();

        constructorDeEntidad
            .Property(usuario => usuario.Rol)
            .HasConversion<int>()
            .IsRequired();

        // Único porque el nombre de usuario es la credencial de entrada: dos filas con el
        // mismo nombre harían que el inicio de sesión dependiera de cuál devuelva el motor
        // primero. Además, este índice es el que hace que la consulta del login sea una
        // búsqueda directa y no un recorrido de la tabla.
        constructorDeEntidad
            .HasIndex(usuario => usuario.NombreUsuario)
            .IsUnique()
            .HasDatabaseName(EsquemaBaseDeDatos.INDICE_USUARIOS_NOMBRE_USUARIO);
    }
}
