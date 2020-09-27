using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using iMobileDevice;
using iMobileDevice.Plist;

namespace iFakeLocation {
    static class PlistReader {
        private static unsafe object ReadValueFromNode(PlistHandle node) {
            if (node == null || node.IsInvalid)
                return null;

            var plist = LibiMobileDevice.Instance.Plist;
            switch (plist.plist_get_node_type(node)) {
                case PlistType.Boolean:
                    char c = '\0';
                    plist.plist_get_bool_val(node, ref c);
                    return c != '\0';
                case PlistType.Uint:
                    ulong u = 0;
                    plist.plist_get_uint_val(node, ref u);
                    return u;
                case PlistType.Real:
                    double d = 0;
                    plist.plist_get_real_val(node, ref d);
                    return d;
                case PlistType.String:
                    string s;
                    plist.plist_get_string_val(node, out s);
                    return s;
                case PlistType.Key:
                    string k;
                    plist.plist_get_key_val(node, out k);
                    return k;
                case PlistType.Data:
                    string data;
                    ulong len = 0;
                    plist.plist_get_data_val(node, out data, ref len);
                    byte[] b = new byte[len];
                    fixed (char* cc = data)
                        Marshal.Copy((IntPtr) cc, b, 0, b.Length);
                    return b;
                case PlistType.Date:
                    int sec = 0, usec = 0;
                    plist.plist_get_date_val(node, ref sec, ref usec);
                    return new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(sec).ToLocalTime();
                case PlistType.Dict:
                    return ReadPlistDictFromNode(node);
                case PlistType.Array:
                    uint length = 0;
                    plist.plist_to_xml(node, out var result, ref length);
                    return result;
            }

            return null;
        }

        public static Dictionary<string, object> ReadPlistDictFromNode(PlistHandle node,
            ICollection<string> keys = null) {
            var dict = new Dictionary<string, object>();
            var plist = LibiMobileDevice.Instance.Plist;

            var nt = plist.plist_get_node_type(node);
            if (nt != PlistType.Dict)
                return dict;

            PlistHandle subnode = null;
            PlistDictIterHandle it = null;

            plist.plist_dict_new_iter(node, out it);
            string key;
            plist.plist_dict_next_item(node, it, out key, out subnode);
            while (subnode != null && !subnode.IsInvalid) {
                if (keys == null || keys.Contains(key))
                    dict[key] = ReadValueFromNode(subnode);
                subnode.Close();
                plist.plist_dict_next_item(node, it, out key, out subnode);
            }

            return dict;
        }
    }
}