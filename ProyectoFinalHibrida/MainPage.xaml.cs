using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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


        public MainPage()
        {
            this.InitializeComponent();
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
