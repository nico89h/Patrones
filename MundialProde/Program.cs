using System;
using System.Collections.Generic;
using MundialProde.Models;
using MundialProde.Usuarios;
using MundialProde.Servicios;

namespace MundialProde
{
    public class Program
    {
        private static CopaMundo copa;
        private static Ranking ranking;
        private static TorneoService torneoService;
        private static UsuarioService usuarioService;
        private static PrediccionService prediccionService;
        private static SimulacionService simulacionService;

        public static void Main(string[] args)
        {
            try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { }

            Console.WriteLine("==================================================");
            Console.WriteLine("   SIMULADOR DE MUNDIAL + PRODE DE PREDICCIONES   ");
            Console.WriteLine("==================================================");

            IniciarServicios();

            bool salir = false;
            while (!salir)
            {
                MostrarMenu();
                string opcion = Console.ReadLine();
                Console.WriteLine();
                switch (opcion)
                {
                    case "1": torneoService.Copa.Mostrar(); break;
                    case "2": usuarioService.MenuUsuarios(); break;
                    case "3": MenuPredicciones(); break;
                    case "4": simulacionService.MenuElegirEstrategia(); break;
                    case "5": simulacionService.MenuSimulacion(); break;
                    case "6": VerRanking(); break;
                    case "0": salir = true; break;
                    default: Console.WriteLine("Opción inválida."); break;
                }
            }

            Console.WriteLine("\n¡Hasta la próxima!");
        }

        private static void IniciarServicios()
        {
            Console.Write("\nNombre de la Copa del Mundo a crear (enter = 'Copa Mundial 2026'): ");
            string nombreCopa = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nombreCopa)) nombreCopa = "Copa Mundial 2026";

            copa = new CopaMundo(nombreCopa);
            var usuarios = new List<Usuario>();
            ranking = new Ranking(usuarios, copa);

            torneoService = new TorneoService(copa, ranking);
            prediccionService = new PrediccionService(copa);
            usuarioService = new UsuarioService(usuarios, prediccionService);
            simulacionService = new SimulacionService(copa, torneoService);

            torneoService.Inicializar();
        }

        private static void MostrarMenu()
        {
            Console.WriteLine("\n----------------- MENÚ -----------------");
            Console.WriteLine("1. Ver estructura del torneo (bracket)");
            Console.WriteLine("2. Gestionar usuarios");
            Console.WriteLine("3. Cargar predicciones");
            Console.WriteLine($"4. Elegir estrategia de simulación (actual: {copa.estrategiaSimulacion.Nombre})");
            Console.WriteLine("5. Simular partidos");
            Console.WriteLine("6. Ver ranking");
            Console.WriteLine("0. Salir");
            Console.Write("Elegí una opción: ");
        }

        // Selecciona el usuario y delega la carga de la predicción en el service.
        private static void MenuPredicciones()
        {
            var usuario = usuarioService.SeleccionarUsuario();
            if (usuario == null) return;
            prediccionService.MenuPredicciones(usuario);
        }

        // Muestra el ranking y, ahí mismo, deja tipear el número (la
        // posición en la tabla) de un usuario para ver su historial.
        private static void VerRanking()
        {
            var ordenado = ranking.ObtenerRankingOrdenado();
            ranking.MostrarRanking();
            if (ordenado.Count == 0) return;

            Console.Write("\nNúmero de usuario para ver su historial (enter para omitir): ");
            string entrada = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(entrada)) return;

            if (int.TryParse(entrada, out int idx) && idx >= 1 && idx <= ordenado.Count)
                usuarioService.MostrarHistorialPredicciones(ordenado[idx - 1]);
            else
                Console.WriteLine("Usuario inválido.");
        }
    }
}
