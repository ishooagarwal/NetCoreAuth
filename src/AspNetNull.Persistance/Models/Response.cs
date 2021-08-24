namespace AspNetNull.Persistance.Models
{
    using System.Net;
    using System.Net.Http.Headers;
    using System.Text.Json.Serialization;

    public class Response
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Message { get; set; }
        /// <summary>
        /// gets or sets HttpStatusCode.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public HttpStatusCode HttpStatusCode { get; set; }

        /// <summary>
        /// Gets or sets ResponseHeaders.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public HttpResponseHeaders ResponseHeaders { get; set; }
    }
}
