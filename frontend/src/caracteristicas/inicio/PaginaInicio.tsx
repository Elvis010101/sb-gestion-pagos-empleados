import { Link } from 'react-router-dom';

import { useSesion } from '../autenticacion/useSesion';
import { Tarjeta } from '../../diseno/Tarjeta';
import { Rutas } from '../../rutas/rutas';

import estilos from './PaginaInicio.module.css';

interface AccesoDirecto {
  ruta: string;
  titulo: string;
  descripcion: string;
  requiereAdministrador: boolean;
}

const ACCESOS_DIRECTOS: readonly AccesoDirecto[] = [
  {
    ruta: Rutas.ConsultaEmpleados,
    titulo: 'Consulta de empleados',
    descripcion: 'Buscar por nombre, departamento y estado, con el pago semanal calculado.',
    requiereAdministrador: false,
  },
  {
    ruta: Rutas.CrearRegistro,
    titulo: 'Crear registro',
    descripcion: 'Registrar un empleado de cualquiera de los cuatro tipos de contrato.',
    requiereAdministrador: true,
  },
  {
    ruta: Rutas.EntidadesGubernamentales,
    titulo: 'Entidades gubernamentales',
    descripcion: 'Mantenimiento del catálogo de entidades del Estado.',
    requiereAdministrador: false,
  },
  {
    ruta: Rutas.ReporteSemanal,
    titulo: 'Reporte semanal',
    descripcion: 'Nómina de la semana con el desglose del cálculo de cada empleado.',
    requiereAdministrador: false,
  },
];

export function PaginaInicio() {
  const { sesion, esAdministrador } = useSesion();

  const accesosVisibles = ACCESOS_DIRECTOS.filter(
    (acceso) => !acceso.requiereAdministrador || esAdministrador,
  );

  return (
    <Tarjeta
      titulo="Sistema de Gestión de Pagos"
      descripcion="Registro, consulta y cálculo de la nómina semanal de los empleados."
    >
      <p className={estilos.saludo}>
        Bienvenido, <strong>{sesion?.nombreUsuario}</strong>.
        {esAdministrador
          ? ' Su rol le permite registrar, editar y dar de baja registros.'
          : ' Su rol es de consulta: puede ver la información pero no modificarla.'}
      </p>

      <div className={estilos.accesos}>
        {accesosVisibles.map((acceso) => (
          <Link key={acceso.ruta} to={acceso.ruta} className={estilos.acceso}>
            <span className={estilos.tituloDelAcceso}>{acceso.titulo}</span>
            <span className={estilos.descripcionDelAcceso}>{acceso.descripcion}</span>
          </Link>
        ))}
      </div>
    </Tarjeta>
  );
}
