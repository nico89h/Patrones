using MundialProde.Usuarios;
using MundialProde.Predicciones;
using MundialProde.Models;

namespace MundialProde.Factory
{
    // parametros esperados: [CopaMundo copa, Seleccion seleccionElegida]
    public class PrediccionCampeonFactory : PrediccionFactory
    {
        protected override Prediccion InstanciarPrediccion(Usuario usuario, object[] parametros)
        {
            var copa = (CopaMundo)parametros[0];
            var seleccion = (Seleccion)parametros[1];
            return new PrediccionCampeon(usuario, copa, seleccion);
        }
    }
}
