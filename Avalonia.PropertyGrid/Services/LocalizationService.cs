﻿using Avalonia.PropertyGrid.Localization;
using Avalonia.PropertyGrid.Model.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.PropertyGrid.Services
{
    /// <summary>
    /// Class LocalizationService.
    /// </summary>
    public static class LocalizationService
    {
        /// <summary>
        /// The default
        /// </summary>
        public readonly static ILocalizationService Default = new JsonAssetLocalizationService();
    }
}