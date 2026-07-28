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
            // Recorrido por índice (no foreach): si mientras se simula un hijo
            // se agrega automáticamente uno nuevo a esta misma lista (ej:
            // RevisarAvanceDeFase() agrega la Semifinal apenas termina
            // Cuartos), el bucle lo sigue viendo y también lo simula, en
            // cascada, sin romper por "colección modificada durante la
            // enumeración".
            for (int i = 0; i < _hijos.Count; i++)
                _hijos[i].Simular(estrategia);
        }

        public override bool EstaFinalizado()
            => _hijos.Count > 0 && _hijos.All(h => h.EstaFinalizado());

        // Recorre el árbol: primero se pregunta a sí misma, después a cada hijo
        // (que a su vez, si es otra Etapa, sigue bajando recursivamente).
        public override ComponenteTorneo Buscar(string nombre)
        {
            if (Nombre == nombre) return this;
            foreach (var hijo in _hijos)
            {
                var encontrado = hijo.Buscar(nombre);
                if (encontrado != null) return encontrado;
            }
            return null;
        }
    }
}
