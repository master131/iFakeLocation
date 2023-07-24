using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace iFakeLocation
{
    internal class TSSRequest {
        private const string TSS_CONTROLLER_ACTION_URL = "http://gs.apple.com/TSS/controller?action=2";
        private const string TSS_CLIENT_VERSION_STRING = "libauthinstall-973.0.1";

        private Dictionary<string, object> _request = new() {
            { "@HostPlatformInfo", "mac" },
            { "@VersionInfo", TSS_CLIENT_VERSION_STRING },
            { "@UUID", Guid.NewGuid().ToString("D").ToUpperInvariant() }
        };

        public void Update(string key, object value) {
            _request[key] = value;
        }

        public void Update(Dictionary<string, object> dict) {
            foreach (var kvp in dict) {
                _request[kvp.Key] = kvp.Value;
            }
        }

        public void ApplyRestoreRequestRules(Dictionary<string, object> tssEntry, Dictionary<string, object> parameters,
            IEnumerable<Dictionary<string, object>> rules) {
            foreach (var rule in rules) {
                var conditionsFulfilled = true;
                foreach (var kvp in (Dictionary<string, object>)rule["Conditions"]) {
                    if (!conditionsFulfilled)
                        break;
                    object value2 = null;
                    if (kvp.Key == "ApRawProductionMode")
                        value2 = parameters.ContainsKey("ApProductionMode") ? parameters["ApProductionMode"] : null;
                    else if (kvp.Key == "ApCurrentProductionMode")
                        value2 = parameters.ContainsKey("ApProductionMode") ? parameters["ApProductionMode"] : null;
                    else if (kvp.Key == "ApRawSecurityMode")
                        value2 = parameters.ContainsKey("ApSecurityMode") ? parameters["ApSecurityMode"] : null;
                    else if (kvp.Key == "ApRequiresImage4")
                        value2 = parameters.ContainsKey("ApSupportsImg4") ? parameters["ApSupportsImg4"] : null;
                    else if (kvp.Key == "ApDemotionPolicyOverride")
                        value2 = parameters.ContainsKey("DemotionPolicy") ? parameters["DemotionPolicy"] : null;
                    else if (kvp.Key == "ApInRomDFU")
                        value2 = parameters.ContainsKey("ApInRomDFU") ? parameters["ApInRomDFU"] : null;

                    conditionsFulfilled = value2 != null && value2.Equals(kvp.Value);
                }

                if (!conditionsFulfilled)
                    continue;

                foreach (var kvp in (Dictionary<string, object>)rule["Actions"]) {
                    if (!kvp.Value.GetType().IsValueType || kvp.Value.ToString() != "255") {
                        tssEntry[kvp.Key] = kvp.Value;
                    }
                }
            }
        }

        public Dictionary<string, object> SendAndReceive() {
            var handler = new HttpClientHandler {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("User-Agent", "InetURL/1.0");
            client.DefaultRequestHeaders.Add("Expect", "");

            var plist = PlistHelper.ToPlistXml(_request);
            var response = client.PostAsync(TSS_CONTROLLER_ACTION_URL,
                new StringContent(plist, Encoding.UTF8, "text/xml")).Result;
            response.EnsureSuccessStatusCode();

            var responseBody = response.Content.ReadAsStringAsync().Result;
            var responseMessage = responseBody.Split(new[] { "MESSAGE=" }, 2, StringSplitOptions.None)[1].Split('&')[0];
            if (responseMessage != "SUCCESS") {
                throw new Exception("TSS request received unexpected response: " + responseMessage);
            }

            var plistBody = responseBody.Split(new[] { "REQUEST_STRING=" }, 2, StringSplitOptions.None)[1];
            return PlistHelper.ReadPlistDictFromString(plistBody);
        }
    }
}
