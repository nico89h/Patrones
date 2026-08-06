using System;
using System.Collections.Generic;
using System.Linq;
using MundialProde.Strategy;

namespace MundialProde.Models
{
    public class Etapa : ComponenteTorneo
    {
        private readonly List<ComponenteTorneo> _hijos = new List<ComponenteTorneo>();

        public Etapa(string nombre) : base(nombre) { }

        public override void Agregar(ComponenteTorneo c) => _hijos.Add(c);
        public override void Eliminar(ComponenteTorneo c) => _hijos.Remove(c);
        public IReadOnlyList<ComponenteTorneo> ObtenerHijos() => _hijos;

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
            for (int i = 0; i < _hijos.Count; i++)
                _hijos[i].Simular(estrategia);
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
    }
}
