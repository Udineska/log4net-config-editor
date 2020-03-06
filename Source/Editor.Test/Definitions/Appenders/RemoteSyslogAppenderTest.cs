﻿// Copyright © 2019 Alex Leendertsen

using System.Linq;
using System.Xml;
using Editor.ConfigProperties;
using Editor.ConfigProperties.Base;
using Editor.Definitions.Appenders;
using Editor.Descriptors;
using Editor.Interfaces;
using NSubstitute;
using NUnit.Framework;
using static log4net.Appender.RemoteSyslogAppender;

namespace Editor.Test.Definitions.Appenders
{
    [TestFixture]
    public class RemoteSyslogAppenderTest
    {
        private RemoteSyslogAppender mSut;

        [SetUp]
        public void SetUp()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement log4NetNode = xmlDoc.CreateElement("log4net");

            IElementConfiguration configuration = Substitute.For<IElementConfiguration>();
            configuration.ConfigXml.Returns(xmlDoc);
            configuration.Log4NetNode.Returns(log4NetNode);

            mSut = new RemoteSyslogAppender(configuration);
        }

        [Test]
        public void Name_ShouldReturnCorrectValue()
        {
            Assert.AreEqual("Remote Syslog Appender", mSut.Name);
        }

        [Test]
        public void Icon_ShouldReturnCorrectValue()
        {
            Assert.AreEqual("pack://application:,,,/Editor;component/Images/list-add.png", mSut.Icon);
        }

        [Test]
        public void Descriptor_ShouldReturnCorrectValue()
        {
            Assert.AreEqual(AppenderDescriptor.RemoteSyslog, mSut.Descriptor);
        }

        [Test]
        public void Initialize_ShouldAddDefaultProperties()
        {
            mSut.Initialize();

            TestHelpers.AssertAppenderSkeletonPropertiesExist(mSut.Properties);
        }

        [Test]
        public void Initialize_ShouldAddUdpProperties()
        {
            mSut.Initialize();

            mSut.Properties.Single(p => p.GetType() == typeof(RemoteAddress));
            Assert.AreEqual(2, mSut.Properties.Count(p => p.GetType() == typeof(Port)));
        }

        [Test]
        public void Initialize_ShouldAddRemoteSyslogProperties()
        {
            mSut.Initialize();

            mSut.Properties.Single(p => p.GetType() == typeof(EnumProperty<SyslogFacility>));
            mSut.Properties.Single(p => p.GetType() == typeof(RemoteIdentity));
        }

        [Test]
        public void Initialize_ShouldAddCorrectNumberOfProperties()
        {
            mSut.Initialize();

            Assert.AreEqual(12, mSut.Properties.Count);
        }
    }
}
