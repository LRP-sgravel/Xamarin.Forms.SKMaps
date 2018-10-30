using Xamarin.Forms.SKMaps.Sample.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xamarin.Forms.SKMaps.Sample.Services
{
    class ResourceLocator : IResourceLocator
    {
        public Assembly ResourcesAssembly { get; }
        public string ResourcesNamespace { get; }
        private Dictionary<string, string> _ResourcePaths { get; }

        public ResourceLocator(string resourcesNamespace, Assembly resourceAssembly)
        {
            if (resourceAssembly == null)
            {
                throw new ArgumentNullException(nameof(resourceAssembly));
            }

            if (string.IsNullOrEmpty(resourcesNamespace))
            {
                throw new ArgumentException("Namespace cannot be empty", nameof(resourcesNamespace));
            }

            ResourcesAssembly = resourceAssembly;
            _ResourcePaths = new Dictionary<string, string>();
            ResourcesNamespace = resourcesNamespace;
        }

        public void RegisterPath(string key, string path)
        {
            _ResourcePaths.Add(key, path);
        }

        public bool HasPath(string key)
        {
            return _ResourcePaths.ContainsKey(key);
        }

        public string GetPath(string key)
        {
            string result = string.Empty;

            if (HasPath(key))
            {
                result = _ResourcePaths[key];
            }

            return result;
        }

        public string GetResourcePath(string pathKey, string resource)
        {
            string[] list = { ResourcesNamespace, GetPath(pathKey), resource };
            string result;

            result = list.Aggregate("", (current, segment) => string.Format("{0}.{1}", current, segment.Replace("/", ".")));

            return result.Remove(0, 1);
        }
    }
}
