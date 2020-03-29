﻿// Copyright © 2018 Alex Leendertsen

using System.Xml;
using Editor.ConfigProperties.Base;
using Editor.Interfaces;
using Editor.Utilities;

namespace Editor.ConfigProperties
{
    public class MaximumFileSize : StringValueProperty
    {
        private const string MaximumFileSizeName = "maximumFileSize";
        private const string DefaultMaxFileSize = "10MB";

        public MaximumFileSize()
            : base("Maximum File Size:", MaximumFileSizeName)
        {
            Value = DefaultMaxFileSize;
            ToolTip = "The maximum size that the output file is allowed to reach before being rolled over to backup files. Must be suffixed with \"KB\", \"MB\", or \"GB\".";
        }

        public override bool TryValidate(IMessageBoxService messageBoxService)
        {
            string trimmed = Value.Trim();
            if (!(trimmed.EndsWith("KB") || trimmed.EndsWith("MB") || trimmed.EndsWith("GB")))
            {
                messageBoxService.ShowError("Maximum file size must end with either \"KB\", \"MB\", or \"GB\".");
                return false;
            }

            return base.TryValidate(messageBoxService);
        }

        public override void Save(XmlDocument xmlDoc, XmlNode newNode)
        {
            if (Value != DefaultMaxFileSize)
            {
                xmlDoc.CreateElementWithValueAttribute(MaximumFileSizeName, Value.Trim()).AppendTo(newNode);
            }
        }
    }
}
