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
    public class HttpServerLite {
        readonly HttpListener _listener = new HttpListener();
        readonly Func<VsixSend, Task<VsixResponse>> _responce;

        public HttpServerLite(string prefix, Func<VsixSend, Task<VsixResponse>> responceMethod) {
            _listener.Prefixes.Add(prefix);
            _responce = responceMethod;
            _listener.Start();
        }

        public void Start() => Task.Factory.StartNew(async () => await ServerWorkerAsync(), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        async Task ServerWorkerAsync() {
            try {
                while (_listener.IsListening) {
                    await Task.Run<Task>(async () => {
                        var ctx = _listener.GetContext() as HttpListenerContext;
                        try {
                            if (ctx == null)
                                return;
                            using (var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding)) {
                                var request = reader.ReadToEnd();
                                var obj = JsonConvert.DeserializeObject<VsixSend>(request);
                                var content = JsonConvert.SerializeObject(await _responce(obj));
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
                    });
                }
            }
            catch { }
        }

        public void Stop() {
            _listener.Stop();
            _listener.Close();
        }
    }
}
