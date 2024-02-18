namespace iFakeLocation;

internal struct LocationRequest
{
    public double lat { get; init; }
    public double lng { get; init; }
    public string udid { get; init; }
}

internal readonly struct Records(double lat, double lng)
{
    public static readonly Records Empty = new Records(0, 0);

    public double Lat { get; init; } = lat;
    public double Lng { get; init; } = lng;
}