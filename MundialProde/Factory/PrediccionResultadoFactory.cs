using MundialProde.Usuarios;
using MundialProde.Predicciones;
using MundialProde.Models;

namespace MundialProde.Factory
{
    // parametros esperados: [Partido partido, int golesLocal, int golesVisitante]
    public class PrediccionResultadoFactory : PrediccionFactory
    {
        protected override Prediccion InstanciarPrediccion(Usuario usuario, object[] parametros)
        {
            var partido = (Partido)parametros[0];
            var golesLocal = (int)parametros[1];
            var golesVisitante = (int)parametros[2];
            return new PrediccionResultado(usuario, partido, golesLocal, golesVisitante);
        }
    }
}
