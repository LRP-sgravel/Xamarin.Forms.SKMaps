using Xamarin.Forms.SKMaps.Sample.Services.Interface;
using MvvmCross;
using System;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.SKMaps.Sample.Forms.Markup
{
    [ContentProperty("Source")]
    public class EmbeddedImageExtension : IMarkupExtension
    {
        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            ImageSource result = null;

            if (Source != null)
            {
                IResourceLocator resLocator = Mvx.Resolve<IResourceLocator>();

                if (resLocator == null || resLocator.ResourcesAssembly == null || !resLocator.HasPath(ResourceKeys.ImagesKey))
                {
                    Debug.WriteLine("Resource info not set for images");
                }
                else
                {
                    string resourceFullName = resLocator.GetResourcePath(ResourceKeys.ImagesKey, Source);

#if DEBUG
                    string[] assemblyResourceNames = resLocator.ResourcesAssembly.GetManifestResourceNames();

                    if (!assemblyResourceNames.Contains(resourceFullName))
                    {
                        Debug.WriteLine($"Trying to bind image from resource {resourceFullName}, but it wasn't found in asembly {resLocator.ResourcesAssembly.FullName}");
                    }
#endif

                    try
                    {
                        result = ImageSource.FromResource(resourceFullName, resLocator.ResourcesAssembly);
                    }
                    catch (Exception)
                    {
                        // Simply return null for unfound resources
                    }
                }
            }

            return result;
        }
    }
}
