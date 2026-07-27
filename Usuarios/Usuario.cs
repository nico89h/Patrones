using System.Collections.Generic;
using MundialProde.Predicciones;

namespace MundialProde.Usuarios
{
    public class Usuario
    {
        public string Nombre { get; }
        public int Puntaje { get; private set; }
        public bool EsBot { get; }
        public List<Prediccion> Predicciones { get; } = new List<Prediccion>();

        public Usuario(string nombre, bool esBot = false)
        {
            Nombre = nombre;
            EsBot = esBot;
        }

        public void SumarPuntos(int puntos) => Puntaje += puntos;

        public void AgregarPrediccion(Prediccion prediccion) => Predicciones.Add(prediccion);
    }
}
