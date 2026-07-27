using System;
using System.Collections.Generic;
using System.Linq;

namespace MundialProde.Models
{
    public class CopaMundo
    {
        public string Nombre { get; }
        public List<Etapa> Etapas { get; } = new List<Etapa>();
        public List<Seleccion> Selecciones { get; } = new List<Seleccion>();
        public Seleccion Campeon { get; set; }

        public CopaMundo(string nombre)
        {
            Nombre = nombre;
        }

        public Etapa ObtenerEtapa(string nombre) => Etapas.FirstOrDefault(e => e.Nombre == nombre);

        public List<Partido> TodosLosPartidos() => Etapas.SelectMany(e => e.ObtenerPartidos()).ToList();

        public void Mostrar()
        {
            Console.WriteLine($"\n### {Nombre} ###");
            foreach (var etapa in Etapas)
                etapa.Mostrar("  ");
        }
    }
}
