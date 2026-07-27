using MundialProde.Predicciones;

namespace MundialProde.Predicciones.Estados
{
    // La predicción no coincidió con el resultado real: no se puede editar y no suma puntos.
    public class EstadoFallado : IEstadoPrediccion
    {
        public string Nombre => "Fallado";

        public bool PuedeEditar() => false;

        public IEstadoPrediccion Evaluar(Prediccion prediccion) => this; // estado final

        public int CalcularPuntos(Prediccion prediccion) => 0;
    }
}
