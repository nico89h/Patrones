using System;
using System.Collections.Generic;
using MundialProde.Strategy;
using MundialProde.Observer;

namespace MundialProde.Models
{
    // ===== PATRON COMPOSITE (hoja) + PATRON OBSERVER (subject/notificador) =====
    // Un Partido es la hoja del árbol del torneo (Composite) y, a la vez, el
    // "publisher" que avisa a sus observadores (ej: el Ranking) cuando finaliza.
    public class Partido : ComponenteTorneo, IPartidoNotificador
    {
        private static readonly Random _rndPenales = new Random();

        public Seleccion Local { get; }
        public Seleccion Visitante { get; }
        public int GolesLocal { get; private set; }
        public int GolesVisitante { get; private set; }
        public EstadoPartido Estado { get; private set; } = EstadoPartido.Pendiente;
        public string Etapa { get; }
        public Seleccion GanadorPenales { get; private set; }

        private readonly List<IObservadorPartido> _observadores = new List<IObservadorPartido>();

        public Partido(Seleccion local, Seleccion visitante, string etapa)
            : base($"{local.Nombre} vs {visitante.Nombre}")
        {
            Local = local;
            Visitante = visitante;
            Etapa = etapa;
        }

        // ----- IPartidoNotificador -----
        public void Suscribir(IObservadorPartido observador) => _observadores.Add(observador);
        public void Desuscribir(IObservadorPartido observador) => _observadores.Remove(observador);

        public void Notificar()
        {
            foreach (var obs in _observadores)
                obs.ActualizarPuntaje(this);
        }

        // Ganador por goles (null si terminó empatado)
        public Seleccion Ganador
        {
            get
            {
                if (Estado != EstadoPartido.Finalizado) return null;
                if (GolesLocal > GolesVisitante) return Local;
                if (GolesVisitante > GolesLocal) return Visitante;
                return null;
            }
        }

        // Ganador real considerando definición por penales en fases eliminatorias
        public Seleccion GanadorDefinitivo => Ganador ?? GanadorPenales;

        public override void Simular(IEstrategiaSimulacion estrategia)
        {
            if (Estado == EstadoPartido.Finalizado) return;

            var resultado = estrategia.Simular(Local, Visitante);
            GolesLocal = resultado.golesLocal;
            GolesVisitante = resultado.golesVisitante;
            Estado = EstadoPartido.Finalizado;

            ActualizarEstadisticasGrupo();

            // En fase eliminatoria no puede quedar un ganador ausente: se define por penales.
            if (!Etapa.StartsWith("Grupo") && GolesLocal == GolesVisitante)
                GanadorPenales = _rndPenales.Next(2) == 0 ? Local : Visitante;

            // Dispara la notificación a los observadores (ej: Ranking) -> PATRON OBSERVER
            Notificar();

            // Se muestra el resultado apenas se termina de simular el partido.
            Mostrar();
        }

        private void ActualizarEstadisticasGrupo()
        {
            Local.GolesFavor += GolesLocal;
            Local.GolesContra += GolesVisitante;
            Visitante.GolesFavor += GolesVisitante;
            Visitante.GolesContra += GolesLocal;

            if (GolesLocal > GolesVisitante) Local.PuntosGrupo += 3;
            else if (GolesVisitante > GolesLocal) Visitante.PuntosGrupo += 3;
            else { Local.PuntosGrupo += 1; Visitante.PuntosGrupo += 1; }
        }

        public override List<Partido> ObtenerPartidos() => new List<Partido> { this };

        public override bool EstaFinalizado() => Estado == EstadoPartido.Finalizado;

        public override void Mostrar(string indent = "")
        {
            string resultado;
            if (Estado == EstadoPartido.Finalizado)
            {
                resultado = $"{GolesLocal} - {GolesVisitante}";
                if (GanadorPenales != null)
                    resultado += $" (definido por penales: {GanadorPenales.Nombre})";
            }
            else
            {
                resultado = "vs";
            }
            Console.WriteLine($"{indent}[{Etapa}] {Local.Nombre} {resultado} {Visitante.Nombre}  ({Estado})");
        }
    }
}
