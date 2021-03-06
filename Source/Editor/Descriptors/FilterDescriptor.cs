﻿// Copyright © 2018 Alex Leendertsen

using System.Collections.Generic;
using Editor.Descriptors.Base;
using Editor.Enums;

namespace Editor.Descriptors
{
    public class FilterDescriptor : DescriptorBase
    {
        public static readonly FilterDescriptor DenyAll, LevelMatch, LevelRange, LoggerMatch, Property, String;
        private static readonly IDictionary<string, FilterDescriptor> sDescriptorsByTypeNamespace;

        static FilterDescriptor()
        {
            DenyAll = new FilterDescriptor("Deny All", FilterType.DenyAll, "log4net.Filter.DenyAllFilter");
            LevelMatch = new FilterDescriptor("Level", FilterType.LevelMatch, "log4net.Filter.LevelMatchFilter");
            LevelRange = new FilterDescriptor("Level Range", FilterType.LevelRange, "log4net.Filter.LevelRangeFilter");
            LoggerMatch = new FilterDescriptor("Logger", FilterType.LoggerMatch, "log4net.Filter.LoggerMatchFilter");
            Property = new FilterDescriptor("Property", FilterType.Property, "log4net.Filter.PropertyFilter");
            String = new FilterDescriptor("String", FilterType.String, "log4net.Filter.StringMatchFilter");

            sDescriptorsByTypeNamespace = new Dictionary<string, FilterDescriptor>
            {
                { DenyAll.TypeNamespace, DenyAll },
                { LevelMatch.TypeNamespace, LevelMatch },
                { LevelRange.TypeNamespace, LevelRange },
                { LoggerMatch.TypeNamespace, LoggerMatch },
                { Property.TypeNamespace, Property },
                { String.TypeNamespace, String }
            };
        }

        private FilterDescriptor(string name, FilterType type, string typeNamespace)
            : base(name, "filter", typeNamespace)
        {
            Type = type;
        }

        public static bool TryFindByTypeNamespace(string typeNamespace, out FilterDescriptor filter)
        {
            if (typeNamespace == null)
            {
                filter = null;
                return false;
            }

            return sDescriptorsByTypeNamespace.TryGetValue(typeNamespace, out filter);
        }

        public FilterType Type { get; }
    }
}
