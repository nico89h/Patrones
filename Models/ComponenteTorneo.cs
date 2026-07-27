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
        public string Nombre { get; protected set; }

        protected ComponenteTorneo(string nombre)
        {
            Nombre = nombre;
        }

        public abstract void Mostrar(string indent = "");
        public abstract List<Partido> ObtenerPartidos();
        public abstract void Simular(IEstrategiaSimulacion estrategia);
        public abstract bool EstaFinalizado();
    }
}
