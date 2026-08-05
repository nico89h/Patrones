using MundialProde.Usuarios;
using MundialProde.Predicciones.Estados;

namespace MundialProde.Predicciones
{
    public abstract class Prediccion
    {
        private readonly Usuario _usuario;
        private IEstadoPrediccion _estado;

        protected Prediccion(Usuario usuario)
        {
            _usuario = usuario;
            _estado = new EstadoPendiente();
        }

        public bool PuedeEditar() => _estado.PuedeEditar();
        public void Evaluar()
        {
            _estado = _estado.Evaluar(this);
        }

        public int ObtenerPuntos() => _estado.CalcularPuntos(this);
        public string NombreEstado => _estado.Nombre;
        public abstract bool Acerto();
        public abstract int PuntosBase { get; }
        public abstract string Descripcion();
        public abstract string Tipo { get; }
    }
}
