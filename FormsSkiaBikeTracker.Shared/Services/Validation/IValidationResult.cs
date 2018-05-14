// **********************************************************************
// 
//   IValidationResult.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System;

namespace FormsSkiaBikeTracker.Services.Validation
{
    public interface IValidationResult
    {
        event EventHandler ErrorsChanged;

        bool HasErrors { get; }

        void AddError(string propertyName, string errorText);
        void ClearError(string propertyName);
        string GetError(string propertyName);
    }
}
