using MundialProde.Models;

namespace MundialProde.Observer
{
    // ===== PATRON OBSERVER (observador) =====
    public interface IObservadorPartido
    {
        void ActualizarPuntaje(Partido partido);
    }
}
