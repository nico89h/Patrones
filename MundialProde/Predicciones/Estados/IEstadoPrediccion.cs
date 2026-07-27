using MundialProde.Predicciones;

namespace MundialProde.Predicciones.Estados
{
    // ===== PATRON STATE =====
    // El comportamiento de una Prediccion (si se puede editar, cuántos puntos
    // otorga) depende por completo de su estado actual.
    public interface IEstadoPrediccion
    {
        string Nombre { get; }
        bool PuedeEditar();
        IEstadoPrediccion Evaluar(Prediccion prediccion);
        int CalcularPuntos(Prediccion prediccion);
    }
}
