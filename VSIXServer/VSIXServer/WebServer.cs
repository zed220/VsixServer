using CommonLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSIXServer {
    public class WebServer {
        readonly HttpListener _listener = new HttpListener();
        readonly Func<VsixSend, VsixResponse> _responce;

        public WebServer(string prefix, Func<VsixSend, VsixResponse> responce) {
            _listener.Prefixes.Add(prefix);
            _responce = responce;
            _listener.Start();
        }

        public void Run() {
            ThreadPool.QueueUserWorkItem(o => {
                try {
                    while (_listener.IsListening) {
                        ThreadPool.QueueUserWorkItem(c => {
                            var ctx = c as HttpListenerContext;
                            try {
                                if (ctx == null) {
                                    return;
                                }

                                using (var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding)) {
                                    var request = reader.ReadToEnd();
                                    var obj = JsonConvert.DeserializeObject<VsixSend>(request);
                                    var content = JsonConvert.SerializeObject(_responce(obj));
                                    ctx.Response.ContentLength64 = content.Length;
                                    ctx.Response.ContentEncoding = Encoding.UTF8;
                                    ctx.Response.ContentType = "application/json";
                                    ctx.Response.OutputStream.Write(Encoding.UTF8.GetBytes(content), 0, content.Length);
                                }
                            }
                            catch {
                            }
                            finally {
                                if (ctx != null)
                                    ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch { }
            });
        }

        public void Stop() {
            _listener.Stop();
            _listener.Close();
        }
    }
}
