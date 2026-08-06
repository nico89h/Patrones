using MundialProde.Predicciones;

namespace MundialProde.Predicciones.Estados
{
    // La predicción coincidió con el resultado real: ya no se puede editar y otorga puntos.
    public class EstadoAcertado : IEstadoPrediccion
    {
        public string Nombre => "Acertado";

        public bool PuedeEditar() => false;

        public IEstadoPrediccion Evaluar(Prediccion prediccion) => this; // estado final

        public int CalcularPuntos(Prediccion prediccion) => prediccion.ObtenerPuntosBase();
    }
}
