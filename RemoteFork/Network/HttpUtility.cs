using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using RemoteFork.Properties;

namespace RemoteFork.Network {
    public static class HttpUtility {
        private static readonly CookieContainer cookieContainer = new CookieContainer();

        public static string GetRequest(string link, Dictionary<string, string> header = null) {
            try {
                using (var handler = new HttpClientHandler()) {
                    SetHandler(handler);
                    using (var httpClient = new HttpClient(handler)) {
                        AddHeader(httpClient, header);

                        var response = httpClient.GetAsync(link).Result;
                        return ReadContext(response.Content);
                    }
                }
            } catch (Exception ex) {
                Logger.Error("HttpUtility->GetRequest: {0}", ex.Message);
                return ex.Message;
            }
        }

        public static string PostRequest(string link, Dictionary<string, string> data,
            Dictionary<string, string> header = null) {
            try {
                using (var handler = new HttpClientHandler()) {
                    SetHandler(handler);
                    using (var httpClient = new HttpClient(handler)) {
                        AddHeader(httpClient, header);

                        HttpContent content = new FormUrlEncodedContent(data);

                        var response = httpClient.PostAsync(link, content).Result;
                        return ReadContext(response.Content);
                    }
                }
            } catch (Exception ex) {
                Logger.Error("HttpUtility->PostRequest: {0}", ex.Message);
                return ex.Message;
            }
        }

        private static void SetHandler(HttpClientHandler handler) {
            handler.CookieContainer = cookieContainer;
            handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip; 
        }

        private static void AddHeader(HttpClient httpClient, Dictionary<string, string> header) {
            if (header != null) {
                foreach (var h in header) {
                    try {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value);
                    } catch (Exception ex) {
                        Logger.Debug("HttpUtility->AddHeader: {0}", ex.Message);
                    }
                }
            }
            if (!httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(Settings.Default.UserAgent)) {
                Logger.Debug("HttpUtility->AddUserAgent: {0}", Settings.Default.UserAgent);
            }
        }

        private static string ReadContext(HttpContent context) {
            return context.ReadAsStringAsync().Result;
            //var result = context.ReadAsByteArrayAsync().Result;
            //try {
            //    var encoding = Encoding.GetEncoding(context.Headers.ContentType.CharSet);
            //    result = Encoding.Convert(encoding, Encoding.Default, result);
            //} catch {
            //    try {
            //        var encoding = Encoding.UTF8;
            //        result = Encoding.Convert(encoding, Encoding.Default, result);
            //    } catch {
            //        try {
            //            var encoding = Encoding.ASCII;
            //            result = Encoding.Convert(encoding, Encoding.Default, result);
            //        } catch {
            //            try {
            //                var encoding = Encoding.Unicode;
            //                result = Encoding.Convert(encoding, Encoding.Default, result);
            //            } catch (Exception ex) {
            //                Logger.Error("HttpUtility->ReadContext: {0}", ex.Message);
            //            }
            //        }
            //    }
            //}
            //return Encoding.Default.GetString(result);
        }
    }
}
