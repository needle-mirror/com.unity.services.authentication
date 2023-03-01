//-----------------------------------------------------------------------------
// <auto-generated>
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Unity.Services.Authentication.Shared
{
    /// <summary>
    /// A URI builder
    /// </summary>
    internal class ApiRequestPathBuilder
    {
        private string _baseUrl;
        private string _path;
        private string _query = "?";
        
        public ApiRequestPathBuilder(string baseUrl, string path)
        {
            _baseUrl = baseUrl;
            _path = path;
        }

        public void AddPathParameters(Dictionary<string, string> parameters)
        {
            foreach (var parameter in parameters)
            {
                _path = _path.Replace("{" + parameter.Key + "}", Uri.EscapeDataString(parameter.Value));
            }
        }

        public void AddQueryParameters(Multimap<string, string> parameters)
        {
            foreach (var parameter in parameters)
            {
                foreach (var value in parameter.Value)
                {
                    _query = _query + parameter.Key + "=" + Uri.EscapeDataString(value) + "&";
                }
            }
        }

        public string GetFullUri()
        {
            return _baseUrl + _path + _query.Substring(0, _query.Length - 1);
        }
    }
}
