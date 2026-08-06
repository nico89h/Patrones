using System;
using System.Collections.Generic;
using System.Linq;
using MundialProde.Strategy;

namespace MundialProde.Models
{
    public class CopaMundo
    {
        private readonly string _nombre;

        // ===== PATRON COMPOSITE (uso correcto y recursivo) =====
        // CopaMundo NO guarda una lista plana de etapas. Guarda UN solo
        // ComponenteTorneo raíz:
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

        private Seleccion _campeon;

        // Se elige una vez desde el menú/servicio de simulación y se
        // reutiliza para simular etapas/partidos/todo el torneo. 
        // (CambiarEstrategia, NombreEstrategiaActual, SimularEtapa, SimularPartido, SimularTodo).
        private IEstrategiaSimulacion _estrategiaSimulacion = new EstrategiaEstadistica();

        public CopaMundo(string nombre)
        {
            _nombre = nombre;
            _raiz = new Etapa(nombre);
        }

        public string ObtenerNombre() => _nombre;
        public IReadOnlyList<Seleccion> ObtenerSelecciones() => _selecciones;

        // Busca cualquier componente del árbol por nombre (recursivo, vía Composite.Buscar).
        public ComponenteTorneo BuscarComponente(string nombre) => _raiz.Buscar(nombre);
        public List<Partido> TodosLosPartidos() => _raiz.ObtenerPartidos();
        public void Mostrar() => _raiz.Mostrar();
        public void AgregarAlArbol(ComponenteTorneo componente) => _raiz.Agregar(componente);
        public void SimularTodo() => _raiz.Simular(_estrategiaSimulacion);
        public void SimularEtapa(Etapa etapa) => etapa.Simular(_estrategiaSimulacion);
        public void SimularPartido(Partido partido) => partido.Simular(_estrategiaSimulacion);
        public void CambiarEstrategia(IEstrategiaSimulacion nueva) => _estrategiaSimulacion = nueva;
        public string ObtenerNombreEstrategiaActual() => _estrategiaSimulacion.Nombre;

        public IReadOnlyList<ComponenteTorneo> ObtenerRamasPrincipales()
            => _raiz is Etapa raiz ? raiz.ObtenerHijos().ToList() : new List<ComponenteTorneo>();

        public void AgregarSeleccion(Seleccion seleccion) => _selecciones.Add(seleccion);

        // ===== Campeón (lo define el Observer -Ranking- al finalizar la Final) =====
        public void DefinirCampeon(Seleccion seleccion) => _campeon = seleccion;

        public bool EsCampeon(Seleccion seleccion) => _campeon != null && _campeon == seleccion;
    }
}
