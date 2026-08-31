import { useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import { EstadoVacio } from '../../comunes/componentes/EstadoVacio';
import { IndicadorDeCarga } from '../../comunes/componentes/IndicadorDeCarga';
import { MensajeDeError } from '../../comunes/componentes/MensajeDeError';
import { useConsultaApi } from '../../comunes/hooks/useConsultaApi';
import { Tarjeta } from '../../diseno/Tarjeta';
import { Rutas } from '../../rutas/rutas';
import { useSesion } from '../autenticacion/useSesion';

import { FormularioEmpleado } from './FormularioEmpleado';
import {
  REGISTRO_TIPOS_EMPLEADO,
  construirValoresDesdeEmpleado,
  type ValoresFormularioEmpleado,
} from './configuracionDeTiposDeEmpleado';
import { obtenerEmpleadoPorId } from './empleadosApi';
import { CLAVE_POR_ETIQUETA_TIPO_CONTRATO } from './tipos';

export function PaginaEditarEmpleado() {
  const { esAdministrador } = useSesion();
  const navegar = useNavigate();

  // El parámetro de ruta siempre llega como texto, incluso con la restricción `:int` del
  // backend: la URL no tiene tipos. Convertirlo aquí es el borde donde entra el dato.
  const { identificador: identificadorEnTexto } = useParams<{ identificador: string }>();
  const identificador = Number(identificadorEnTexto);

  const consultarEmpleado = useCallback(() => obtenerEmpleadoPorId(identificador), [identificador]);

  const { datos: empleado, estaCargando, error, recargar } = useConsultaApi(consultarEmpleado);

  if (!esAdministrador) {
    return (
      <Tarjeta titulo="Editar empleado">
        <EstadoVacio
          titulo="No tiene permiso para esta operación"
          descripcion="Su rol es de consulta. La edición corresponde al rol Administrador."
        />
      </Tarjeta>
    );
  }

  if (!Number.isInteger(identificador)) {
    return (
      <Tarjeta titulo="Editar empleado">
        <EstadoVacio
          titulo="Identificador no válido"
          descripcion="La dirección no corresponde a ningún empleado."
        />
      </Tarjeta>
    );
  }

  if (estaCargando) {
    return (
      <Tarjeta titulo="Editar empleado">
        <IndicadorDeCarga mensaje="Cargando el empleado…" />
      </Tarjeta>
    );
  }

  if (error !== null) {
    return (
      <Tarjeta titulo="Editar empleado">
        <MensajeDeError error={error} alReintentar={recargar} />
      </Tarjeta>
    );
  }

  if (empleado === null) {
    return (
      <Tarjeta titulo="Editar empleado">
        <EstadoVacio titulo="No se encontró el empleado" />
      </Tarjeta>
    );
  }

  // El servidor entrega el ROTULO del tipo de contrato; el registro se indexa por clave
  // interna. Esta traducción es el único punto donde ambos vocabularios se tocan.
  const clave = CLAVE_POR_ETIQUETA_TIPO_CONTRATO[empleado.tipoContrato];

  if (clave === undefined) {
    // Pasa si el backend agrega un quinto tipo y el frontend todavía no lo conoce. Se dice
    // con claridad en vez de romperse: un `undefined` propagado daría una pantalla en blanco
    // y un error de consola que nadie relaciona con la causa.
    return (
      <Tarjeta titulo="Editar empleado">
        <EstadoVacio
          titulo="Tipo de contrato no reconocido"
          descripcion={`El servidor reportó el tipo "${empleado.tipoContrato}", que esta versión de la interfaz no sabe editar.`}
        />
      </Tarjeta>
    );
  }

  const configuracion = REGISTRO_TIPOS_EMPLEADO[clave];

  async function guardar(valores: ValoresFormularioEmpleado): Promise<void> {
    await configuracion.actualizar(identificador, valores);
    navegar(Rutas.ConsultaEmpleados);
  }

  return (
    <Tarjeta
      titulo={`Editar a ${empleado.primerNombre} ${empleado.apellidoPaterno}`}
      descripcion={`${configuracion.etiqueta}. El pago semanal se recalcula en el servidor al guardar.`}
    >
      <FormularioEmpleado
        configuracion={configuracion}
        valoresIniciales={construirValoresDesdeEmpleado(configuracion, empleado)}
        permiteEditarEstado
        etiquetaDeGuardar="Guardar cambios"
        alGuardar={guardar}
        alCancelar={() => navegar(Rutas.ConsultaEmpleados)}
      />
    </Tarjeta>
  );
}
