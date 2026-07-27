using MundialProde.Usuarios;
using MundialProde.Predicciones;

namespace MundialProde.Factory
{
    // ===== PATRON FACTORY METHOD (creador) =====
    // Evita que la lógica de creación de cada modalidad de pronóstico esté
    // acoplada en una sola clase: cada subclase decide qué Prediccion concreta
    // instanciar, permitiendo agregar nuevas modalidades sin tocar el resto.
    public abstract class PrediccionFactory
    {
        public Prediccion CrearPrediccion(Usuario usuario, object[] parametros)
        {
            Prediccion prediccion = InstanciarPrediccion(usuario, parametros);
            usuario.AgregarPrediccion(prediccion);
            return prediccion;
        }

        protected abstract Prediccion InstanciarPrediccion(Usuario usuario, object[] parametros);
    }
}
