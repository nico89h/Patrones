using System;
using System.Collections.Generic;
using System.Linq;
using MundialProde.Strategy;

namespace MundialProde.Models
{
    public class CopaMundo
    {
        public string Nombre { get; }

        // ===== PATRON COMPOSITE =====
        // CopaMundo NO guarda una lista plana de etapas. Guarda UN solo ComponenteTorneo raíz
        // Todo el árbol del torneo cuelga de ahí:
        //
        //   Raiz ("Copa Mundial 2026")
        //    ├── "Fase de Grupos"
        //    │     ├── "Grupo A" -> Partido, Partido, ...
        //    │     ├── "Grupo B" -> ...
        //    │     ├── "Grupo C" -> ...
        //    │     └── "Grupo D" -> ...
        //    └── "Fase Eliminatoria"   (se va completando a medida que avanza el torneo)
        //          ├── "Cuartos de Final" -> Partido, Partido, Partido, Partido
        //          ├── "Semifinal" -> Partido, Partido
        //          └── "Final" -> Partido
        //
        // Gracias al Composite, da lo mismo pedirle Mostrar()/Simular()/
        // ObtenerPartidos() a la Raiz completa, a "Fase de Grupos", a
        // "Grupo A" o a un Partido suelto: la interfaz es la misma.
        private readonly ComponenteTorneo _raiz;
        private readonly List<Seleccion> _selecciones = new List<Seleccion>();
        public IReadOnlyList<Seleccion> Selecciones => _selecciones;

        private Seleccion _campeon;

        // Se elige una vez desde el menú/servicio de simulación y se
        // reutiliza; getter y setter porque se lee y se cambia individualmente
        // desde afuera del modelo (Strategy intercambiable en tiempo de ejecución)
        public IEstrategiaSimulacion estrategiaSimulacion { get; set; } = new EstrategiaEstadistica();

        public CopaMundo(string nombre)
        {
            Nombre = nombre;
            _raiz = new Etapa(nombre);
        }

        // Busca cualquier componente del árbol por nombre (recursivo, vía Composite.Buscar).
        public ComponenteTorneo BuscarComponente(string nombre) => _raiz.Buscar(nombre);
        public List<Partido> TodosLosPartidos() => _raiz.ObtenerPartidos();
        public void Mostrar() => _raiz.Mostrar();
        public void AgregarAlArbol(ComponenteTorneo componente) => _raiz.Agregar(componente);
        public void SimularTodo(IEstrategiaSimulacion estrategia) => _raiz.Simular(estrategia);

        public IReadOnlyList<ComponenteTorneo> ObtenerRamasPrincipales()
            => _raiz is Etapa raiz ? raiz.Hijos.ToList() : new List<ComponenteTorneo>();

        public void AgregarSeleccion(Seleccion seleccion) => _selecciones.Add(seleccion);

        // ===== Campeón (lo define el Observer -Ranking- al finalizar la Final) =====
        public void DefinirCampeon(Seleccion seleccion) => _campeon = seleccion;

        public bool EsCampeon(Seleccion seleccion) => _campeon != null && _campeon == seleccion;
    }
}
