using Microsoft.EntityFrameworkCore;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Enumeraciones;

namespace SB.GestionPagos.Infraestructura.Persistencia.Semilla;

/// <summary>
/// Datos con los que la base nace poblada: dos usuarios y dieciséis empleados repartidos
/// entre los cuatro tipos de contrato.
/// </summary>
/// <remarks>
/// Se siembra con <c>HasData</c>, es decir, DENTRO de la migración. La alternativa —un
/// método que inserte al arrancar la aplicación— tiene dos problemas: el script SQL
/// entregable saldría vacío de datos, y habría que escribir a mano la lógica de "no
/// insertar dos veces". Con <c>HasData</c>, EF genera los INSERT en la migración y sabe
/// diferenciar altas, cambios y bajas entre una migración y la siguiente.
///
/// Los objetos son ANÓNIMOS y no entidades del Dominio. Es obligatorio: <c>Id</c> tiene
/// setter privado —lo asigna el motor— y los constructores del Dominio validan y normalizan.
/// <c>HasData</c> lee las propiedades por nombre sin construir la entidad, así que puede
/// fijar el identificador sin que el Dominio tenga que abrir una puerta para ello.
/// </remarks>
internal static class DatosSemilla
{
    // -----------------------------------------------------------------------
    // Usuarios
    // -----------------------------------------------------------------------

    private const int IDENTIFICADOR_USUARIO_ADMINISTRADOR = 1;

    private const int IDENTIFICADOR_USUARIO_CONSULTA = 2;

    private const string NOMBRE_USUARIO_ADMINISTRADOR = "admin";

    private const string NOMBRE_USUARIO_CONSULTA = "usuario";

    // Hashes BCrypt (factor de trabajo 12) de "Admin123!" y "Usuario123!", documentados en
    // el README. Van como literales y NO como una llamada a BCrypt.HashPassword porque el
    // algoritmo genera una sal aleatoria en cada invocación: el hash cambiaría en cada
    // compilación y EF detectaría una diferencia en el modelo, pidiendo una migración nueva
    // cada vez. Una semilla tiene que ser determinista.
    //
    // Son credenciales de demostración de una prueba técnica. En un sistema real, la cuenta
    // inicial se crea con una contraseña de un solo uso que hay que cambiar al primer acceso.
    private const string HASH_CONTRASENA_ADMINISTRADOR =
        "$2a$12$iA7cg2RZfZbgY6/shX4Py.bPfze.s9Pv.YDJ7sJHKa9ds/MBsXHVa";

    private const string HASH_CONTRASENA_CONSULTA =
        "$2a$12$RM7LaYWnevG9wPyWUyxQSe4C/3G3F0Ek7i.50I0zSurYpSIPlnWRa";

    // -----------------------------------------------------------------------
    // Departamentos usados por la semilla
    // -----------------------------------------------------------------------

    private const string DEPARTAMENTO_TECNOLOGIA = "Tecnología";

    private const string DEPARTAMENTO_FINANZAS = "Finanzas";

    private const string DEPARTAMENTO_RECURSOS_HUMANOS = "Recursos Humanos";

    private const string DEPARTAMENTO_OPERACIONES = "Operaciones";

    private const string DEPARTAMENTO_VENTAS = "Ventas";

    /// <summary>
    /// Fecha de alta de todos los registros sembrados.
    /// </summary>
    /// <remarks>
    /// Es un instante FIJO y no <c>DateTime.UtcNow</c> por la misma razón que los hashes son
    /// literales: si cambiara en cada compilación, EF vería el modelo modificado y exigiría
    /// una migración nueva cada vez que alguien ejecuta `dotnet build`.
    /// </remarks>
    private static readonly DateTime _fechaCreacionSemilla =
        new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    internal static void Aplicar(ModelBuilder constructorDeModelo)
    {
        SembrarUsuarios(constructorDeModelo);
        SembrarEmpleadosAsalariados(constructorDeModelo);
        SembrarEmpleadosPorHoras(constructorDeModelo);
        SembrarEmpleadosPorComision(constructorDeModelo);
        SembrarEmpleadosAsalariadosPorComision(constructorDeModelo);
    }

    private static void SembrarUsuarios(ModelBuilder constructorDeModelo)
    {
        constructorDeModelo.Entity<Usuario>().HasData(
            new
            {
                Id = IDENTIFICADOR_USUARIO_ADMINISTRADOR,
                NombreUsuario = NOMBRE_USUARIO_ADMINISTRADOR,
                HashContrasena = HASH_CONTRASENA_ADMINISTRADOR,
                Rol = RolUsuario.Administrador
            },
            new
            {
                Id = IDENTIFICADOR_USUARIO_CONSULTA,
                NombreUsuario = NOMBRE_USUARIO_CONSULTA,
                HashContrasena = HASH_CONTRASENA_CONSULTA,
                Rol = RolUsuario.Usuario
            });
    }

