import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

/**
 * Puerto del servidor de desarrollo.
 *
 * Va fijo y no "el primero que esté libre" porque el backend solo autoriza este origen en su
 * política de CORS (`Cors:OrigenesPermitidos` del appsettings). Si Vite se moviera de puerto
 * al encontrarlo ocupado, el navegador bloquearía todas las respuestas y el error que se ve
 * en consola no menciona el puerto: se pierde media hora buscando el problema donde no está.
 */
const PUERTO_DESARROLLO = 5173;

export default defineConfig({
  plugins: [react()],
  server: {
    port: PUERTO_DESARROLLO,
    strictPort: true,
  },
});
