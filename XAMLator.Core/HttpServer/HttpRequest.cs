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
using System.Dynamic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace XAMLator.HttpServer
{
    /// <summary>
    /// Http request.
    /// </summary>
    public class HttpRequest
    {
        public HttpRequest(string httpMethod, Uri url, Stream stream, Dictionary<string, IEnumerable<string>> headers)
        {
            HttpMethod = httpMethod;
            Url = url;
            Body = stream;
            Parameters = new ExpandoObject();
            Headers = headers;
        }

        /// <summary>
        /// Gets the Http method.
        /// </summary>
        /// <value>The Http method.</value>
        public string HttpMethod { get; private set; }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <value>The URL.</value>
        public Uri Url { get; private set; }

        public Stream Body { get; private set; }

        /// <summary>
        /// Gets the query or url parameters of the request.
        /// </summary>
        /// <value>The parameters.</value>
        public dynamic Parameters { get; private set; }

        /// <summary>
        /// Gets the headers of the request.
        /// </summary>
        /// <value>The headers.</value>
        public Dictionary<string, IEnumerable<string>> Headers { get; private set; }
    }

    public class JsonHttpRequest : HttpRequest
    {
        public JsonHttpRequest(string httpMethod, Uri url, Stream stream, Dictionary<string,
                                IEnumerable<string>> headers, string json) :
        base(httpMethod, url, stream, headers)
        {
            object data = Serializer.DeserializeJson(json);
            if (data is JObject jData)
            {
                Data = jData.ToObject<Dictionary<string, string>>();
            }
        }

        /// <summary>
        /// Gets the data of the request, deseralized from the request body
        /// </summary>
        /// <value>The data.</value>
        public object Data { get; private set; }
    }
}