    // El discriminador NO se escribe en ningún objeto de abajo: EF lo deduce del tipo de
    // entidad al que se le encadena el HasData. Es la ventaja concreta de haber declarado
    // los valores del discriminador en el mapeo y no a mano.

    private static void SembrarEmpleadosAsalariados(ModelBuilder constructorDeModelo)
    {
        constructorDeModelo.Entity<EmpleadoAsalariado>().HasData(
            new
            {
                Id = 1,
                PrimerNombre = "Ana",
                ApellidoPaterno = "Reyes",
                NumeroSeguroSocial = "001-0000001-1",
                Departamento = DEPARTAMENTO_TECNOLOGIA,
                Estado = EstadoEmpleado.Activo,
                FechaCreacion = _fechaCreacionSemilla,
                SalarioSemanal = 32500.00m
            },
            new
            {
                Id = 2,
                PrimerNombre = "Carlos",
                ApellidoPaterno = "Mejía",
                NumeroSeguroSocial = "001-0000002-2",
                Departamento = DEPARTAMENTO_FINANZAS,
                Estado = EstadoEmpleado.Activo,
                FechaCreacion = _fechaCreacionSemilla,
                SalarioSemanal = 28000.00m
            },
            new
            {
                Id = 3,
                PrimerNombre = "Lucía",
                ApellidoPaterno = "Fernández",
                NumeroSeguroSocial = "001-0000003-3",
                Departamento = DEPARTAMENTO_RECURSOS_HUMANOS,
                Estado = EstadoEmpleado.Activo,
                FechaCreacion = _fechaCreacionSemilla,
                SalarioSemanal = 24500.00m
            },
            // Inactivo a propósito: sin al menos un empleado dado de baja no se puede
            // comprobar ni el filtro por estado del RF-03 ni la exclusión de la nómina (D-04).
            new
            {
                Id = 4,
                PrimerNombre = "Pedro",
                ApellidoPaterno = "Guzmán",
                NumeroSeguroSocial = "001-0000004-4",
                Departamento = DEPARTAMENTO_OPERACIONES,
                Estado = EstadoEmpleado.Inactivo,
                FechaCreacion = _fechaCreacionSemilla,
                SalarioSemanal = 21000.00m
            });
    }

    private static void SembrarEmpleadosPorHoras(ModelBuilder constructorDeModelo)
    {
        constructorDeModelo.Entity<EmpleadoPorHoras>().HasData(
            // Exactamente 40 horas: la frontera de la fórmula. Con este registro se ve en
            // datos reales que a las 40 horas todavía NO hay recargo.
            new
            {
                Id = 5,
                PrimerNombre = "María",
                ApellidoPaterno = "Santos",
                NumeroSeguroSocial = "001-0000005-5",
                Departamento = DEPARTAMENTO_OPERACIONES,
                Estado = EstadoEmpleado.Activo,
                FechaCreacion = _fechaCreacionSemilla,
                SueldoPorHora = 350.00m,
                HorasTrabajadas = 40.00m
            },
            // Con horas extra, y con media hora suelta para que se vea que la columna
            // admite jornadas parciales.
            new
            {
                Id = 6,
                PrimerNombre = "José",
                ApellidoPaterno = "Peña",
                NumeroSeguroSocial = "001-0000006-6",
                Departamento = DEPARTAMENTO_OPERACIONES,
                Estado = EstadoEmpleado.Activo,
                FechaCreacion = _fechaCreacionSemilla,
                SueldoPorHora = 420.00m,
                HorasTrabajadas = 46.50m
            },
            new
            {
                Id = 7,
                PrimerNombre = "Rosa",
                ApellidoPaterno = "Jiménez",
                NumeroSeguroSocial = "001-0000007-7",
                Departamento = DEPARTAMENTO_TECNOLOGIA,
                Estado = EstadoEmpleado.Activo,
                FechaCreacion = _fechaCreacionSemilla,
                SueldoPorHora = 550.00m,
                HorasTrabajadas = 38.00m
            },
            new
            {
                Id = 8,
                PrimerNombre = "Miguel",
                ApellidoPaterno = "Castillo",
                NumeroSeguroSocial = "001-0000008-8",
                Departamento = DEPARTAMENTO_OPERACIONES,
                Estado = EstadoEmpleado.Inactivo,
                FechaCreacion = _fechaCreacionSemilla,
                SueldoPorHora = 310.00m,
                HorasTrabajadas = 40.00m
            });
    }

