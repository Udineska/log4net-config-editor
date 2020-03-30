﻿// Copyright © 2020 Alex Leendertsen

using System.Linq;
using System.Windows;
using System.Xml;
using Editor.ConfigProperties.Base;
using Editor.Descriptors;
using Editor.Utilities;

namespace Editor.ConfigProperties
{
    internal class TypeAttribute : MultiValuePropertyBase<AppenderDescriptor>
    {
        private const double WidthValue = 150;

        public TypeAttribute()
            : base(GridLength.Auto, "Type:", AppenderDescriptor.All, WidthValue)
        {
            SelectedValue = AppenderDescriptor.All.First();
        }

        public TypeAttribute(AppenderDescriptor descriptor)
            : base(GridLength.Auto, "Type:", AppenderDescriptor.All, WidthValue)
        {
            //Will be overwritten when/if load is called
            SelectedValue = descriptor;
        }

        public override void Load(XmlNode originalNode)
        {
            if (AppenderDescriptor.TryFindByTypeNamespace(originalNode.Attributes[Log4NetXmlConstants.Type].Value, out AppenderDescriptor descriptor))
            {
                SelectedValue = descriptor;
            }
        }

        public override void Save(XmlDocument xmlDoc, XmlNode newNode)
        {
            newNode.AppendAttribute(xmlDoc, Log4NetXmlConstants.Type, SelectedValue.TypeNamespace);
        }
    }
}
