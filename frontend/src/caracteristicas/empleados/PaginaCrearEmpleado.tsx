import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { CampoDeSeleccion } from '../../comunes/componentes/CampoDeSeleccion';
import { EstadoVacio } from '../../comunes/componentes/EstadoVacio';
import { Tarjeta } from '../../diseno/Tarjeta';
import { Rutas } from '../../rutas/rutas';
import { useSesion } from '../autenticacion/useSesion';

import { FormularioEmpleado } from './FormularioEmpleado';
import estilos from './PaginaCrearEmpleado.module.css';
import {
  REGISTRO_TIPOS_EMPLEADO,
  construirValoresIniciales,
  type ValoresFormularioEmpleado,
} from './configuracionDeTiposDeEmpleado';
import { TIPOS_EMPLEADO, type TipoEmpleado } from './tipos';

const TIPO_PREDETERMINADO: TipoEmpleado = 'Asalariado';

/**
 * Las opciones del selector se derivan del registro, no se escriben a mano. Es lo que hace
 * que un tipo nuevo aparezca en la lista con solo darlo de alta en el registro.
 */
const OPCIONES_DE_TIPO = TIPOS_EMPLEADO.map((clave) => ({
  valor: clave,
  etiqueta: REGISTRO_TIPOS_EMPLEADO[clave].etiqueta,
}));

export function PaginaCrearEmpleado() {
  const { esAdministrador } = useSesion();
  const navegar = useNavigate();
  const [claveDeTipo, establecerClaveDeTipo] = useState<TipoEmpleado>(TIPO_PREDETERMINADO);

  const configuracion = REGISTRO_TIPOS_EMPLEADO[claveDeTipo];

  const valoresIniciales = useMemo(() => construirValoresIniciales(configuracion), [configuracion]);

  // Ocultar la pantalla al rol Usuario es cortesía, no seguridad: quien escriba la dirección
  // a mano llega igual, y ahí el que dice que no es el 403 del servidor.
  if (!esAdministrador) {
    return (
      <Tarjeta titulo="Crear registro">
        <EstadoVacio
          titulo="No tiene permiso para esta operación"
          descripcion="Su rol es de consulta. El registro de empleados corresponde al rol Administrador."
        />
      </Tarjeta>
    );
  }

  async function guardar(valores: ValoresFormularioEmpleado): Promise<void> {
    await configuracion.crear(valores);
    navegar(Rutas.ConsultaEmpleados);
  }

  return (
    <Tarjeta
      titulo="Registrar empleado"
      descripcion="Elija el tipo de contrato: el formulario ajusta sus campos a lo que ese tipo necesita."
    >
      <div className={estilos.selectorDeTipo}>
        <CampoDeSeleccion
          className={estilos.selector}
          etiqueta="Tipo de contrato"
          opciones={OPCIONES_DE_TIPO}
          value={claveDeTipo}
          onChange={(evento) => establecerClaveDeTipo(evento.target.value as TipoEmpleado)}
        />
        <span className={estilos.descripcionDelTipo}>{configuracion.descripcion}</span>
      </div>

      {/* La `key` fuerza a React a descartar el formulario y montar uno nuevo al cambiar de
          tipo. No es una optimización: sin ella React reutilizaría la misma instancia y
          react-hook-form conservaría los valores y los errores del tipo anterior, con campos
          registrados que ya no se dibujan. Aquí la `key` expresa identidad —"este es otro
          formulario"—, que es justamente para lo que existe. */}
      <FormularioEmpleado
        key={configuracion.clave}
        configuracion={configuracion}
        valoresIniciales={valoresIniciales}
        permiteEditarEstado={false}
        etiquetaDeGuardar="Registrar empleado"
        alGuardar={guardar}
        alCancelar={() => navegar(Rutas.ConsultaEmpleados)}
      />
    </Tarjeta>
  );
}
