using System;
using System.Collections.Generic;

namespace MundialProde.Models
{
    public class CopaMundo
    {
        public string Nombre { get; }

        // ===== PATRON COMPOSITE (uso correcto y recursivo) =====
        // CopaMundo NO guarda una lista plana de etapas. Guarda UN solo
        // ComponenteTorneo raíz. Todo el árbol del torneo cuelga de ahí:
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
        public Etapa Raiz { get; }

        public List<Seleccion> Selecciones { get; } = new List<Seleccion>();
        public Seleccion Campeon { get; set; }

        public CopaMundo(string nombre)
        {
            Nombre = nombre;
            Raiz = new Etapa(nombre);
        }

        // Busca cualquier etapa del árbol por nombre (recursivo, vía Composite.Buscar).
        public Etapa ObtenerEtapa(string nombre) => Raiz.Buscar(nombre) as Etapa;

        public List<Partido> TodosLosPartidos() => Raiz.ObtenerPartidos();

        public void Mostrar() => Raiz.Mostrar();
    }
}
