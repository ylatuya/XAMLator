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
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace XAMLator.HttpServer
{
    /// <summary>
    /// Http response.
    /// </summary>
    public abstract class HttpResponse
    {
        public HttpResponse()
        {
            Stream = WriteResponse;
            Headers = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the response data.
        /// </summary>
        /// <value>The response data.</value>
        public Func<Stream, Task> Stream { get; set; }

        /// <summary>
        /// Gets or sets the content-type of the response
        /// </summary>
        /// <value>The content-type.</value>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the status code of the response.
        /// </summary>
        /// <value>The status code.</value>
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        /// <summary>
        /// Gets the headers of the request.
        /// </summary>
        /// <value>The headers.</value>
        public Dictionary<string, string> Headers { get; private set; }

        protected abstract Task WriteResponse(Stream stream);
    }

    /// <summary>
    /// JSON Http response.
    /// </summary>
    public class JsonHttpResponse : HttpResponse
    {
        public JsonHttpResponse()
        {
            ContentType = "application/json";
        }

        /// <summary>
        /// Gets the response data.
        /// </summary>
        /// <value>The response data.</value>
        public object Data { get; set; }

        protected override async Task WriteResponse(Stream stream)
        {
            if (Data != null)
            {
                var json = Serializer.SerializeJson(Data);
                using (var sw = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    await sw.WriteAsync(json);
                }
            }
        }
    }
}