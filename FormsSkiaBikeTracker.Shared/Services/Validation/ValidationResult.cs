// **********************************************************************
// 
//   ValidationResult.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System;
using System.Collections.Generic;

namespace FormsSkiaBikeTracker.Services.Validation
{
    public class ValidationResult : IValidationResult
    {
        public event EventHandler ErrorsChanged;

        private Dictionary<string, string> _Errors { get; } = new Dictionary<string, string>();

        public bool HasErrors => _Errors.Count > 0;

        public void AddError(string propertyName, string errorText)
        {
            _Errors[propertyName] = errorText;
            ErrorsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ClearError(string propertyName)
        {
            if (_Errors.ContainsKey(propertyName))
            {
                _Errors.Remove(propertyName);
                ErrorsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string GetError(string propertyName)
        {
            string result = null;

            if (_Errors.ContainsKey(propertyName))
            {
                result = _Errors[propertyName];
            }

            return result;
        }
    }
}
