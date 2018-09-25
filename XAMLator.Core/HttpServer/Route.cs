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
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XAMLator.HttpServer
{
	/// <summary>
	/// Route that binds a handler for a resource based in the Http method and the path of the request.
	/// </summary>
	public class Route
	{
		static readonly Regex PathRegex = new Regex(@"\{(?<param>\S+)\}", RegexOptions.Compiled);

		readonly string[] paramNames;

		public Route(string method, string path, Func<HttpRequest, Task<HttpResponse>> action)
		{
			Method = method;
			Path = path;
			Action = action;

			var paramNames = new List<string>();
			Regex = new Regex("^" + PathRegex.Replace(path, m =>
			{
				var p = m.Groups["param"].Value; paramNames.Add(p);
				return String.Format(@"(?<{0}>\S+)", p);
			}) + "$");
			this.paramNames = paramNames.ToArray();
		}

		/// <summary>
		/// Gets the Http method.
		/// </summary>
		/// <value>The Http method.</value>
		string Method { get; set; }

		/// <summary>
		/// Gets the resource path.
		/// </summary>
		/// <value>The path.</value>
		string Path { get; set; }

		/// <summary>
		/// Gets the action that will handle the request.
		/// </summary>
		/// <value>The action.</value>
		Func<HttpRequest, Task<HttpResponse>> Action { get; set; }

		Regex Regex { get; set; }

		/// <summary>
		/// Checks if a request should be handled by this match.
		/// </summary>
		/// <returns><c>true</c>, if there is match, <c>false</c> otherwise.</returns>
		/// <param name="request">Request.</param>
		public bool IsMatch(HttpRequest request)
		{
			return request.HttpMethod == Method && Regex.IsMatch(request.Url.AbsolutePath);
		}

		/// <summary>
		/// Calls the resource handler registered for this route.
		/// </summary>
		/// <returns>The response.</returns>
		/// <param name="request">The request.</param>
		public Task<HttpResponse> Invoke(HttpRequest request)
		{
			var match = Regex.Match(request.Url.AbsolutePath);
			var urlParameters = paramNames.ToDictionary(k => k, k => match.Groups[k].Value);
			//NameValueCollection queryParameters = HttpUtility.ParseQueryString(request.Url.Query);
			NameValueCollection queryParameters = new NameValueCollection();

			var parameters = request.Parameters as IDictionary<string, Object>;
			foreach (var kv in urlParameters)
			{
				parameters.Add(kv.Key, kv.Value);
			}
			foreach (String key in queryParameters.AllKeys)
			{
				parameters.Add(key, queryParameters[key]);
			}
			return Action.Invoke(request);
		}
	}
}
