using System.Reflection;

namespace FormsSkiaBikeTracker.Services.Interface
{
    public interface IResourceLocator
    {
        Assembly ResourcesAssembly { get; }
        string ResourcesNamespace { get; }

        string GetPath(string key);
        string GetResourcePath(string pathKey, string resource);
        bool HasPath(string key);
        void RegisterPath(string key, string path);
    }
}
