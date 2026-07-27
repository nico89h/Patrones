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
        private readonly bool _mostrarNombre;

        public Etapa(string nombre, bool mostrarNombre = true) : base(nombre)
        {
            _mostrarNombre = mostrarNombre;
        }

        public override bool PuedeContenerComponentes => true;

        public override void Agregar(ComponenteTorneo c) => _hijos.Add(c);
        public void Eliminar(ComponenteTorneo c) => _hijos.Remove(c);
        public ComponenteTorneo Obtener(int i) => _hijos[i];
        public IReadOnlyList<ComponenteTorneo> Hijos => _hijos;

        public override void Mostrar(string indent = "")
        {
            if (_mostrarNombre)
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

        public override List<ComponenteTorneo> BuscarTodos(Func<ComponenteTorneo, bool> criterio)
        {
            var encontrados = new List<ComponenteTorneo>();
            if (criterio(this)) encontrados.Add(this);

            foreach (var hijo in _hijos)
                encontrados.AddRange(hijo.BuscarTodos(criterio));

            return encontrados;
        }
    }
}
