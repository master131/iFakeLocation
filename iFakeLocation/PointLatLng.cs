namespace iFakeLocation {
    struct PointLatLng {
        public static readonly PointLatLng Empty = new PointLatLng(0, 0);

        public double Lat { get; set; }
        public double Lng { get; set; }

        public PointLatLng(double lat, double lng) {
            Lat = lat;
            Lng = lng;
        }
    }
}