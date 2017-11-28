using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;
using Common.Logging;
using RemoteFork.Properties;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteFork.Network {
    public static class HttpUtility {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpUtility));

        private static readonly CookieContainer CookieContainer = new CookieContainer();
        private static bool _clearCookies;
        private static System.IO.StreamReader stream;
        private static System.IO.StreamReader stream2;
        public static void GetByteRequest(Unosquare.Net.HttpListenerResponse output, string link, Dictionary<string, string> header = null, bool wcc = false)
        {
            try
            {
                Console.WriteLine("GetByteRequest");
                HttpWebRequest wc=null;
                if (wcc)
                {
                    wc = (HttpWebRequest)HttpWebRequest.Create(link);
                    wc.Proxy = GlobalProxySelection.GetEmptyWebProxy();
                }
                _clearCookies = false;
                if (header != null)
                {
                    foreach (var h in header)
                    {
                        try
                        {
                            Console.WriteLine(h.Key + " set=" + h.Value);
                            if (h.Key == "Cookie")
                            {
                                _clearCookies = true;
                                CookieContainer.SetCookies(new Uri(link), h.Value.Replace(";", ","));
                            }
                            if (wcc)
                            {
                                if (h.Key == "Range")
                                {
                                    var x = h.Value.Split('=')[1].Split('-');
                                    if (x.Length == 1) wc.AddRange(Convert.ToInt64(x[0]));
                                    else if (x.Length == 2)
                                    {
                                        if (String.IsNullOrEmpty(x[1]))
                                        {
                                            Console.WriteLine(x[0]);
                                            if (Convert.ToInt64(x[0]) > 0) wc.AddRange(Convert.ToInt64(x[0]));
                                        }
                                        else wc.AddRange(Convert.ToInt64(x[0]), Convert.ToInt64(x[1]));
                                    }
                                }
                                else wc.Headers.Add(h.Key, h.Value);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Er getbyte h " + ex.ToString());
                            System.IO.File.AppendAllText(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6) + "\\log.txt", ex.Message + " Error h " + link + Environment.NewLine);
                        }
                    }
                }
                if (wcc)
                {
                    var resp = (HttpWebResponse)wc.GetResponse();
                    Console.WriteLine(resp.Headers);
                    if (!string.IsNullOrEmpty(resp.Headers["Content-Length"]))
                        output.ContentLength64 = Convert.ToInt64(resp.Headers["Content-Length"]);

                    Console.WriteLine("wc stream");
                    stream = new System.IO.StreamReader(resp.GetResponseStream());
                    stream.BaseStream.CopyTo(output.OutputStream);
                }
                else
                {
                    Console.WriteLine("HttpClientHandler stream");                   
                    using (var handler = new HttpClientHandler())
                    {
                        SetHandler(handler);
                        using (var httpClient = new System.Net.Http.HttpClient(handler))
                        {

                            AddHeader(httpClient, header);
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => { return true; };
                            var response = httpClient.GetAsync(link).Result;
                            var r = response.Content.ReadAsByteArrayAsync().Result;
                            Console.WriteLine("Len " + r.Length);
                          //  Console.WriteLine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6) + "\\log.txt");
                          // System.IO.File.AppendAllText(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6)+"\\log.txt", r.Length +"byte  "+ link+Environment.NewLine);

                            if (r.Length > 0)
                            {
                                output.ContentLength64 = r.Length;
                                output.OutputStream.Write(r, 0, r.Length);

                            }

                        }
                    }
                }
             
                     
            }
            catch (Exception ex)
            {
                Console.WriteLine("Er getbyte" + ex.ToString());
                System.IO.File.AppendAllText(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6) + "\\log.txt", ex.Message+" Error " + link + Environment.NewLine);

                Log.Error(m => m("HttpUtility->GetRequest: {0}", ex.Message));
                
                //output.OutputStream.Close();
                  using (var writer = new System.IO.StreamWriter(output.OutputStream))
                  {
                      writer.Write(ex.ToString());
                  }
 
            }
        }
        public static string GetRequest(string link, Dictionary<string, string> header = null, bool verbose = false, bool databyte=false, bool autoredirect=true) {
            try {
                _clearCookies = false;
                if (header != null) {
                    foreach (var entry in header) {
                        Log.Info(entry.ToString());
                        if (entry.Key == "Cookie") {
                            _clearCookies = true;
                            CookieContainer.SetCookies(new Uri(link), entry.Value.Replace(";", ","));
                        }
                    }
                }
                using (var handler = new HttpClientHandler(){AllowAutoRedirect = autoredirect }) {
                  
                    SetHandler(handler);
                    using (var httpClient = new HttpClient(handler)) {
                        AddHeader(httpClient, header);
                        Console.WriteLine("Get "+link);
                        HttpResponseMessage response=null;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        ServicePointManager.ServerCertificateValidationCallback +=(sender, cert, chain, sslPolicyErrors) => { return true; };
                            response = httpClient.GetAsync(link).Result;
                       
                        if (_clearCookies) {
                            var cookies = handler.CookieContainer.GetCookies(new Uri(link));
                            foreach (Cookie co in cookies) {
                                co.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                            }
                        }
                        Log.Info("return get headers=" + verbose);
                        //return response.ToString();
                        Console.WriteLine("HEADS");
                       
                        if (verbose) {
                            string sh = "";
                            try
                            {
                                var headers = response.Headers.Concat(response.Content.Headers);
                                
                                foreach (var i in headers)
                                {
                                    foreach (var j in i.Value)
                                    {
                                        Console.WriteLine(i.Key + ": " + j);
                                        sh += i.Key + ": " + j + "\n";
                                    }

                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Err get headers: " + e.ToString());
                            }
                            return string.Format("{0}\n{1}", sh, ReadContext(response.Content));
                        } else {
                            return ReadContext(response.Content, databyte);
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine("HttpUtility->GetRequest: " + ex.ToString());
                Log.Error(m => m("HttpUtility->GetRequest: {0}", ex.Message));
                return ex.Message;
            }
        }

        public static string PostRequest(string link, string data,
                                         Dictionary<string, string> header = null, bool verbose = false, bool autoredirect = true) {
            try {
                _clearCookies = false;
                if (header != null) {
                    foreach (var entry in header) {
                        Log.Info(entry.ToString());
                        if (entry.Key == "Cookie") {
                            _clearCookies = true;
                            CookieContainer.SetCookies(new Uri(link), entry.Value.Replace(";", ","));
                        }
                    }
                }

                using (var handler = new HttpClientHandler() { AllowAutoRedirect = autoredirect }) {
                    SetHandler(handler);
                    using (var httpClient = new HttpClient(handler)) {
                        AddHeader(httpClient, header);

                        //HttpContent content = new FormUrlEncodedContent(data);
                        StringContent queryString = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => { return true; };
                        var response = httpClient.PostAsync(link, queryString).Result;
                        if (_clearCookies) {
                            var cookies = handler.CookieContainer.GetCookies(new Uri(link));
                            foreach (Cookie co in cookies) {
                                co.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                            }
                        }
                        Log.Info("return post headers=" + verbose);
                        if (verbose) {
                            var headers = response.Headers.Concat(response.Content.Headers);
                            string sh = "";
                            foreach (var i in headers)
                            {
                                foreach (var j in i.Value)
                                {
                                    Console.WriteLine(i.Key + ": " + j);
                                    sh += i.Key + ": " + j + "\n";
                                }

                            }
                            return string.Format("{0}\n{1}", sh, ReadContext(response.Content));
                        } else {
                            return ReadContext(response.Content);
                        }
                    }
                }
            } catch (Exception ex) {
                Log.Error(m => m("HttpUtility->PostRequest: {0}", ex.Message));
                return ex.Message;
            }
        }

        private static void SetHandler(HttpClientHandler handler) {
            handler.Proxy = GlobalProxySelection.GetEmptyWebProxy();
            handler.CookieContainer = CookieContainer;
            handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
        }

        public static void AddHeader(HttpClient httpClient, Dictionary<string, string> header) {

            if (header != null) {
                foreach (var h in header) {
                    try {
                        Console.WriteLine(h.Key + " set=" + h.Value);
                        //if(h.Key== "Content-Type") httpClient.DefaultRequestHeaders.Add(h.Key, h.Value);
                       // else 
                        if (!httpClient.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value)) Console.WriteLine("NOT ADD");
                    } catch (Exception ex) {
                        Log.Debug(m => m("HttpUtility->AddHeader: {0}", ex.Message));
                    }
                }
            } else if (!httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(Settings.Default.UserAgent)) {
                Log.Debug(m => m("HttpUtility->AddUserAgent: {0}", Settings.Default.UserAgent));
            }
        }

        private static string ReadContext(HttpContent context, bool databyte = false) {
                
            try {
                return context.ReadAsStringAsync().Result;
            } catch (Exception e) {
                Log.Info("charset=" + context.Headers.ContentType.CharSet);
                var result = context.ReadAsByteArrayAsync().Result;
                try {
                    var encoding = Encoding.Default;
                    result = Encoding.Convert(encoding, Encoding.Default, result);
                } catch {
                    try {
                        var encoding = Encoding.GetEncoding(context.Headers.ContentType.CharSet);
                        result = Encoding.Convert(encoding, Encoding.Default, result);
                    } catch {
                        try {
                            var encoding = Encoding.UTF8;
                            result = Encoding.Convert(encoding, Encoding.Default, result);
                        } catch {
                            try {
                                var encoding = Encoding.ASCII;
                                result = Encoding.Convert(encoding, Encoding.Default, result);
                            } catch {
                                try {
                                    var encoding = Encoding.Unicode;
                                    result = Encoding.Convert(encoding, Encoding.Default, result);
                                } catch (Exception ex) {
                                    Log.Error(m => m("HttpUtility->ReadContext: {0}", ex.Message));
                                }
                            }
                        }
                    }
                }
                return Encoding.Default.GetString(result);
            }
        }

        public static string QueryParametersToString(NameValueCollection query) {
            if ((query == null) || (query.Count == 0)) {
                return string.Empty;
            }

            var sb = new StringBuilder();

            for (var i = 0; i < query.Count; i++) {
                var key = System.Web.HttpUtility.UrlEncode(query.GetKey(i));
                var keyPrefix = key != null ? key + "=" : string.Empty;
                var values = query.GetValues(i);

                if (sb.Length > 0) {
                    sb.Append('&');
                }

                if ((values == null) || (values.Length == 0)) {
                    sb.Append(keyPrefix);
                } else if (values.Length == 1) {
                    sb.Append(keyPrefix).Append(System.Web.HttpUtility.UrlEncode(values[0]));
                } else {
                    for (var j = 0; j < values.Length; j++) {
                        if (j > 0) {
                            sb.Append('&');
                        }

                        sb.Append(keyPrefix).Append(System.Web.HttpUtility.UrlEncode(values[j]));
                    }
                }
            }

            return sb.ToString();
        }
    }
}