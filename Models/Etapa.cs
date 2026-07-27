using System;
using System.Collections.Generic;
using System.Linq;
using MundialProde.Strategy;

namespace MundialProde.Models
{
    // ===== PATRON COMPOSITE (compuesto) =====
    // Una Etapa (ej: "Grupo A", "Cuartos de Final") agrupa Partidos (u otras
    // Etapas, si se quisiera anidar más) y expone la misma interfaz que un
    // Partido individual: Mostrar(), ObtenerPartidos(), Simular(), EstaFinalizado().
    public class Etapa : ComponenteTorneo
    {
        private readonly List<ComponenteTorneo> _hijos = new List<ComponenteTorneo>();

        public Etapa(string nombre) : base(nombre) { }

        public void Agregar(ComponenteTorneo c) => _hijos.Add(c);
        public void Eliminar(ComponenteTorneo c) => _hijos.Remove(c);
        public ComponenteTorneo Obtener(int i) => _hijos[i];
        public IReadOnlyList<ComponenteTorneo> Hijos => _hijos;

        public override void Mostrar(string indent = "")
        {
            Console.WriteLine($"{indent}== {Nombre} ==");
            foreach (var hijo in _hijos)
                hijo.Mostrar(indent + "   ");
        }

        public override List<Partido> ObtenerPartidos()
            => _hijos.SelectMany(h => h.ObtenerPartidos()).ToList();

        public override void Simular(IEstrategiaSimulacion estrategia)
        {
            foreach (var hijo in _hijos)
                hijo.Simular(estrategia);
        }

        public override bool EstaFinalizado()
            => _hijos.Count > 0 && _hijos.All(h => h.EstaFinalizado());
    }
}
