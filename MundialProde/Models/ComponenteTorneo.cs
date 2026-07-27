using System.Collections.Generic;
using System;
using MundialProde.Strategy;

namespace MundialProde.Models
{
    // ===== PATRON COMPOSITE (componente) =====
    // Permite tratar de manera uniforme a un Partido individual (hoja) y a una
    // Etapa completa del torneo (compuesto), sin que el código cliente tenga
    // que distinguir el nivel de profundidad del árbol del torneo.
    public abstract class ComponenteTorneo
    {
        public string Nombre { get; protected set; }

        protected ComponenteTorneo(string nombre)
        {
            Nombre = nombre;
        }

        public abstract void Mostrar(string indent = "");
        public abstract List<Partido> ObtenerPartidos();
        public abstract void Simular(IEstrategiaSimulacion estrategia);
        public abstract bool EstaFinalizado();
        public abstract bool PuedeContenerComponentes { get; }

        // Las operaciones de navegación se resuelven dentro del árbol. Una
        // hoja se consulta a sí misma y un compuesto delega en sus hijos.
        public abstract ComponenteTorneo Buscar(string nombre);
        public abstract List<ComponenteTorneo> BuscarTodos(Func<ComponenteTorneo, bool> criterio);

        // Sólo los compuestos aceptan hijos. Partido (hoja) conserva la
        // interfaz uniforme, pero rechaza explícitamente esta operación.
        public virtual void Agregar(ComponenteTorneo componente)
            => throw new NotSupportedException("Un partido no puede contener componentes.");
    }
}
