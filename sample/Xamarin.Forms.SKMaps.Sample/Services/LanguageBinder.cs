using MvvmCross;
using MvvmCross.Localization;
using MvvmCross.Plugin.JsonLocalization;
using System;
using System.Diagnostics;

namespace Xamarin.Forms.SKMaps.Sample.Services
{
    public class LanguageBinder : IMvxLanguageBinder
    {
        private IMvxTextProvider _textProvider;
        private IMvxTextProvider _TextProvider
        {
            get
            {
                if (_textProvider == null)
                    _textProvider = Mvx.IoCProvider.Resolve<IMvxTextProvider>();

                return _textProvider;
            }
        }

        private bool _ThrowWhenNotFound { get; set; }

        public string TypeName { get; set; }

        private TextProviderBuilder _textBuilder;
        private TextProviderBuilder _TextBuilder
        {
            get
            {
                if (_textBuilder == null)
                {
                    _textBuilder = Mvx.IoCProvider.Resolve<IMvxTextProviderBuilder>() as TextProviderBuilder;
                }

                return _textBuilder;
            }
        }

        private readonly string _namespaceName;

        public LanguageBinder(string namespaceName, string typeName, bool throwWhenNotFound = true)
        {
            _namespaceName = namespaceName;
            _ThrowWhenNotFound = throwWhenNotFound;
            TypeName = typeName;
        }

        public string GetText(string entryKey)
        {
            return GetText(_namespaceName, TypeName, entryKey);
        }

        public string GetText(string entryKey, params object[] args)
        {
            return string.Format(GetText(entryKey), args);
        }

        private string GetText(string namespaceKey, string typeKey, string entryKey)
        {
            string result = entryKey;
            Exception error = null;

            try
            {
                result = _TextProvider.GetText(namespaceKey, typeKey, entryKey);
            }
            catch (Exception e)
            {
                error = e;
            }

            if (error != null)
            {
                try
                {
                    if (_TextBuilder != null)
                    {
                        result = _TextProvider.GetText(namespaceKey, _TextBuilder._DefaultTypeKey, entryKey);

                        // Used fallback
                        error = null;
                        Debug.WriteLine($"Used default type key for {namespaceKey}:{typeKey}:{entryKey}");
                    }
                }
                catch (Exception e)
                {
                    error = new AggregateException(error, e);
                }
            }

            if (error != null)
            {
                if (_ThrowWhenNotFound)
                {
                    throw error;
                }
                else
                {
                    Debug.WriteLine($"Cannot find text for key {namespaceKey}:{typeKey}:{entryKey}");
                }
            }

            return result;
        }
    }
}
