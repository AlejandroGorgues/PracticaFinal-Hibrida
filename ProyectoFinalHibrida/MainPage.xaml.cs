using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
        MapIcon mapIcon = new MapIcon();
        JObject o2;


        public MainPage()
        {
            this.InitializeComponent();
            mostrarContenido();

        }

        async Task mostrarContenido()
        {
            
            //rutaTextBox.Text = str;
            Windows.UI.Core.DispatchedHandler actualizarTextBox = async () =>
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage stream = await client.GetAsync("http://dev.virtualearth.net/REST/V1/Routes/Driving?wp.0=Madrid&wp.1=Segovia&avoid=minimizeTolls&output=json&key=UbyVV4Ma5EM8zXJ44OZi%7EE0q2RVZqdjQ2CX1z9HHMZw%7EAlQ1dCMOGkoxR9h0Gctn4QncW1KHvfVz_lvwqEobK-U2fcAQBw9z5hi9gWV6i2NU");
                String str = await stream.Content.ReadAsStringAsync();
                JsonValue jsonValue = JsonValue.Parse(str);
                string lat = jsonValue.GetObject().GetNamedString("brandLogoUri");
                string city = jsonValue.GetObject().GetNamedString("copyright");
                string pruebaconcatenacion = lat + Environment.NewLine + city;
                rutaTextBox.Text = lat + "\r\n" +city;
            };
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, actualizarTextBox);
            //double lat = jsonValue.GetObject().GetArray().GetObjectAt(0).GetNamedNumber("StatusCode");
            //string city = jsonValue.GetObject().GetArray().GetObjectAt(0).GetNamedString("Copyright");
          
            //rutaTextBox.Text = lat.ToString() + city;
            /*await Task.Run(async () =>
             {
                 using (StreamReader file = File.OpenText(@"assets\prueba.json"))
                 using (JsonTextReader reader = new JsonTextReader(file))
                 {
                     while (reader.Read())
                     {
                         if (reader.TokenType == JsonToken.StartObject)
                         {
                             // Load each object from the stream and do something with it
                             JObject obj = JObject.Load(reader);
                             Windows.UI.Core.DispatchedHandler actualizarTextBox = () =>
                             {
                                 rutaTextBox.Text = obj["Latitude"] + " - " + obj["City"];
                             };
                             await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, actualizarTextBox);
                             
                          
                         }
                     }

                 }
                //JsonValue jsonValue = JsonValue.Parse(@"assets\prueba.json");
                //double Latitude = o2.GetArray().GetObjectAt(0).GetNamedNumber("Latitude");
                //String City = jsonValue.GetObject().GetArray().GetObjectAt(1).GetNamedString("city");
                //rutaTextBox.Text = Latitude.ToString() + City;
                rutaTextBox.Text = o2.ToString();
             });*/
            
            
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Geoposition pos = await GPS.GetGeopositionAsync();
            Windows.UI.Core.DispatchedHandler Lectura_Posicion = () =>
            {
                mapView.Center = pos.Coordinate.Point;
                mapView.ZoomLevel = 15;
                mapIcon.Location = pos.Coordinate.Point;
                mapView.MapElements.Add(mapIcon);
            };
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Lectura_Posicion);
        }
    }
}
