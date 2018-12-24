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
using Windows.Services.Maps;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


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
        MapIcon mapIconRuta;
        String urlInicial = "http://dev.virtualearth.net/REST/V1/Routes/Driving?wp.0=";
        String urlMedio = "&wp.1=";
        String urlFinal = "&avoid=minimizeTolls&output=json&key=88yPY0kZcOGX7RaKeHM8~ogAnZjvVpFAWmF1mTZUDZQ~AnnZzfuaCDOzl0HmlTs8aFZ9zjIgW8JFlm69BS6UUPsppWQgusCRU1C0VRk0wVHR";
        JsonArray arrayRuta;
        ArrayList puntos = new ArrayList();
        ArrayList warningItems = new ArrayList();
        int iteracionEscritura = 1;



        public MainPage()
        {
            this.InitializeComponent();
            mapView.MapServiceToken = "88yPY0kZcOGX7RaKeHM8~ogAnZjvVpFAWmF1mTZUDZQ~AnnZzfuaCDOzl0HmlTs8aFZ9zjIgW8JFlm69BS6UUPsppWQgusCRU1C0VRk0wVHR";

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            
            Geopoint lastPoint = new Geopoint(new BasicGeoposition()), currentPoint;

            Windows.UI.Core.DispatchedHandler actualizarTextBox = async () =>
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage stream = await client.GetAsync(urlInicial+lugarIncialTextBox.Text+urlMedio+lugarFinalTextBox.Text+urlFinal);
                //Si ha obtenido una ruta como respuesta, escribe en el mapa
                if (stream.IsSuccessStatusCode)
                {
                    String str = await stream.Content.ReadAsStringAsync();
                    JsonValue jsonValue = JsonValue.Parse(str);
                    arrayRuta = jsonValue.GetObject().GetNamedArray("resourceSets").GetObjectAt(0).GetNamedArray("resources").GetObjectAt(0).GetNamedArray("routeLegs").GetObjectAt(0).GetNamedArray("itineraryItems");

                    //Bucle que dibuja cada punto de la ruta y la ruta entre el punto actual y el anterior en el mapa
                    var first = arrayRuta.First();
                    foreach (var puntoRuta in arrayRuta)
                    {
                        PuntoBing puntoBing = obtenerPunto(puntoRuta.GetObject());

                        currentPoint = geopositionPoint(puntoBing.Latitude, puntoBing.Longitude);
                        mapIconRuta = new MapIcon
                        {
                            Location = currentPoint,
                            Title = "Pike Place Market"
                        };
                        mapView.MapElements.Add(mapIconRuta);

                        puntos.Add(puntoBing);
                        escribePunto(puntoBing);

                        //Si el punto actual es el último, pasa a ser el actual en el código
                        
                        if (!puntoRuta.Equals(first))
                        {

                            // Obtiene la ruta entre el punto anterior y el actual.
                            MapRouteFinderResult routeResult =
                                    await MapRouteFinder.GetDrivingRouteAsync(
                                    startPoint: lastPoint,
                                    endPoint: currentPoint,
                                    optimization: MapRouteOptimization.Time,
                                    restrictions: MapRouteRestrictions.None);

                            if (routeResult.Status == MapRouteFinderStatus.Success)
                            {

                                // Usa la ruta para inicializar MapRouteView.
                                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                                viewOfRoute.RouteColor = Colors.Yellow;
                                viewOfRoute.OutlineColor = Colors.Black;

                                // Añade el nuevo MapRouteView al conjunto de rutas
                                // de MapControl.
                                mapView.Routes.Add(viewOfRoute);

                                //El punto actual se convierte en el anterior
                                lastPoint = currentPoint;
                            }
                        }
                        else
                        {
                            lastPoint = currentPoint;
                        }
                    }
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

        private void escribePunto(PuntoBing punto)
        {

            rutaTextBox.Text =  rutaTextBox.Text + Environment.NewLine + "Acción " + iteracionEscritura + " : " + Environment.NewLine + " A " + punto.Distancia + "Km " + punto.Accion;
            iteracionEscritura += 1;
        }

        private void mapView_MapElementClick(MapControl sender, MapElementClickEventArgs args)
        {

            BasicGeoposition locationIcon = args.Location.Position;
            ArrayList listNumberWarning = new ArrayList();
            var warningI = 1;

            foreach (PuntoBing puntoBing in puntos)
            {
                if(puntoBing.Latitude == Math.Round(locationIcon.Latitude, 5) && puntoBing.Longitude == Math.Round(locationIcon.Longitude, 5))
                {
                    latitudTB.Text = puntoBing.Latitude.ToString();
                    longitudTB.Text = puntoBing.Longitude.ToString();
                    tipoAccionTB.Text = puntoBing.TipoRoad;
                    distanciaTB.Text = puntoBing.Distancia.ToString();
                    tiempoTB.Text = puntoBing.Tiempo.ToString();
                    foreach(var signal in puntoBing.Signs)
                    {
                        signalsTB.Text = signalsTB.Text + Environment.NewLine + signal.ToString();
                    }

                    foreach(var warning in puntoBing.Warnings)
                    {
                        warningItems.Add(warning as Warning);
                        listNumberWarning.Add("Advertencia: "+warningI);
                        warningI++;
                    }
                    warningsCB.ItemsSource = listNumberWarning;
                }
            }

        }

        private void warningsCB_DropDownClosed(object sender, object e)
        {
            var comboB = sender as ComboBox;
            var index = comboB.SelectedIndex;
            Warning wSelected = warningItems[index] as Warning;
            warningsTB.Text = "Grado: " + wSelected.Grado;
            warningsTB.Text = warningsTB.Text + Environment.NewLine + "Descripción: " + wSelected.Descripcion;
            warningsTB.Text = warningsTB.Text + Environment.NewLine + "Inicio: " + wSelected.CoordinateStart;
            warningsTB.Text = warningsTB.Text + Environment.NewLine + "Final: " + wSelected.CoordinateEnd;
        }
    }
}
