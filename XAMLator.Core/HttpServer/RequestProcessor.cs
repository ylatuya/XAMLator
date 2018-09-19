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
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace XAMLator.HttpServer
{
    /// <summary>
    /// Process request based on their URL and the http method.
    /// </summary>
    public abstract class RequestProcessor : IRequestProcessor
    {
        const string GET = "GET";
        const string POST = "POST";
        const string PUT = "PUT";

        readonly List<Route> routes = new List<Route>();

        public async Task<HttpResponse> HandleRequest(HttpRequest request)
        {
            var route = routes.FirstOrDefault(r => r.IsMatch(request));
            if (route == null)
            {
                return new JsonHttpResponse { StatusCode = HttpStatusCode.NotFound };
            }
            return await route.Invoke(request);
        }

        /// <summary>
        /// Callbacks router for Get requests
        /// </summary>
        protected RouteBuilder Get { get { return new RouteBuilder(GET, this); } }

        /// <summary>
        /// Callbacks router for Post requests
        /// </summary>
        protected RouteBuilder Post { get { return new RouteBuilder(POST, this); } }

        /// <summary>
        /// Callbacks router for Put requests
        /// </summary>
        protected RouteBuilder Put { get { return new RouteBuilder(PUT, this); } }

        protected class RouteBuilder
        {
            readonly string method;
            readonly RequestProcessor requestProcessor;

            public RouteBuilder(string method, RequestProcessor requestProcessor)
            {
                this.method = method;
                this.requestProcessor = requestProcessor;
            }

            public Func<HttpRequest, Task<HttpResponse>> this[string path]
            {
                set
                {
                    AddRoute(path, value);
                }
            }

            void AddRoute(string path, Func<HttpRequest, Task<HttpResponse>> value)
            {
                requestProcessor.routes.Add(new Route(method, path, value));
            }
        }
    }
}
