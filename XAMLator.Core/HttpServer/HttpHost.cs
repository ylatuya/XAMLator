//
//  Copyright (C) 2018 Fluendo S.A.
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace XAMLator.HttpServer
{
    /// <summary>
    /// Http host that handles HTTP request and dispatch them to a <see cref="IRequestProcessor"/>.
    /// </summary>
    public class HttpHost
    {
        readonly IRequestProcessor requestProcessor;
        readonly string baseUri;
        HttpListener listener;

        public event EventHandler ClientConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:VAS.Services.HttpServer.HttpHost"/> class.
        /// </summary>
        /// <param name="baseUri">Base URI.</param>
        /// <param name="requestProcessor">Request processor.</param>
        public HttpHost(string baseUri, IRequestProcessor requestProcessor)
        {
            this.baseUri = baseUri;
            this.requestProcessor = requestProcessor;
        }

        public static async Task<HttpHost> StartServer(IRequestProcessor processor, int port, int portsRange)
        {
            HttpHost host = null;

            for (int i = 0; i < portsRange; i++)
            {
                try
                {
                    host = new HttpHost($"http://+:{port}/", processor);
                    await host.StartListening();
                    break;
                }
                catch (Exception ex)
                {
                    if (ex is SocketException || ex is HttpListenerException)
                    {
                        host = null;
                        port++;
                        Log.Exception(ex);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return host;
        }

        /// <summary>
        /// Start the host.
        /// </summary>
        public Task<bool> StartListening()
        {
            var taskCompletion = new TaskCompletionSource<bool>();
            Task.Factory.StartNew(() => Run(taskCompletion), TaskCreationOptions.LongRunning);
            return taskCompletion.Task;
        }

        /// <summary>
        /// Stop the host.
        /// </summary>
        public void StopListening()
        {
            listener.Stop();
        }

        async Task Run(TaskCompletionSource<bool> tcs)
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(baseUri);
                listener.Start();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            Log.Information($"Http server listening at {baseUri}");
            tcs.SetResult(true);

            // Loop
            for (; ; )
            {
                var c = await listener.GetContextAsync().ConfigureAwait(false);
                try
                {
                    ClientConnected?.Invoke(this, null);
                    await ProcessRequest(c).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
        }

        async Task ProcessRequest(HttpListenerContext context)
        {
            HttpResponse response;
            try
            {
                Log.Information($"Handling incomming request {context.Request.Url}");
                var request = await GetRequest(context.Request);
                response = await requestProcessor.HandleRequest(request);
                Log.Information($"Request handled with response {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Log.Error("Internal server error");
                Log.Exception(ex);
                response = new JsonHttpResponse { StatusCode = HttpStatusCode.InternalServerError };
            }
            try
            {
                await WriteResponse(response, context.Response);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        async Task WriteResponse(HttpResponse response, HttpListenerResponse httpListenerResponse)
        {
            httpListenerResponse.StatusCode = (int)response.StatusCode;
            httpListenerResponse.ContentType = response.ContentType;

            foreach (var header in response.Headers)
            {
                httpListenerResponse.AddHeader(header.Key, header.Value);
            }

            using (var output = httpListenerResponse.OutputStream)
            {
                await response.Stream(output);
            }
        }

        async Task<HttpRequest> GetRequest(HttpListenerRequest request)
        {
            var headers = request.Headers.AllKeys.ToDictionary<string, string, IEnumerable<string>>(
                key => key, request.Headers.GetValues);
            if (request.ContentType == "application/json")
            {
                StreamReader sr = new StreamReader(request.InputStream, Encoding.UTF8);
                string json = await sr.ReadToEndAsync();
                return new JsonHttpRequest(request.HttpMethod, request.Url, request.InputStream, headers, json);
            }

            return new HttpRequest(request.HttpMethod, request.Url, request.InputStream, headers);
        }
    }
}
