//-----------------------------------------------------------------------------
// <auto-generated>
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------------
using System;
using System.Net;

namespace Unity.Services.Authentication.Shared
{
    /// <summary>
    /// Provides a non-generic contract for the ApiResponse wrapper.
    /// </summary>
    internal interface IApiResponse
    {
        /// <summary>
        /// The content of this response
        /// </summary>
        object Content { get; }

        /// <summary>
        /// Gets or sets the status code (HTTP status code)
        /// </summary>
        /// <value>The status code.</value>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets or sets the HTTP headers
        /// </summary>
        /// <value>HTTP headers</value>
        Multimap<string, string> Headers { get; }

        /// <summary>
        /// Gets or sets any error text defined by the calling client.
        /// </summary>
        string ErrorText { get; }

        /// <summary>
        /// The raw content of this response
        /// </summary>
        string RawContent { get; }
    }

    /// <summary>
    /// API Response
    /// </summary>
    internal class ApiResponse : IApiResponse
    {
        /// <summary>
        /// Gets or sets the status code (HTTP status code)
        /// </summary>
        /// <value>The status code.</value>
        public HttpStatusCode StatusCode { get; internal set; }

        /// <summary>
        /// Gets or sets the HTTP headers
        /// </summary>
        /// <value>HTTP headers</value>
        public Multimap<string, string> Headers { get; internal set; }

        /// <summary>
        /// Gets or sets any error text defined by the calling client.
        /// </summary>
        public string ErrorText { get; internal set; }

        /// <summary>
        /// The raw content
        /// </summary>
        public string RawContent { get; internal set; }

        /// <summary>
        /// The data type of <see cref="Content"/>
        /// </summary>
        public object Content => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse" /> class.
        /// </summary>
        public ApiResponse()
        {
        }
    }

    /// <summary>
    /// API Response
    /// </summary>
    internal class ApiResponse<T> : IApiResponse
    {
        /// <summary>
        /// Gets or sets the status code (HTTP status code)
        /// </summary>
        /// <value>The status code.</value>
        public HttpStatusCode StatusCode { get; internal set; }

        /// <summary>
        /// Gets or sets the HTTP headers
        /// </summary>
        /// <value>HTTP headers</value>
        public Multimap<string, string> Headers { get; internal set; }

        /// <summary>
        /// Gets or sets the data (parsed HTTP body)
        /// </summary>
        /// <value>The data.</value>
        public T Data { get; internal set; }

        /// <summary>
        /// Gets or sets any error text defined by the calling client.
        /// </summary>
        public string ErrorText { get; internal set; }

        /// <summary>
        /// The raw content
        /// </summary>
        public string RawContent { get; internal set; }

        /// <summary>
        /// The data type of <see cref="Content"/>
        /// </summary>
        public object Content
        {
            get { return Data; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse{T}" /> class.
        /// </summary>
        public ApiResponse()
        {
        }
    }
}
