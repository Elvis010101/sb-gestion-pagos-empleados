/**
 * Declaración única de las rutas de la aplicación.
 *
 * Las direcciones NO se escriben como texto suelto en los componentes. Un `<Link to="/consulta">`
 * repetido en seis archivos es una dirección que el día que cambie se corrige en cinco sitios
 * y se olvida en el sexto, y ese enlace roto no lo detecta ningún compilador. Aquí sí: si se
 * renombra una constante, TypeScript señala todos los usos.
 */
export const Rutas = {
  IniciarSesion: '/acceso',
  Inicio: '/',
  ConsultaEmpleados: '/consulta',
  CrearRegistro: '/crear-registro',
  EditarEmpleado: '/empleados/:identificador/editar',
  EntidadesGubernamentales: '/entidades-gubernamentales',
  ReporteSemanal: '/reporte-semanal',
} as const;

/**
 * Construye la dirección concreta de edición a partir del identificador.
 *
 * La ruta con parámetro no se puede usar tal cual —navegar a `/empleados/:identificador/editar`
 * llevaría literalmente ahí—, y armarla con plantillas repartidas por el código reintroduce
 * justo el problema que resuelve el objeto de arriba.
 */
export function construirRutaEditarEmpleado(identificador: number): string {
  return Rutas.EditarEmpleado.replace(':identificador', String(identificador));
}

/**
 * Metadatos de una pantalla.
 */
export interface DefinicionDePagina {
  ruta: string;

  /** Rótulo del encabezado y, si corresponde, del ítem de navegación. */
  titulo: string;

  /** Si aparece en la barra lateral. Las pantallas a las que se llega desde otra, no. */
  apareceEnNavegacion: boolean;

  /**
   * Si solo tiene sentido para un administrador. La barra lateral la oculta al rol Usuario.
   * Esconderla es cortesía, no seguridad: quien escriba la dirección a mano igual choca con
   * el 403 del servidor, que es donde la autorización se decide de verdad.
   */
  requiereAdministrador: boolean;
}

/**
 * Catálogo de pantallas. Es la fuente de la que se alimentan DOS piezas: la navegación de la
 * barra lateral y el título del encabezado. Tenerlas leyendo de la misma tabla es lo que
 * evita que el menú diga "Consulta" y el encabezado "Listado de empleados".
 */
export const DEFINICIONES_DE_PAGINA: readonly DefinicionDePagina[] = [
  {
    ruta: Rutas.Inicio,
    titulo: 'Inicio',
    apareceEnNavegacion: true,
    requiereAdministrador: false,
  },
  {
    ruta: Rutas.ConsultaEmpleados,
    titulo: 'Consulta',
    apareceEnNavegacion: true,
    requiereAdministrador: false,
  },
  {
    ruta: Rutas.CrearRegistro,
    titulo: 'Crear registro',
    apareceEnNavegacion: true,
    requiereAdministrador: true,
  },
  {
    ruta: Rutas.EntidadesGubernamentales,
    titulo: 'Entidades gubernamentales',
    apareceEnNavegacion: true,
    requiereAdministrador: false,
  },
  {
    ruta: Rutas.ReporteSemanal,
    titulo: 'Reporte semanal',
    apareceEnNavegacion: true,
    requiereAdministrador: false,
  },
  {
    ruta: Rutas.EditarEmpleado,
    titulo: 'Editar empleado',
    apareceEnNavegacion: false,
    requiereAdministrador: true,
  },
];
