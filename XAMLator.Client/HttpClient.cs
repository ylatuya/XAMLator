using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace XAMLator.Client
{
    /// <summary>
    /// Http client to communicate the XAMLator server.
    /// </summary>
    public class HttpClient
    {
        string baseUri;
        System.Net.Http.HttpClient client;

        public HttpClient(string uri)
        {
            client = new System.Net.Http.HttpClient();
            baseUri = uri;
        }

        /// <summary>
        /// Request to preview a xaml document.
        /// </summary>
        /// <returns>The evaluation response.</returns>
        /// <param name="doc">The XAML document.</param>
        public async Task<EvalResponse> PreviewXaml(XAMLDocument doc)
        {
            EvalRequest request = new EvalRequest { Xaml = doc.XAML, XamlType = doc.Type };
            var content = new StringContent(Serializer.SerializeJson(request), Encoding.UTF8);
            var response = await client.PostAsync(baseUri + "/xaml", content);
            if (response.IsSuccessStatusCode)
            {
                return Serializer.DeserializeJson<EvalResponse>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                return null;
            }
        }
    }
}
