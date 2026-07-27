using System;
using System.Collections.Generic;
using MundialProde.Strategy;

namespace MundialProde.Models
{
    public class CopaMundo
    {
        public string Nombre { get; }
        // CopaMundo conoce únicamente el componente raíz, nunca el tipo de
        // compuesto concreto que organiza el torneo.
        public ComponenteTorneo Raiz { get; }
        public List<Seleccion> Selecciones { get; } = new List<Seleccion>();
        public Seleccion Campeon { get; set; }

        public CopaMundo(string nombre, ComponenteTorneo raiz)
        {
            Nombre = nombre;
            Raiz = raiz ?? throw new ArgumentNullException(nameof(raiz));
        }

        public void AgregarComponente(ComponenteTorneo componente) => Raiz.Agregar(componente);

        public ComponenteTorneo BuscarComponente(string nombre) => Raiz.Buscar(nombre);

        public List<ComponenteTorneo> BuscarComponentes(Func<ComponenteTorneo, bool> criterio)
            => Raiz.BuscarTodos(criterio);

        public List<Partido> TodosLosPartidos() => Raiz.ObtenerPartidos();

        public void Simular(IEstrategiaSimulacion estrategia) => Raiz.Simular(estrategia);

        public void Mostrar()
        {
            Console.WriteLine($"\n### {Nombre} ###");
            Raiz.Mostrar("  ");
        }
    }
}
