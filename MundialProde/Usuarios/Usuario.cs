using System.Collections.Generic;
using MundialProde.Predicciones;

namespace MundialProde.Usuarios
{
    public class Usuario
    {
        public string Nombre { get; }
        public int Puntaje { get; private set; }
        public bool EsBot { get; }
        private readonly List<Prediccion> _predicciones = new List<Prediccion>();
        public IReadOnlyList<Prediccion> Predicciones => _predicciones;
        public Usuario(string nombre, bool esBot = false)
        {
            Nombre = nombre;
            EsBot = esBot;
        }
        public void SumarPuntos(int puntos) => Puntaje += puntos;
        public void AgregarPrediccion(Prediccion prediccion) => _predicciones.Add(prediccion);
        public void RemoverPrediccion(Prediccion prediccion) => _predicciones.Remove(prediccion);
    }
}
