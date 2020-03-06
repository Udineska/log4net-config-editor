﻿// Copyright © 2018 Alex Leendertsen

using System.Windows;
using System.Xml;
using Editor.ConfigProperties.Base;
using Editor.Utilities;

namespace Editor.ConfigProperties
{
    internal class MinLevel : LevelPropertyBase
    {
        private const string LevelMinName = "levelMin";

        public MinLevel()
            : base(GridLength.Auto, "Min Level:", true)
        {
        }

        public override void Load(XmlNode originalNode)
        {
            TryLoadSelectedLevel(originalNode.GetValueAttributeValueFromChildElement(LevelMinName));
        }

        public override void Save(XmlDocument xmlDoc, XmlNode newNode)
        {
            if (!string.IsNullOrEmpty(SelectedValue))
            {
                xmlDoc.CreateElementWithValueAttribute(LevelMinName, SelectedValue).AppendTo(newNode);
            }
        }
    }
}
