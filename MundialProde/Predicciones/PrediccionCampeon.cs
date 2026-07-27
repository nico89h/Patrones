using MundialProde.Usuarios;
using MundialProde.Models;

namespace MundialProde.Predicciones
{
    public class PrediccionCampeon : Prediccion
    {
        public CopaMundo Copa { get; }
        public Seleccion SeleccionPredicha { get; }

        public PrediccionCampeon(Usuario usuario, CopaMundo copa, Seleccion seleccionPredicha)
            : base(usuario)
        {
            Copa = copa;
            SeleccionPredicha = seleccionPredicha;
        }

        public override bool Acerto()
            => Copa.Campeon != null && Copa.Campeon == SeleccionPredicha;

        public override int PuntosBase => 10;

        public override string Descripcion() => $"Campeón: {SeleccionPredicha.Nombre}";
    }
}
