﻿// Copyright © 2018 Alex Leendertsen

using System.Windows;
using System.Xml;
using Editor.ConfigProperties.Base;
using Editor.Utilities;

namespace Editor.ConfigProperties
{
    internal class LevelToMatch : LevelPropertyBase
    {
        private const string LevelMatchName = "levelToMatch";

        public LevelToMatch()
            : base(GridLength.Auto, "Level to Match:")
        {
        }

        public override void Load(XmlNode originalNode)
        {
            TryLoadSelectedLevel(originalNode.GetValueAttributeValueFromChildElement(LevelMatchName));
        }

        public override void Save(XmlDocument xmlDoc, XmlNode newNode)
        {
            xmlDoc.CreateElementWithValueAttribute(LevelMatchName, SelectedValue).AppendTo(newNode);
        }
    }
}
