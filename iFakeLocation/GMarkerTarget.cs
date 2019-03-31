using System.Drawing;
using GMap.NET;
using GMap.NET.WindowsForms;
using iFakeLocation.Properties;

namespace iFakeLocation
{
    class GMarkerTarget : GMapMarker {
        private static readonly Bitmap Image = Resources.target;

        public GMarkerTarget(PointLatLng pos) : base(pos) {
            IsHitTestVisible = false;
        }

        public override void OnRender(Graphics g) {
            g.DrawImage(Image, LocalPosition.X - Image.Width / 2,
                LocalPosition.Y - Image.Height / 2);
            base.OnRender(g);
        }
    }
}
