﻿// Copyright © 2018 Alex Leendertsen

using System.Xml;
using Editor.ConfigProperties;
using NUnit.Framework;

namespace Editor.Test.ConfigProperties
{
    [TestFixture]
    public class ValueTest
    {
        [SetUp]
        public void SetUp()
        {
            mSut = new Value();
        }

        private Value mSut;

        [TestCase(null)]
        [TestCase("")]
        [TestCase("value=\"\"")]
        public void Load_ShouldNotLoadValue(string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml($"<appender name=\"ColoredConsoleAppender\" {xml}>\r\n" +
                           "</appender>");

            mSut.Load(xmlDoc.FirstChild);

            Assert.IsNull(mSut.Value);
        }

        [TestCase("value=\"\"")]
        [TestCase("")]
        public void Load_ShouldMaintainValue_FromCtor(string value)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml($"<appender name=\"ColoredConsoleAppender\" {value}>\r\n" +
                           "</appender>");

            mSut.Load(xmlDoc.FirstChild);

            Assert.IsNull(mSut.Value);
        }

        [TestCase(null)]
        [TestCase("")]
        public void Save_ShouldNotSaveValueToAttribute_WhenValueIsNullOrEmpty(string value)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement appender = xmlDoc.CreateElement("appender");

            mSut.Value = value;

            mSut.Save(xmlDoc, appender);

            Assert.IsNull(appender.Attributes["value"]);
        }

        [Test]
        public void Load_ShouldLoadCorrectValue()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<appender name=\"ColoredConsoleAppender\" value=\"log4net.Appender.ColoredConsoleAppender\">\r\n" +
                           "</appender>");

            mSut.Load(xmlDoc.FirstChild);

            Assert.AreEqual("log4net.Appender.ColoredConsoleAppender", mSut.Value);
        }

        [Test]
        public void Name_ShouldBeCorrect_RegularCtor()
        {
            Assert.AreEqual("Value:", mSut.Name);
        }

        [Test]
        public void Save_ShouldSaveValueToAttribute()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement appender = xmlDoc.CreateElement("appender");

            const string value = "whatev";
            mSut.Value = value;

            mSut.Save(xmlDoc, appender);

            Assert.AreEqual(value, appender.Attributes["value"].Value);
        }

        [Test]
        public void Value_ShouldBeCorrect_RegularCtor()
        {
            Assert.IsNull(mSut.Value);
        }
    }
}
