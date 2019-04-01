using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using iMobileDevice;

namespace iFakeLocation
{
    public partial class MainForm : Form {
        private GMapOverlay markersOverlay;
        private PointLatLng? selectedLocation;

        public MainForm()
        {
            InitializeComponent();

            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            Text += " v" + ver.Major + "." + ver.Minor;

            try
            {
                // Load the native modules
                NativeLibraries.Load("x86");
            }
            catch
            {
                MessageBox.Show("Unable to load the necessary files to run the program.", Text, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            // Setup the map control
            FixUrls();
            baseMapComboBox.SelectedIndex = 0;
            mainGMapControl.Manager.Mode = AccessMode.ServerAndCache;
            mainGMapControl.MapProvider = GMapProviders.GoogleMap;
            mainGMapControl.DragButton = MouseButtons.Left;
            mainGMapControl.MouseWheelZoomEnabled = true;
            mainGMapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            mainGMapControl.ShowCenter = false;
            mainGMapControl.MinZoom = mainGMapControl.MapProvider.MinZoom;
            mainGMapControl.MaxZoom = 20;
            mainGMapControl.IgnoreMarkerOnMouseWheel = true;
            mainGMapControl.Zoom = 3;
            mainGMapControl.DisableFocusOnMouseEnter = true;
            mainGMapControl.MarkersEnabled = true;
            markersOverlay = new GMapOverlay();
            mainGMapControl.Overlays.Add(markersOverlay);

            // Geocode the location of current country to a coordinate
            var region = RegionInfo.CurrentRegion;
            GeoCoderStatusCode code;
            var latLng = GMapProviders.OpenStreetMap.GetPoint(region.EnglishName, out code);
            if (latLng.HasValue)
            {
                mainGMapControl.Position = latLng.Value;
            }
            else
            {
                MessageBox.Show(this, "Unable to initialise the map, ensure you are connected to the internet.", Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        public static void FixUrls()
        {
            var maps = typeof(GoogleMapProviderBase);
            var urls = maps.GetFields(BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic)
                .Where(x => x.Name.Contains("UrlFormat") && x.FieldType == typeof(string));
            foreach (var url in urls)
            {
                var value = (string)url.GetValue(null);
                url.SetValue(null, value.Replace("http://", "https://"));
            }
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(locationTextBox.Text))
            {
                // Convert keyword to coordinates
                GeoCoderStatusCode code;
                var latLng = GMapProviders.OpenStreetMap.GetPoint(locationTextBox.Text, out code);
                if (latLng.HasValue)
                {
                    // Set new marker location
                    markersOverlay.Clear();
                    markersOverlay.Markers.Add(new GMarkerTarget(latLng.Value));
                    mainGMapControl.Position = latLng.Value;
                    mainGMapControl.Zoom = 13;
                    selectedLocation = latLng.Value;
                    sendButton.Enabled = stopButton.Enabled = deviceComboBox.Items.Count > 0;
                }
                else
                {
                    MessageBox.Show(this, "Unable to find the specified location, ensure you entered the location correctly and are connected to the internet.", Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
        }

        private void mainGMapControl_MouseDoubleClick(object sender, MouseEventArgs e) {
            // Convert local X, Y click to lat and lng.
            var pos = mainGMapControl.FromLocalToLatLng(e.X, e.Y);
            markersOverlay.Clear();
            markersOverlay.Markers.Add(new GMarkerTarget(pos));
            selectedLocation = pos;
            sendButton.Enabled = stopButton.Enabled = deviceComboBox.Items.Count > 0;
        }

        private void refreshButton_Click(object sender, EventArgs e) {
            deviceComboBox.Items.Clear();

            // Get devices
            var devices = DeviceInformation.GetDevices();
            if (devices == null)
            {
                MessageBox.Show(this, "Unable to retrieve connected iDevices. Do you have iTunes installed? " +
                                      "Make sure your device is detected in iTunes first and then close iTunes. Click the Refresh button again.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Clear combo box and populate, auto-selecting the first one
            deviceComboBox.Items.Clear();
            foreach (var device in devices)
                deviceComboBox.Items.Add(device);

            if (deviceComboBox.Items.Count > 0)
                deviceComboBox.SelectedIndex = 0;

            // Enable/disable send button
            sendButton.Enabled = deviceComboBox.Items.Count > 0 && selectedLocation.HasValue;
            stopButton.Enabled = deviceComboBox.Items.Count > 0;
        }

        private void sendButton_Click(object sender, EventArgs e) {
            var deviceInfo = (DeviceInformation) deviceComboBox.Items[deviceComboBox.SelectedIndex];
            try
            {
                // Ensure that the corresponding developer image has been downloaded
                if (!DeveloperImageHelper.HasImageForDevice(deviceInfo))
                {
                    var links = DeveloperImageHelper.GetLinksForDevice(deviceInfo);
                    if (links == null)
                        throw new Exception("Unable to find developer image for the device's iOS version.");

                    var p = new DownloadForm();
                    foreach (var link in links)
                        p.AddDownload(link.Item1, link.Item2);
                    p.ShowDialog(this);

                    if (!p.Success)
                        return;
                }

                // Enable developer mode and send the specified location to the device
                string[] ps;
                if (DeveloperImageHelper.HasImageForDevice(deviceInfo, out ps))
                {
                    deviceInfo.EnableDeveloperMode(ps[0], ps[1]);
                    if (selectedLocation != null)
                    {
                        deviceInfo.SetLocation(selectedLocation.Value);
                        MessageBox.Show(
                            this,
                            "Location has been succesfully set. Confirm using Maps or other apps.",
                            Text,
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }        
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            var deviceInfo = (DeviceInformation)deviceComboBox.Items[deviceComboBox.SelectedIndex];
            try
            {
                // Ensure that the corresponding developer image has been downloaded
                if (!DeveloperImageHelper.HasImageForDevice(deviceInfo))
                {
                    var links = DeveloperImageHelper.GetLinksForDevice(deviceInfo);
                    if (links == null)
                        throw new Exception("Unable to find developer image for the device's iOS version.");

                    var p = new DownloadForm();
                    foreach (var link in links)
                        p.AddDownload(link.Item1, link.Item2);
                    p.ShowDialog(this);

                    if (!p.Success)
                        return;
                }

                // Enable developer mode and send the specified location to the device
                string[] ps;
                if (DeveloperImageHelper.HasImageForDevice(deviceInfo, out ps))
                {
                    deviceInfo.EnableDeveloperMode(ps[0], ps[1]);
                    deviceInfo.StopLocation();
                    MessageBox.Show(
                        this,
                        "Fake location has been stopped. If your location is still stuck, try turning Location Services off and back on.",
                        Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
            }
            catch (Exception ex)
            {
               MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void baseMapComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            mainGMapControl.MapProvider =
                new GMapProvider[] {GMapProviders.GoogleMap, GMapProviders.OpenStreetMap}[baseMapComboBox.SelectedIndex];
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e) {
            Environment.Exit(0);
        }
    }
}
