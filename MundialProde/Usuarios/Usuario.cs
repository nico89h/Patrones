using System.Collections.Generic;
using MundialProde.Predicciones;

namespace MundialProde.Usuarios
{
    public class Usuario
    {
        private readonly string _nombre;
        private int _puntaje;
        private readonly bool _esBot;
        private readonly List<Prediccion> _predicciones = new List<Prediccion>();

        public Usuario(string nombre, bool esBot = false)
        {
            _nombre = nombre;
            _esBot = esBot;
        }

        public string ObtenerNombre() => _nombre;
        public int ObtenerPuntaje() => _puntaje;
        public bool EsBot() => _esBot;
        public IReadOnlyList<Prediccion> ObtenerPredicciones() => _predicciones;

        public void SumarPuntos(int puntos) => _puntaje += puntos;
        public void AgregarPrediccion(Prediccion prediccion) => _predicciones.Add(prediccion);
        public void RemoverPrediccion(Prediccion prediccion) => _predicciones.Remove(prediccion);
    }
}
