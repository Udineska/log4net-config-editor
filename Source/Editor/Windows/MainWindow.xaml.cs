// Copyright © 2018 Alex Leendertsen

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Editor.Definitions.Factory;
using Editor.Descriptors;
using Editor.Descriptors.Base;
using Editor.HistoryManager;
using Editor.Interfaces;
using Editor.Models;
using Editor.Utilities;
using Editor.Windows.SizeLocation;
using log4net.Core;
using Microsoft.Win32;

namespace Editor.Windows
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string UpdateMerge = "Merge";
        private const string UpdateOverwrite = "Overwrite";
        private XmlDocument mConfigXml;
        private XmlNode mLog4NetNode;
        private readonly HistoryManager.HistoryManager mConfigHistoryManager;

        public MainWindow()
            : base("MainWindowPlacement")
        {
            InitializeComponent();

            mConfigHistoryManager = new HistoryManager.HistoryManager("HistoricalConfigs", new SettingManager<string>());

            xAddAppenderButton.ItemsSource = new[]
            {
                AppenderDescriptor.Console,
                AppenderDescriptor.File,
                AppenderDescriptor.RollingFile,
                AppenderDescriptor.EventLog,
                AppenderDescriptor.Async,
                AppenderDescriptor.Forwarding,
                AppenderDescriptor.ManagedColor
            };

            xUpdateComboBox.ItemsSource = new[] { UpdateMerge, UpdateOverwrite };

            xThresholdComboBox.ItemsSource = Log4NetUtilities.LevelsByName.Keys;

            Title = $"log4net Configuration Editor - v{Assembly.GetEntryAssembly().GetName().Version.ToString(3)}";
        }

        private void MainWindowOnLoaded(object sender, RoutedEventArgs e)
        {
            IEnumerable<string> configs = mConfigHistoryManager.Get();

            if (configs.Any())
            {
                string config = configs.First();
                RefreshConfigComboBox(config);
                LoadFromFile(config);
            }
        }

        private void ConfigComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedConfig = (string)xConfigComboBox.SelectedItem;
            RefreshConfigComboBox(selectedConfig);
            LoadFromFile(selectedConfig);
        }

        private void OpenThereOnClick(object sender, RoutedEventArgs e)
        {
            string selectedConfig = (string)xConfigComboBox.SelectedItem;

            if (!string.IsNullOrEmpty(selectedConfig))
            {
                Process.Start(selectedConfig);
            }
        }

        private void OpenHereOnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Config Files (*.xml, *.config) | *.xml; *.config" };

            bool? showDialog = ofd.ShowDialog(this);

            if (showDialog.Value)
            {
                RefreshConfigComboBox(ofd.FileName);
                LoadFromFile(ofd.FileName);
            }
        }

        /// <summary>
        /// Save the specified file name to the set of historical configs.
        /// Sets the config ComboBox's ItemsSource to the set of historical configs.
        /// Sets the config ComboBox's to the specified filename.
        /// </summary>
        /// <param name="fileName"></param>
        private void RefreshConfigComboBox(string fileName)
        {
            mConfigHistoryManager.Save(fileName);
            xConfigComboBox.ItemsSource = mConfigHistoryManager.Get();
            xConfigComboBox.SelectedItem = fileName;
        }

        private void ReloadOnClick(object sender, RoutedEventArgs e)
        {
            ReloadFromFile();
        }

        private void SaveOnClick(object sender, RoutedEventArgs e)
        {
            SaveToFile();
        }

        private void SandAndCloseOnClick(object sender, RoutedEventArgs e)
        {
            SaveToFile();
            Close();
        }

        private void CloseOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveToFile()
        {
            SaveRootAttributes();

            using (XmlTextWriter xtw = new XmlTextWriter((string)xConfigComboBox.SelectedItem, Encoding.UTF8) { Formatting = Formatting.Indented })
            {
                mConfigXml.Save(xtw);
            }
        }

        private void SaveRootAttributes()
        {
            if (xDebugCheckBox.IsChecked.HasValue && xDebugCheckBox.IsChecked.Value)
            {
                mLog4NetNode.AppendAttribute(mConfigXml, Log4NetXmlConstants.DebugAttributeName, "true");
            }
            else
            {
                mLog4NetNode.Attributes.RemoveNamedItem(Log4NetXmlConstants.DebugAttributeName);
            }

            if (Equals(xUpdateComboBox.SelectedItem, UpdateOverwrite))
            {
                //"Merge" is default, so we only need to add an attribute when "Overwrite" is selected
                mLog4NetNode.AppendAttribute(mConfigXml, Log4NetXmlConstants.UpdateAttributeName, UpdateOverwrite);
            }
            else
            {
                mLog4NetNode.Attributes.RemoveNamedItem(Log4NetXmlConstants.UpdateAttributeName);
            }

            if (!Equals(xThresholdComboBox.SelectedItem, Level.All.Name))
            {
                //"All" is default, so we only need to add an attribute when something other than "All" is selected
                mLog4NetNode.AppendAttribute(mConfigXml, Log4NetXmlConstants.ThresholdAttributeName, (string)xThresholdComboBox.SelectedItem);
            }
            else
            {
                mLog4NetNode.Attributes.RemoveNamedItem(Log4NetXmlConstants.ThresholdAttributeName);
            }
        }

        private void ReloadFromFile()
        {
            LoadFromFile((string)xConfigComboBox.SelectedItem);
        }

        private void LoadFromFile(string fileName)
        {
            mConfigXml = new XmlDocument();
            mConfigXml.Load(fileName);

            bool? unrecognizedAppender = LoadFromRam();

            if (unrecognizedAppender.HasValue && unrecognizedAppender.Value)
            {
                MessageBox.Show(this, "At least one unrecognized appender was found in this configuration.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Loads current state of <see cref="mConfigXml"/> into view.
        /// Overwrites any existing values in the view not saved to <see cref="mConfigXml"/>.
        /// Returns true if an unrecognized/unsupported appender was found.
        /// Returns null if configuration can not be loaded.
        /// </summary>
        /// <returns></returns>
        private bool? LoadFromRam()
        {
            XmlNodeList log4NetNodes = mConfigXml.SelectNodes("//log4net");

            if (log4NetNodes == null || log4NetNodes.Count == 0)
            {
                MessageBox.Show(this, "Could not find log4net configuration.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            if (log4NetNodes.Count > 1)
            {
                MessageBox.Show(this, "More than one 'log4net' element was found in the specified file. Using the first occurrence.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            mLog4NetNode = log4NetNodes[0];

            List<ChildModel> children = new List<ChildModel>();

            bool unrecognized = LoadAppenders(children);
            LoadRenderers(children);
            children.AddRange(XmlUtilities.GetRootLoggerAndLoggers(mLog4NetNode));

            xChildren.ItemsSource = children;

            xAddRefsButton.ItemsSource = XmlUtilities.FindAvailableAppenderRefLocations(mLog4NetNode);

            LoadRootAttributes();

            return unrecognized;
        }

        private bool LoadAppenders(ICollection<ChildModel> children)
        {
            //Only selects appenders under this log4net element
            XmlNodeList appenderList = mLog4NetNode.SelectNodes("appender");

            bool unrecognized = false;

            if (appenderList != null)
            {
                foreach (XmlNode node in appenderList)
                {
                    if (TryCreate(node, out AppenderModel model))
                    {
                        children.Add(model);
                    }
                    else
                    {
                        unrecognized = true;
                    }
                }
            }

            return unrecognized;
        }

        private void LoadRenderers(ICollection<ChildModel> children)
        {
            XmlNodeList rendererList = mLog4NetNode.SelectNodes(RendererDescriptor.Renderer.ElementName);

            foreach (XmlNode renderer in rendererList)
            {
                children.Add(new RendererModel(renderer));
            }
        }

        private bool TryCreate(XmlNode appender, out AppenderModel appenderModel)
        {
            string type = appender.Attributes?["type"]?.Value;

            if (AppenderDescriptor.TryFindByTypeNamespace(type, out AppenderDescriptor descriptor))
            {
                string name = appender.Attributes?["name"].Value;
                int incomingReferences = mLog4NetNode.SelectNodes($"//appender-ref[@ref='{name}']").Count;
                appenderModel = new AppenderModel(descriptor, appender, name, incomingReferences);
                return true;
            }

            appenderModel = null;
            return false;
        }

        private void LoadRootAttributes()
        {
            if (bool.TryParse(mLog4NetNode.Attributes?[Log4NetXmlConstants.DebugAttributeName]?.Value, out bool debugResult) && debugResult)
            {
                xDebugCheckBox.IsChecked = true;
            }
            else
            {
                xDebugCheckBox.IsChecked = false;
            }

            string update = mLog4NetNode.Attributes?[Log4NetXmlConstants.UpdateAttributeName]?.Value;

            if (Equals(update, UpdateOverwrite))
            {
                xUpdateComboBox.SelectedItem = UpdateOverwrite;
            }
            else
            {
                xUpdateComboBox.SelectedItem = UpdateMerge;
            }

            if (Log4NetUtilities.TryParseLevel(mLog4NetNode.Attributes?[Log4NetXmlConstants.ThresholdAttributeName]?.Value, out Level levelResult) && !Equals(levelResult, Level.All))
            {
                xThresholdComboBox.SelectedItem = levelResult.Name;
            }
            else
            {
                xThresholdComboBox.SelectedItem = Level.All.Name;
            }
        }

        private void AddAppenderItemOnClick(object appender)
        {
            OpenElementWindow((AppenderDescriptor)appender, null);
        }

        private void EditAppenderOnClick(object sender, RoutedEventArgs e)
        {
            object dataContext = ((DataGridRow)sender).DataContext;

            if (dataContext is AppenderModel appenderModel)
            {
                OpenElementWindow(appenderModel.Descriptor, appenderModel.Node);
            }
            else if (dataContext is RendererModel rendererModel)
            {
                OpenElementWindow(RendererDescriptor.Renderer, rendererModel.Node);
            }
            else if (dataContext is LoggerModel loggerModel)
            {
                OpenElementWindow(LoggerDescriptor.Logger, loggerModel.Node);
            }
            else if (dataContext is ChildModel childModel)
            {
                OpenElementWindow(LoggerDescriptor.Root, childModel.Node);
            }
        }

        private void RemoveAppenderOnClick(object sender, RoutedEventArgs e)
        {
            foreach (ChildModel childModel in xChildren.SelectedItems)
            {
                mLog4NetNode.RemoveChild(childModel.Node);

                if (childModel is AppenderModel appenderModel)
                {
                    RemoveRefsTo(appenderModel);
                }
            }

            LoadFromRam();
        }

        private void RemoveRefsOnClick(object sender, RoutedEventArgs e)
        {
            foreach (ChildModel childModel in xChildren.SelectedItems)
            {
                if (childModel is AppenderModel appenderModel)
                {
                    RemoveRefsTo(appenderModel);
                }
            }

            LoadFromRam();
        }

        private void RemoveRefsTo(AppenderModel appenderModel)
        {
            //Remove all appender refs
            foreach (XmlNode refModel in XmlUtilities.FindAppenderRefs(mLog4NetNode, appenderModel.Name))
            {
                refModel.ParentNode?.RemoveChild(refModel);
            }
        }

        private void AddRefsButtonOnItemClick(object obj)
        {
            ChildModel destination = (ChildModel)obj;

            foreach (AppenderModel appenderModel in xChildren.SelectedItems.OfType<AppenderModel>())
            {
                XmlUtilities.AddAppenderRefToNode(mConfigXml, destination.Node, appenderModel.Name);
            }

            LoadFromRam();
        }

        private void OpenElementWindow(DescriptorBase descriptor, XmlNode originalNode)
        {
            IElementConfiguration configuration = new ElementConfiguration(mConfigXml, mLog4NetNode, originalNode, mConfigXml.CreateElement(descriptor.ElementName));
            ElementWindow elementWindow = new ElementWindow(configuration,
                                                            DefinitionFactory.Create(descriptor, configuration),
                                                            WindowSizeLocationFactory.Create(descriptor),
                                                            new AppenderSaveStrategy(configuration))
                { Owner = this };
            elementWindow.ShowDialog();
            LoadFromRam();
        }

        private void AddRootClick(object sender, RoutedEventArgs e)
        {
            if (xChildren.ItemsSource.Cast<ChildModel>().Any(cm => cm.ElementName == "root"))
            {
                MessageBox.Show(this, "This configuration already contains a root logger.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenElementWindow(LoggerDescriptor.Root, null);
        }

        private void AddRendererClick(object sender, RoutedEventArgs e)
        {
            OpenElementWindow(RendererDescriptor.Renderer, null);
        }

        private void AddLoggerClick(object sender, RoutedEventArgs e)
        {
            OpenElementWindow(LoggerDescriptor.Logger, null);
        }

        private class AppenderSaveStrategy : ISaveStrategy
        {
            private readonly IElementConfiguration mConfiguration;

            public AppenderSaveStrategy(IElementConfiguration configuration)
            {
                mConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            }

            public void Execute()
            {
                if (mConfiguration.OriginalNode == null)
                {
                    //New node - add
                    mConfiguration.Log4NetNode.AppendChild(mConfiguration.NewNode);
                }
                else
                {
                    //Edit - replace
                    mConfiguration.Log4NetNode.ReplaceChild(mConfiguration.NewNode, mConfiguration.OriginalNode);
                }
            }
        }
    }

    internal class ElementConfiguration : IElementConfiguration
    {
        public ElementConfiguration(XmlDocument xmlDocument, XmlNode log4NetNode, XmlNode originalNode, XmlNode newNode)
        {
            ConfigXml = xmlDocument;
            Log4NetNode = log4NetNode;
            OriginalNode = originalNode;
            NewNode = newNode;
        }

        public ElementConfiguration(IConfiguration configuration, XmlNode originalNode, XmlNode newNode)
            : this(configuration.ConfigXml, configuration.Log4NetNode, originalNode, newNode)
        {
        }

        public XmlNode OriginalNode { get; }

        public XmlNode NewNode { get; }

        public XmlDocument ConfigXml { get; }

        public XmlNode Log4NetNode { get; }
    }
}