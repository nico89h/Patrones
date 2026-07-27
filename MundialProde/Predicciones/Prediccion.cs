using MundialProde.Usuarios;
using MundialProde.Predicciones.Estados;

namespace MundialProde.Predicciones
{
    // ===== PATRON STATE (contexto) =====
    public abstract class Prediccion
    {
        public Usuario Usuario { get; }
        public IEstadoPrediccion Estado { get; private set; }

        protected Prediccion(Usuario usuario)
        {
            Usuario = usuario;
            Estado = new EstadoPendiente();
        }

        public bool PuedeEditar() => Estado.PuedeEditar();

        // Delega en el estado actual la transición correspondiente.
        public void Evaluar()
        {
            Estado = Estado.Evaluar(this);
        }

        public int ObtenerPuntos() => Estado.CalcularPuntos(this);

        // Cada tipo concreto de predicción sabe comparar contra la realidad.
        public abstract bool Acerto();
        public abstract int PuntosBase { get; }
        public abstract string Descripcion();
    }
}
