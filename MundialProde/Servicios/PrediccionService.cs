using System;
using System.Linq;
using MundialProde.Models;
using MundialProde.Usuarios;
using MundialProde.Predicciones;
using MundialProde.Factory;

namespace MundialProde.Servicios
{
    public class PrediccionService
    {
        private readonly CopaMundo _copa;
        private readonly Random _rnd = new Random();
        private readonly PrediccionResultadoFactory _factoryResultado = new PrediccionResultadoFactory();
        private readonly PrediccionCampeonFactory _factoryCampeon = new PrediccionCampeonFactory();

        public PrediccionService(CopaMundo copa)
        {
            _copa = copa;
        }

        public void MenuPredicciones(Usuario usuario)
        {
            Console.WriteLine("\n1. Predecir resultado de un partido");
            Console.WriteLine("2. Predecir campeón del torneo");
            Console.WriteLine("0. Volver");
            Console.Write("Opción: ");
            string op = Console.ReadLine();
            Console.WriteLine();

            switch (op)
            {
                case "1": PredecirResultado(usuario); break;
                case "2": PredecirCampeon(usuario); break;
                case "0": break;
                default: Console.WriteLine("Opción inválida."); break;
            }
        }

        private void PredecirResultado(Usuario usuario)
        {
            var pendientes = _copa.TodosLosPartidos().Where(p => !p.EstaFinalizado()).ToList();
            if (pendientes.Count == 0) { Console.WriteLine("No hay partidos pendientes para predecir."); return; }

            for (int i = 0; i < pendientes.Count; i++)
                Console.WriteLine($"{i + 1}. {pendientes[i].DescripcionCorta()}");
            Console.WriteLine("0. Cancelar");
            Console.Write("Elegí el partido (número): ");
            string entradaPartido = Console.ReadLine();
            if (entradaPartido == "0" || string.IsNullOrWhiteSpace(entradaPartido)) { Console.WriteLine("Cancelado."); return; }
            if (!int.TryParse(entradaPartido, out int pIdx) || pIdx < 1 || pIdx > pendientes.Count)
            {
                Console.WriteLine("Partido inválido.");
                return;
            }
            var partido = pendientes[pIdx - 1];

            var existente = usuario.ObtenerPredicciones()
                .OfType<PrediccionResultado>()
                .FirstOrDefault(p => p.CorrespondeAPartido(partido) && p.PuedeEditar());
            if (existente != null)
            {
                usuario.RemoverPrediccion(existente);
                Console.WriteLine("(se reemplazó tu predicción anterior para este partido)");
            }

            Console.Write($"Goles de {partido.ObtenerLocal().ObtenerNombre()}: ");
            int.TryParse(Console.ReadLine(), out int gl);
            Console.Write($"Goles de {partido.ObtenerVisitante().ObtenerNombre()}: ");
            int.TryParse(Console.ReadLine(), out int gv);

            _factoryResultado.CrearPrediccion(usuario, new object[] { partido, gl, gv });
            Console.WriteLine("Predicción cargada (estado: Pendiente).");
        }

        private void PredecirCampeon(Usuario usuario)
        {
            var existente = usuario.ObtenerPredicciones().OfType<PrediccionCampeon>().FirstOrDefault(p => p.PuedeEditar());
            if (existente != null)
            {
                usuario.RemoverPrediccion(existente);
                Console.WriteLine("(se reemplazó tu predicción de campeón anterior)");
            }

            var selecciones = _copa.ObtenerSelecciones();
            for (int i = 0; i < selecciones.Count; i++)
                Console.WriteLine($"{i + 1}. {selecciones[i].ObtenerNombre()}");
            Console.WriteLine("0. Cancelar");
            Console.Write("Elegí el campeón (número): ");
            string entradaSeleccion = Console.ReadLine();
            if (entradaSeleccion == "0" || string.IsNullOrWhiteSpace(entradaSeleccion)) { Console.WriteLine("Cancelado."); return; }
            if (!int.TryParse(entradaSeleccion, out int sIdx) || sIdx < 1 || sIdx > selecciones.Count)
            {
                Console.WriteLine("Selección inválida.");
                return;
            }

            _factoryCampeon.CrearPrediccion(usuario, new object[] { _copa, selecciones[sIdx - 1] });
            Console.WriteLine("Predicción de campeón cargada (estado: Pendiente).");
        }
        public void GenerarPrediccionesAutomaticas(Usuario bot)
        {
            foreach (var partido in _copa.TodosLosPartidos().Where(p => !p.EstaFinalizado()))
            {
                int gl = _rnd.Next(0, 4);
                int gv = _rnd.Next(0, 4);
                _factoryResultado.CrearPrediccion(bot, new object[] { partido, gl, gv });
            }

            var selecciones = _copa.ObtenerSelecciones();
            if (selecciones.Count > 0)
            {
                var seleccionAzar = selecciones[_rnd.Next(selecciones.Count)];
                _factoryCampeon.CrearPrediccion(bot, new object[] { _copa, seleccionAzar });
            }
        }
    }
}
