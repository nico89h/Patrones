using System;
using System.Linq;
using MundialProde.Models;
using MundialProde.Strategy;

namespace MundialProde.Servicios
{
    // Encapsula la elección de estrategia de simulación (Strategy) y el
    // disparo de la simulación en sí (etapa puntual, partido puntual o todo
    // el torneo). La fase eliminatoria se sigue generando sola vía
    // TorneoService (RevisarAvanceDeFase, enganchada a Partido.Simular):
    // acá no hay ninguna opción de menú para "generarla a mano".
    public class SimulacionService
    {
        private readonly CopaMundo _copa;
        private readonly TorneoService _torneoService;

        public SimulacionService(CopaMundo copa, TorneoService torneoService)
        {
            _copa = copa;
            _torneoService = torneoService;
        }

        public void MenuElegirEstrategia()
        {
            var nueva = ElegirEstrategia();
            if (nueva != null)
            {
                _copa.CambiarEstrategia(nueva);
                Console.WriteLine($"Estrategia actual: {_copa.ObtenerNombreEstrategiaActual()}");
            }
            // Si nueva es null, ElegirEstrategia ya mostró el mensaje
            // correspondiente (cancelado u opción inválida).
        }

        private IEstrategiaSimulacion ElegirEstrategia()
        {
            Console.WriteLine("Elegí la estrategia de simulación:");
            Console.WriteLine("1. Ranking FIFA");
            Console.WriteLine("2. Historial de enfrentamientos");
            Console.WriteLine("3. Estadísticas de rendimiento reciente");
            Console.WriteLine("0. Cancelar");
            Console.Write("Opción: ");
            string op = Console.ReadLine();

            switch (op)
            {
                case "1": return new EstrategiaRankingFifa();
                case "2": return new EstrategiaHistorica();
                case "3": return new EstrategiaEstadistica();
                case "0":
                    Console.WriteLine($"Cancelado. Se mantiene la estrategia actual: {_copa.ObtenerNombreEstrategiaActual()}");
                    return null;
                default:
                    Console.WriteLine($"Opción inválida. Se mantiene la estrategia actual: {_copa.ObtenerNombreEstrategiaActual()}");
                    return null;
            }
        }

        public void MenuSimulacion()
        {
            Console.WriteLine($"Estrategia actual: {_copa.ObtenerNombreEstrategiaActual()}");
            Console.WriteLine("\n¿Qué querés simular?");
            Console.WriteLine("1. Una etapa específica");
            Console.WriteLine("2. Un partido puntual");
            Console.WriteLine("3. Todo el torneo (lo que esté pendiente)");
            Console.WriteLine("0. Volver");
            Console.Write("Opción: ");
            string op = Console.ReadLine();
            Console.WriteLine();

            switch (op)
            {
                case "1": SimularEtapa(); break;
                case "2": SimularPartido(); break;
                case "3": SimularTorneoCompleto(); break;
                case "0": break;
                default: Console.WriteLine("Opción inválida."); break;
            }
        }

        private void SimularEtapa()
        {
            var etapas = _torneoService.ObtenerEtapasSimulables();
            if (etapas.Count == 0) { Console.WriteLine("No hay etapas con partidos pendientes para simular."); return; }
            Console.WriteLine("Etapas disponibles:");
            for (int i = 0; i < etapas.Count; i++)
                Console.WriteLine($"{i + 1}. {etapas[i].ObtenerNombre()}");
            Console.WriteLine("0. Cancelar");
            Console.Write("Elegí una etapa (número): ");
            string entradaEtapa = Console.ReadLine();
            if (entradaEtapa == "0" || string.IsNullOrWhiteSpace(entradaEtapa)) { Console.WriteLine("Cancelado."); return; }
            if (int.TryParse(entradaEtapa, out int eIdx) && eIdx >= 1 && eIdx <= etapas.Count)
            {
                Console.WriteLine();
                _copa.SimularEtapa(etapas[eIdx - 1]);
                Console.WriteLine("\nEtapa simulada.");
            }
            else Console.WriteLine("Etapa inválida.");
        }

        private void SimularPartido()
        {
            var partidos = _copa.TodosLosPartidos().Where(p => !p.EstaFinalizado()).ToList();
            if (partidos.Count == 0) { Console.WriteLine("No hay partidos pendientes."); return; }
            for (int i = 0; i < partidos.Count; i++)
                Console.WriteLine($"{i + 1}. {partidos[i].DescripcionCorta()}");
            Console.WriteLine("0. Cancelar");
            Console.Write("Elegí un partido (número): ");
            string entradaPartido = Console.ReadLine();
            if (entradaPartido == "0" || string.IsNullOrWhiteSpace(entradaPartido)) { Console.WriteLine("Cancelado."); return; }
            if (int.TryParse(entradaPartido, out int pIdx) && pIdx >= 1 && pIdx <= partidos.Count)
            {
                Console.WriteLine();
                _copa.SimularPartido(partidos[pIdx - 1]);
            }
            else Console.WriteLine("Partido inválido.");
        }

        private void SimularTorneoCompleto()
        {
            _copa.SimularTodo();
            Console.WriteLine("\nSe simuló todo lo pendiente del torneo.");
        }
    }
}
