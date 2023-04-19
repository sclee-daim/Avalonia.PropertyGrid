﻿using Avalonia.PropertyGrid.Controls;
using Avalonia.PropertyGrid.Controls.Implements;
using Avalonia.PropertyGrid.Model.ComponentModel;
using Avalonia.PropertyGrid.Model.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalonia.PropertyGrid.ViewModels
{
    internal class PropertyGridViewModel : ReactiveObject
    {
        public IPropertyGridFilterPattern FilterPattern { get; set; } = new PropertyGridFilterPattern();

        object _SelectedObject;
        public object SelectedObject
        {
            get => _SelectedObject;
            set => this.RaiseAndSetIfChanged(ref _SelectedObject, value);
        }

        bool _ShowCategory = true;

        public bool ShowCategory
        {
            get => _ShowCategory;
            set => this.RaiseAndSetIfChanged(ref _ShowCategory, value);
        }

        CheckedMaskModel _CategoryFilter;
        public CheckedMaskModel CategoryFilter
        {
            get => _CategoryFilter;
            set => this.RaiseAndSetIfChanged(ref _CategoryFilter, value);
        }

        public List<PropertyDescriptor> AllProperties { get; private set; } = new List<PropertyDescriptor>();

        public List<PropertyDescriptor> FilteredProperties { get; private set; } = new List<PropertyDescriptor>();

        public SortedList<string, List<PropertyDescriptor>> Categories { get; private set; } = new SortedList<string, List<PropertyDescriptor>>();

        /// <summary>
        /// Occurs when [filter changed].
        /// </summary>
        public event EventHandler PropertyDescriptorChanged;

        public PropertyGridViewModel()
        {
            FilterPattern.PropertyChanged += OnPropertyChanged;
            PropertyChanged += OnPropertyChanged;
        }

        public void Clear()
        {
            AllProperties.Clear();
            FilteredProperties.Clear();
            Categories.Clear();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(SelectedObject))
            {
                RefreshProperties();
            }
            else if(sender == FilterPattern)
            {
                FilterProperties();
            }
        }

        public void RefreshProperties()
        {
            Clear();

            if(_SelectedObject == null)
            {
                return;
            }

            PropertyDescriptorBuilder builder = new PropertyDescriptorBuilder(_SelectedObject);
            AllProperties.AddRange(builder.GetProperties().Cast<PropertyDescriptor>());

            HashSet<string> categories = new HashSet<string>();
            foreach(var property in AllProperties)
            {
                string category = string.IsNullOrEmpty(property.Category) ? _SelectedObject.GetType().Name : property.Category;

                if(!categories.Contains(category))
                {
                    categories.Add(category);
                }
            }

            CategoryFilter = new CheckedMaskModel(categories.OrderBy(x=>x), Controls.PropertyGrid.LocalizationService["All"]);
            CategoryFilter.CheckChanged += OnCategoryFilterChanged;

            FilterProperties();
        }

        private void OnCategoryFilterChanged(object sender, EventArgs e)
        {
            FilterProperties();   
        }

        public void FilterProperties()
        {
            FilteredProperties.Clear();
            Categories.Clear();

            foreach (var property in AllProperties)
            {
                if (!FilterPattern.Match(property, _SelectedObject))
                {
                    continue;
                }

                FilteredProperties.Add(property);

                string category = string.IsNullOrEmpty(property.Category) ? _SelectedObject.GetType().Name : property.Category;

                if(CategoryFilter == null || CategoryFilter.IsChecked(category))
                {
                    if (!Categories.TryGetValue(category, out var list))
                    {
                        list = new List<PropertyDescriptor> { property };
                        Categories.Add(category, list);
                    }
                    else
                    {
                        list.Add(property);
                    }
                }
            }

            PropertyDescriptorChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
