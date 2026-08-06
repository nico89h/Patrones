using System.Collections.Generic;
using MundialProde.Strategy;

namespace MundialProde.Models
{
    // ===== PATRON COMPOSITE (componente) =====
    // Permite tratar de manera uniforme a un Partido individual (hoja) y a una
    // Etapa completa del torneo (compuesto), sin que el código cliente tenga
    // que distinguir el nivel de profundidad del árbol del torneo.
    public abstract class ComponenteTorneo
    {
        protected string Nombre { get; set; }

        protected ComponenteTorneo(string nombre)
        {
            Nombre = nombre;
        }

        public abstract void Mostrar(string indent = "");
        public abstract List<Partido> ObtenerPartidos();
        public abstract void Simular(IEstrategiaSimulacion estrategia);
        public abstract bool EstaFinalizado();

        public virtual void Agregar(ComponenteTorneo c) { }
        public virtual void Eliminar(ComponenteTorneo c) { }

        // Busca recursivamente un componente (Etapa o Partido) por nombre en todo el árbol.
        // Al ser abstracto acá, tanto la hoja como el compuesto lo resuelven de forma uniforme.
        public abstract ComponenteTorneo Buscar(string nombre);
        public string ObtenerNombre() => Nombre;
    }
}
