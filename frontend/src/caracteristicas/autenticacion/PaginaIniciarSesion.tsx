import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { z } from 'zod';

import logoSuperintendencia from '../../activos/logo-superintendencia-de-bancos.png';
import { ErrorApi, traducirError } from '../../comunes/api/ErrorApi';
import { Boton } from '../../comunes/componentes/Boton';
import { CampoDeTexto } from '../../comunes/componentes/CampoDeTexto';
import { MensajeDeError } from '../../comunes/componentes/MensajeDeError';
import { Rutas } from '../../rutas/rutas';

import estilos from './PaginaIniciarSesion.module.css';
import { useSesion } from './useSesion';

/**
 * Esquema de validación del formulario.
 *
 * Valida solo la FORMA de lo que se escribe —que los campos no estén vacíos—, nunca si las
 * credenciales son correctas: eso solo lo sabe el servidor. La validación del navegador
 * existe para no gastar un viaje de red en un formulario obviamente incompleto y para dar
 * respuesta inmediata; nunca sustituye a la del backend, que es la única que un atacante no
 * puede saltarse.
 */
const esquemaInicioSesion = z.object({
  nombreUsuario: z.string().trim().min(1, 'Ingrese su nombre de usuario.'),
  contrasena: z.string().min(1, 'Ingrese su contraseña.'),
});

/**
 * El tipo del formulario se INFIERE del esquema en vez de declararse aparte. Así no pueden
 * separarse: agregar un campo al esquema lo agrega al tipo, y un campo del formulario que no
 * exista en el esquema deja de compilar.
 */
type FormularioInicioSesion = z.infer<typeof esquemaInicioSesion>;

export function PaginaIniciarSesion() {
  const { sesion, estaComprobandoSesion, iniciarSesion } = useSesion();
  const navegar = useNavigate();
  const ubicacion = useLocation();
  const [errorDeAcceso, establecerErrorDeAcceso] = useState<ErrorApi | null>(null);

  const {
    register: registrarCampo,
    handleSubmit: manejarEnvio,
    formState: { errors: erroresDeCampo, isSubmitting: estaEnviando },
  } = useForm<FormularioInicioSesion>({
    resolver: zodResolver(esquemaInicioSesion),
  });

  // Quien ya tiene sesión no debería poder volver al formulario de acceso: no hay nada que
  // hacer ahí y deja la aplicación en un estado confuso.
  if (!estaComprobandoSesion && sesion !== null) {
    return <Navigate to={Rutas.Inicio} replace />;
  }

  async function enviar(datos: FormularioInicioSesion): Promise<void> {
    establecerErrorDeAcceso(null);

    try {
      await iniciarSesion(datos);
      navegar(obtenerDestino(ubicacion.state), { replace: true });
    } catch (error: unknown) {
      // El error se muestra, no se propaga: un fallo de credenciales es un desenlace previsto
      // del formulario, no una excepción que deba tumbar la pantalla.
      establecerErrorDeAcceso(traducirError(error));
    }
  }

  return (
    <div className={estilos.pantalla}>
      <div className={estilos.tarjeta}>
        <img
          className={estilos.logo}
          src={logoSuperintendencia}
          alt="Superintendencia de Bancos de la República Dominicana"
        />

        <h1 className={estilos.titulo}>Gestión de Pagos</h1>
        <p className={estilos.descripcion}>Ingrese sus credenciales para continuar.</p>

        {/* `noValidate` desactiva los globos del navegador: sus mensajes salen en el idioma
            del navegador y no en el del sistema, y compiten con los de Zod. */}
        <form className={estilos.formulario} onSubmit={manejarEnvio(enviar)} noValidate>
          <CampoDeTexto
            etiqueta="Usuario"
            autoComplete="username"
            autoFocus
            mensajeDeError={erroresDeCampo.nombreUsuario?.message}
            {...registrarCampo('nombreUsuario')}
          />

          <CampoDeTexto
            etiqueta="Contraseña"
            type="password"
            autoComplete="current-password"
            mensajeDeError={erroresDeCampo.contrasena?.message}
            {...registrarCampo('contrasena')}
          />

          {errorDeAcceso !== null ? <MensajeDeError error={errorDeAcceso} /> : null}

          <Boton type="submit" anchoCompleto estaProcesando={estaEnviando}>
            Iniciar sesión
          </Boton>
        </form>
      </div>
    </div>
  );
}

/**
 * Recupera la pantalla que el usuario intentaba abrir antes de que la ruta protegida lo
 * mandara aquí.
 *
 * El estado de navegación llega sin tipar porque lo pudo poner cualquiera —incluso un enlace
 * externo—, así que se comprueba en vez de creerle. Además solo se aceptan rutas internas:
 * admitir cualquier texto permitiría que un enlace preparado enviara al usuario a otro sitio
 * después de autenticarse, que es la definición de una redirección abierta.
 */
function obtenerDestino(estadoDeNavegacion: unknown): string {
  if (typeof estadoDeNavegacion !== 'object' || estadoDeNavegacion === null) {
    return Rutas.Inicio;
  }

  const destino = (estadoDeNavegacion as { destino?: unknown }).destino;

  if (typeof destino !== 'string' || !destino.startsWith('/') || destino.startsWith('//')) {
    return Rutas.Inicio;
  }

  return destino;
}