    private static void SembrarEmpleadosPorComision(ModelBuilder constructorDeModelo)
    {
        constructorDeModelo.Entity<EmpleadoPorComision>().HasData(
            new
            {
                Id = 9,
                PrimerNombre = "Elena",
                ApellidoPaterno = "Vargas",
                NumeroSeguroSocial = "001-0000009-9",
                Departamento = DEPARTAMENTO_VENTAS,
                Estado = EstadoEmpleado.Activo,
                FechaCreacion = _fechaCreacionSemilla,
                VentasBrutas = 185000.00m,
                TarifaComision = 0.0800m
            },
            new
            {
                Id = 10,
                PrimerNombre = "Rafael",
                ApellidoPaterno = "Núñez",
                NumeroSeguroSocial = "001-0000010-0",
                Departamento = DEPARTAMENTO_VENTAS,
                Estado = EstadoEmpleado.Activo,
                FechaCreacion = _fechaCreacionSemilla,
                VentasBrutas = 240500.00m,
                TarifaComision = 0.0650m
            },
            new
            {
                Id = 11,
                PrimerNombre = "Carmen",
                ApellidoPaterno = "Ortiz",
                NumeroSeguroSocial = "001-0000011-1",
                Departamento = DEPARTAMENTO_VENTAS,
                Estado = EstadoEmpleado.Activo,
                FechaCreacion = _fechaCreacionSemilla,
                VentasBrutas = 98750.50m,
                TarifaComision = 0.1000m
            },
            // Tarifa de 7,5 %: es el registro que demuestra por qué TarifaComision NO puede
            // ser decimal(18,2). Con dos decimales se guardaría 0.08 y este empleado cobraría
            // 660 pesos de más cada semana.
            new
            {
                Id = 12,
                PrimerNombre = "Andrés",
                ApellidoPaterno = "Polanco",
                NumeroSeguroSocial = "001-0000012-2",
                Departamento = DEPARTAMENTO_VENTAS,
                Estado = EstadoEmpleado.Inactivo,
                FechaCreacion = _fechaCreacionSemilla,
                VentasBrutas = 132000.00m,
                TarifaComision = 0.0750m
            });
    }

    private static void SembrarEmpleadosAsalariadosPorComision(ModelBuilder constructorDeModelo)
    {
        constructorDeModelo.Entity<EmpleadoAsalariadoPorComision>().HasData(
            new
            {
                Id = 13,
                PrimerNombre = "Patricia",
                ApellidoPaterno = "Rosario",
                NumeroSeguroSocial = "001-0000013-3",
                Departamento = DEPARTAMENTO_VENTAS,
                Estado = EstadoEmpleado.Activo,
                FechaCreacion = _fechaCreacionSemilla,
                VentasBrutas = 210000.00m,
                TarifaComision = 0.0500m,
                SalarioBase = 18000.00m
            },
            new
            {
                Id = 14,
                PrimerNombre = "Luis",
                ApellidoPaterno = "Almonte",
                NumeroSeguroSocial = "001-0000014-4",
                Departamento = DEPARTAMENTO_VENTAS,
                Estado = EstadoEmpleado.Activo,
                FechaCreacion = _fechaCreacionSemilla,
                VentasBrutas = 156300.00m,
                TarifaComision = 0.0450m,
                SalarioBase = 20000.00m
            },
            new
            {
                Id = 15,
                PrimerNombre = "Sofía",
                ApellidoPaterno = "Batista",
                NumeroSeguroSocial = "001-0000015-5",
                Departamento = DEPARTAMENTO_VENTAS,
                Estado = EstadoEmpleado.Activo,
                FechaCreacion = _fechaCreacionSemilla,
                VentasBrutas = 87400.25m,
                TarifaComision = 0.0625m,
                SalarioBase = 16500.00m
            },
            new
            {
                Id = 16,
                PrimerNombre = "Ramón",
                ApellidoPaterno = "Encarnación",
                NumeroSeguroSocial = "001-0000016-6",
                Departamento = DEPARTAMENTO_VENTAS,
                Estado = EstadoEmpleado.Activo,
                FechaCreacion = _fechaCreacionSemilla,
                VentasBrutas = 305000.00m,
                TarifaComision = 0.0350m,
                SalarioBase = 25000.00m
            });
    }
}
