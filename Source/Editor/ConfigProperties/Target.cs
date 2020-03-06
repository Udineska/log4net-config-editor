﻿// Copyright © 2018 Alex Leendertsen

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml;
using Editor.ConfigProperties.Base;
using Editor.Utilities;

namespace Editor.ConfigProperties
{
    public class Target : PropertyBase
    {
        private const string ConsoleOut = "Console.Out";
        private const string ConsoleError = "Console.Error";
        private const string TargetName = "target";

        public Target()
            : base(GridLength.Auto)
        {
            Targets = new[] { ConsoleOut, ConsoleError };
            SelectedItem = ConsoleOut;
        }

        public IEnumerable<string> Targets { get; }

        public string SelectedItem { get; set; }

        public override void Load(XmlNode originalNode)
        {
            string value = originalNode.GetValueAttributeValueFromChildElement(TargetName);

            if (!string.IsNullOrEmpty(value) && Targets.Contains(value))
            {
                SelectedItem = value;
            }
        }

        public override void Save(XmlDocument xmlDoc, XmlNode newNode)
        {
            //Target is "Console.Out" by default, so we only need a target element if "Console.Error"
            if (SelectedItem == ConsoleError)
            {
                xmlDoc.CreateElementWithValueAttribute(TargetName, ConsoleError).AppendTo(newNode);
            }
        }
    }
}
