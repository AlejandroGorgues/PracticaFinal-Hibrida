using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace ProyectoFinalHibrida
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Geolocator GPS = new Geolocator();
        MapIcon mapIconStart = new MapIcon();
        MapIcon mapIconEnd = new MapIcon();
        MapIcon mapIconRuta = new MapIcon();
        String urlInicial = "http://dev.virtualearth.net/REST/V1/Routes/Driving?wp.0=";
        String urlMedio = "&wp.1=";
        String urlFinal = "&avoid=minimizeTolls&output=json&key=UbyVV4Ma5EM8zXJ44OZi%7EE0q2RVZqdjQ2CX1z9HHMZw%7EAlQ1dCMOGkoxR9h0Gctn4QncW1KHvfVz_lvwqEobK-U2fcAQBw9z5hi9gWV6i2NU";
        JsonObject objetoPuntos;
        JsonArray arrayRuta;


        public MainPage()
        {
            this.InitializeComponent();

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            double latEnd, longEnd, latStart, longStart;
            //Geoposition pos = await GPS.GetGeopositionAsync();
            Windows.UI.Core.DispatchedHandler actualizarTextBox = async () =>
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage stream = await client.GetAsync(urlInicial+lugarIncialTextBox.Text+urlMedio+lugarFinalTextBox.Text+urlFinal);
                if (stream.IsSuccessStatusCode)
                {
                    String str = await stream.Content.ReadAsStringAsync();
                    JsonValue jsonValue = JsonValue.Parse(str);
                    objetoPuntos = jsonValue.GetObject().GetNamedArray("resourceSets").GetObjectAt(0).GetNamedArray("resources").GetObjectAt(0).GetNamedArray("routeLegs").GetObjectAt(0);

                    arrayRuta = objetoPuntos.GetNamedArray("itineraryItems");
                    
                    //Bucle que dibuja cada punto de la ruta en el mapa
                    foreach (var puntoRuta in arrayRuta)
                    {
                        PuntoBing puntoBing = obtenerPunto(puntoRuta.GetObject());
                        mapIconRuta.Location = geopositionPoint(puntoBing.Latitude, puntoBing.Longitude);
                        mapView.MapElements.Add(mapIconRuta);
                        rutaTextBox.Text = rutaTextBox.Text + "\r\n" + puntoBing.ToString();
       
                    }

                    //Obtención de la latitud y longitud inicial y final para ser dibujados en el mapa
                    latEnd = objetoPuntos.GetNamedObject("actualEnd").GetNamedArray("coordinates").GetNumberAt(0);
                    longEnd = objetoPuntos.GetNamedObject("actualEnd").GetNamedArray("coordinates").GetNumberAt(1);
                    latStart = objetoPuntos.GetNamedObject("actualStart").GetNamedArray("coordinates").GetNumberAt(0);
                    longStart = objetoPuntos.GetNamedObject("actualStart").GetNamedArray("coordinates").GetNumberAt(1);

                    mapIconStart.Location = geopositionPoint(latStart, longStart);
                    mapIconEnd.Location = geopositionPoint(latEnd, longEnd);
                    mapView.MapElements.Add(mapIconStart);
                    mapView.MapElements.Add(mapIconEnd);
                }
                else
                {
                    rutaTextBox.Text = "Mal";
                }
            };

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, actualizarTextBox);
        }

        //Devuelve un objeto de tipo Geopoint con su latitud y longitud
        private Geopoint geopositionPoint(double latitude, double longitude)
        {
            BasicGeoposition positionBasicEnd = new BasicGeoposition()
            {
                Latitude = latitude,
                Longitude = longitude
            };

            return new Geopoint(positionBasicEnd);
        }


        //Devuelve el contenido de la clave warning como un objeto de tipo Warning
        private Warning obtenerWarning(JsonObject warning)
        {
            if (warning.ContainsKey("origin"))
            {
                return new Warning(warning.GetNamedString("severity"), warning.GetNamedString("text"), warning.GetNamedString("warningType"), warning.GetNamedString("origin"), warning.GetNamedString("to"));
            }
            else
            {
                return new Warning(warning.GetNamedString("severity"), warning.GetNamedString("text"), warning.GetNamedString("warningType"), "0", "0");
            }
            
        } 

        //Devuelve el contenido de un punto de tipo ruta como un objeto de tipo PuntoBing 
        private PuntoBing obtenerPunto(JsonObject puntoJson)
        {
            ArrayList warnings = new ArrayList();
            ArrayList signs = new ArrayList();

            JsonObject datosManiobrabilidad = puntoJson.GetNamedObject("maneuverPoint");
            JsonArray coordenadas = datosManiobrabilidad.GetNamedArray("coordinates");
            JsonObject instruccion = puntoJson.GetNamedObject("instruction");
            JsonObject detalles = puntoJson.GetNamedArray("details").GetObjectAt(0);

            if (puntoJson.ContainsKey("warnings"))
            {
                foreach (var warningAux in puntoJson.GetNamedArray("warnings"))
                {
                    warnings.Add(obtenerWarning(warningAux.GetObject()));
                }
            }


            if(puntoJson.ContainsKey("signs"))
            {
                foreach (var sign in puntoJson.GetNamedArray("signs"))
                {
                    signs.Add(sign);
                }
            }

            return new PuntoBing(coordenadas.GetNumberAt(0), coordenadas.GetNumberAt(1), instruccion.GetNamedString("text"), instruccion.GetNamedString("maneuverType"), detalles.GetNamedString("roadType"), puntoJson.GetNamedNumber("travelDistance"), puntoJson.GetNamedNumber("travelDuration"), warnings, signs); 
        }
    }
}
