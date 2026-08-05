using System;
using System.Linq;
using System.Collections.Generic;
using MundialProde.Usuarios;
using MundialProde.Predicciones;

namespace MundialProde.Servicios
{
    public class UsuarioService
    {
        private readonly List<Usuario> _usuarios;
        private readonly PrediccionService _prediccionService;

        public UsuarioService(List<Usuario> usuarios, PrediccionService prediccionService)
        {
            _usuarios = usuarios;
            _prediccionService = prediccionService;
        }

        public void MenuUsuarios()
        {
            Console.WriteLine("1. Crear usuario");
            Console.WriteLine("2. Generar usuarios aleatorios (bots)");
            Console.WriteLine("3. Listar usuarios");
            Console.WriteLine("0. Volver");
            Console.Write("Opción: ");
            string op = Console.ReadLine();
            Console.WriteLine();

            switch (op)
            {
                case "1":
                    CrearUsuario();
                    break;

                case "2":
                    GenerarBots();
                    break;

                case "3":
                    ListarUsuariosConHistorial();
                    break;

                case "0":
                    break;

                default:
                    Console.WriteLine("Opción inválida.");
                    break;
            }
        }

        private void CrearUsuario()
        {
            Console.Write("Nombre del usuario (enter = cancelar): ");
            string nombre = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nombre)) { Console.WriteLine("Cancelado."); return; }
            _usuarios.Add(new Usuario(nombre));
            Console.WriteLine($"Usuario '{nombre}' creado.");
        }

        private void GenerarBots()
        {
            Console.Write("¿Cuántos bots generar? (enter = cancelar): ");
            string entradaBots = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(entradaBots)) { Console.WriteLine("Cancelado."); return; }
            if (int.TryParse(entradaBots, out int cant) && cant > 0)
            {
                var bots = GeneradorUsuarios.GenerarVarios(cant);
                _usuarios.AddRange(bots);
                foreach (var bot in bots)
                    _prediccionService.GenerarPrediccionesAutomaticas(bot);
                Console.WriteLine($"Se generaron {cant} bots, cada uno con sus predicciones cargadas.");
            }
            else Console.WriteLine("Cantidad inválida.");
        }

        // Lista los usuarios numerados y, ahí mismo, deja tipear el número
        // de uno para ver su historial de predicciones (enter = no ver ninguno).
        public void ListarUsuariosConHistorial()
        {
            if (_usuarios.Count == 0) { Console.WriteLine("Todavía no hay usuarios."); return; }

            for (int i = 0; i < _usuarios.Count; i++)
            {
                var u = _usuarios[i];
                Console.WriteLine($"{i + 1}. {u.Nombre,-22} {(u.EsBot ? "(bot)" : "     ")} | {u.Puntaje} pts | {u.Predicciones.Count} predicciones");
            }

            Console.Write("\nNúmero de usuario para ver su historial (enter para omitir): ");
            string entrada = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(entrada)) return;

            if (int.TryParse(entrada, out int idx) && idx >= 1 && idx <= _usuarios.Count)
                MostrarHistorialPredicciones(_usuarios[idx - 1]);
            else
                Console.WriteLine("Usuario inválido.");
        }

        // Muestra la lista de usuarios y devuelve el elegido. Devuelve null
        // si todavía no hay usuarios, si el usuario cancela (0 / enter) o si
        // ingresa una opción inválida. La usa el menú de predicciones para
        // saber para quién se está cargando el pronóstico.
        public Usuario SeleccionarUsuario()
        {
            if (_usuarios.Count == 0) { Console.WriteLine("Todavía no hay usuarios."); return null; }

            Console.WriteLine("Usuarios disponibles:");
            for (int i = 0; i < _usuarios.Count; i++)
                Console.WriteLine($"{i + 1}. {_usuarios[i].Nombre}");
            Console.WriteLine("0. Volver");
            Console.Write("Elegí un usuario: ");
            string entrada = Console.ReadLine();

            if (entrada == "0" || string.IsNullOrWhiteSpace(entrada)) return null;
            if (!int.TryParse(entrada, out int idx) || idx < 1 || idx > _usuarios.Count)
            {
                Console.WriteLine("Usuario inválido.");
                return null;
            }
            return _usuarios[idx - 1];
        }

        // Historial completo de predicciones de un usuario: qué predijo,
        // en qué estado quedó (Pendiente/Acertado/Fallado) y cuántos puntos
        // le dio cada una.
        public void MostrarHistorialPredicciones(Usuario usuario)
        {
            Console.WriteLine($"\n===== Historial de predicciones de {usuario.Nombre} =====");
            if (usuario.Predicciones.Count == 0)
            {
                Console.WriteLine("(todavía no cargó ninguna predicción)");
                return;
            }

            foreach (var prediccion in usuario.Predicciones)
            {
                Console.WriteLine($"- [{prediccion.Tipo}] {prediccion.Descripcion(),-40} | Estado: {prediccion.NombreEstado,-9} | Puntos: {prediccion.ObtenerPuntos()}");
            }

            int acertadas = usuario.Predicciones.Count(p => p.NombreEstado == "Acertado");
            int falladas = usuario.Predicciones.Count(p => p.NombreEstado == "Fallado");
            int pendientes = usuario.Predicciones.Count(p => p.NombreEstado == "Pendiente");
            Console.WriteLine($"\nTotal: {acertadas} acertadas, {falladas} falladas, {pendientes} pendientes | {usuario.Puntaje} pts");
        }
    }
}
