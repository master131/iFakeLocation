using System.Collections.Generic;
using System.IO;
using Claunia.PropertyList;
using iMobileDevice;
using iMobileDevice.Plist;

namespace iFakeLocation {
    static class PlistHelper {
        private static object ConvertNSToNET(NSObject obj) {
            if (obj == null) {
                return null;
            }

            if (obj is NSDictionary) {
                return ReadPlistDictFromNSObject(obj);
            }

            if (obj is NSArray a) {
                var arr = new object[a.Count];
                for (int i = 0; i < arr.Length; i++)
                    arr[i] = ConvertNSToNET(a[i]);
                return arr;
            }

            if (obj is NSData d) {
                return d.Bytes;
            }

            if (obj is NSDate dd) {
                return dd.Date;
            }

            if (obj is NSNumber n) {
                if (n.isBoolean()) {
                    return n.ToBool();
                }

                if (n.isInteger()) {
                    return n.ToLong();
                }

                if (n.isReal()) {
                    return n.ToDouble();
                }
            }

            if (obj is NSSet s) {
                var objs = s.AllObjects();
                var arr = new List<object>();
                for (var i = 0; i < s.Count; i++)
                    arr.Add(ConvertNSToNET(objs[i]));
                return arr;
            }

            if (obj is NSString ss) {
                return ss.Content;
            }
            return null;
        }

        private static Dictionary<string, object> ReadPlistDictFromNSObject(NSObject nsObject,
            ICollection<string> keys = null) {
            var dict = new Dictionary<string, object>();
            if (nsObject is not NSDictionary nsDict) {
                return dict;
            }

            foreach (var kvp in nsDict) {
                if (keys != null && !keys.Contains(kvp.Key))
                    continue;
                dict[kvp.Key] = ConvertNSToNET(kvp.Value);
            }
            return dict;
        }

        public static Dictionary<string, object> ReadPlistDictFromNode(PlistHandle node,
            ICollection<string> keys = null) {
            var dict = new Dictionary<string, object>();
            var plist = LibiMobileDevice.Instance.Plist;

            var nt = plist.plist_get_node_type(node);
            if (nt != PlistType.Dict)
                return dict;

            // Convert to XML and parse via plist-cil (libplist appears to have bugs that cause crashes?)
            uint length = 0;
            plist.plist_to_xml(node, out var plistXml, ref length);
            return ReadPlistDictFromNSObject(XmlPropertyListParser.ParseString(plistXml), keys);
        }

        public static Dictionary<string, object>
            ReadPlistDictFromStream(Stream stream, ICollection<string> keys = null) {

            var nsObject = PropertyListParser.Parse(stream);
            return ReadPlistDictFromNSObject(nsObject, keys);
        }

        public static Dictionary<string, object>
            ReadPlistDictFromString(string str, ICollection<string> keys = null) {

            var nsObject = XmlPropertyListParser.ParseString(str);
            return ReadPlistDictFromNSObject(nsObject, keys);
        }

        public static string ToPlistXml(Dictionary<string, object> dict) {
            return NSObject.Wrap(dict).ToXmlPropertyList();
        }
    }
}