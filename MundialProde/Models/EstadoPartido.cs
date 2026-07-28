using MundialProde.Strategy;

namespace MundialProde.Models
{
    // ===== PATRON STATE =====
    public abstract class EstadoPartido
    {
        public abstract string Nombre { get; }
        public abstract void Simular(Partido partido, IEstrategiaSimulacion estrategia);
        public abstract bool EstaFinalizado();
        public abstract Seleccion ObtenerGanador(Partido partido);
        public abstract string ObtenerResultado(Partido partido);
    }
}
