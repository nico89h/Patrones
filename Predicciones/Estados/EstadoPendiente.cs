using MundialProde.Predicciones;

namespace MundialProde.Predicciones.Estados
{
    // Mientras el partido no se simuló: la predicción se puede editar y no suma puntos.
    public class EstadoPendiente : IEstadoPrediccion
    {
        public string Nombre => "Pendiente";

        public bool PuedeEditar() => true;

        public IEstadoPrediccion Evaluar(Prediccion prediccion)
        {
            return prediccion.Acerto()
                ? (IEstadoPrediccion)new EstadoAcertado()
                : new EstadoFallado();
        }

        public int CalcularPuntos(Prediccion prediccion) => 0;
    }
}
