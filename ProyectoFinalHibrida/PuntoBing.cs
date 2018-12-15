using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoFinalHibrida
{
    class PuntoBing
    {
        private double latitude;
        private double longitude;
        private string accion;
        private string tipo;
        private string tipoRoad;
        private double distancia;
        private double tiempo;
        private ArrayList warnings;
        private ArrayList signs;

        public PuntoBing(double latitude, double longitude, string accion, string tipo, string tipoRoad, double distancia, double tiempo, ArrayList warnings, ArrayList signs)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.accion = accion;
            this.tipo = tipo;
            this.tipoRoad = tipoRoad;
            this.distancia = distancia;
            this.tiempo = tiempo;
            this.warnings = warnings;
            this.signs = signs;
        }

        public double Latitude { get => latitude; set => latitude = value; }
        public double Longitude { get => longitude; set => longitude = value; }
        public string Accion { get => accion; set => accion = value; }
        public string Tipo { get => tipo; set => tipo = value; }
        public string TipoRoad { get => tipoRoad; set => tipoRoad = value; }
        public double Distancia { get => distancia; set => distancia = value; }
        public double Tiempo { get => tiempo; set => tiempo = value; }
        public ArrayList Warnings { get => warnings; set => warnings = value; }
        public ArrayList Signs { get => signs; set => signs = value; }
        public override string ToString()
        {
            return Latitude + " " + Longitude;
        }
    }
}
