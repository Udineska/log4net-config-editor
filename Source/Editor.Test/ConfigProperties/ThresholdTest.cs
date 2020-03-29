﻿// Copyright © 2018 Alex Leendertsen

using System.Linq;
using System.Xml;
using Editor.ConfigProperties;
using Editor.Utilities;
using log4net.Core;
using NUnit.Framework;

namespace Editor.Test.ConfigProperties
{
    [TestFixture]
    public class ThresholdTest
    {
        [SetUp]
        public void SetUp()
        {
            mSut = new Threshold();
        }

        private Threshold mSut;

        [TestCase("<threshold />", "")]
        [TestCase("<threshold value=\"\" />", "")]
        [TestCase("<threshold value=\"ALL\" />", "ALL")]
        [TestCase("<threshold value=\"All\" />", "ALL")]
        [TestCase("<threshold value=\"all\" />", "ALL")]
        public void Load_ShouldLoadTheCorrectValue(string xml, string expected)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<appender>\n" +
                           $"    {xml}\n" +
                           "</appender>");

            mSut.Load(xmlDoc.FirstChild);

            Assert.AreEqual(expected, mSut.SelectedValue);
        }

        [Test]
        public void Ctor_ShouldInitializeNameCorrectly()
        {
            Assert.AreEqual("Threshold:", mSut.Name);
        }

        [Test]
        public void Levels_ShouldBeAllLevels()
        {
            CollectionAssert.AreEqual(new[] { string.Empty }.Concat(Log4NetUtilities.LevelsByName.Keys), mSut.Values);
        }

        [Test]
        public void Save_ShouldNotSave_WhenUnseleted()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement appender = xmlDoc.CreateElement("appender");

            mSut.Save(xmlDoc, appender);

            CollectionAssert.IsEmpty(appender.ChildNodes);
        }

        [Test]
        public void Save_ShouldSaveSelectedLevel()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement appender = xmlDoc.CreateElement("appender");

            mSut.SelectedValue = Level.All.Name;
            mSut.Save(xmlDoc, appender);

            XmlNode thresholdNode = appender.SelectSingleNode("threshold");

            Assert.IsNotNull(thresholdNode);
            Assert.AreEqual(Level.All.Name, thresholdNode.Attributes?["value"].Value);
        }

        [Test]
        public void SelectedLevel_ShouldBeNone()
        {
            Assert.AreEqual(string.Empty, mSut.SelectedValue);
        }

        [Test]
        public void Tooltip_ShouldBeCorrect()
        {
            Assert.AreEqual("All log events with lower level than the threshold level are ignored by the appender.", mSut.ToolTip);
        }
    }
}
