﻿// Copyright © 2020 Alex Leendertsen

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Editor.ConfigProperties.Base;
using Editor.HistoryManager;
using Editor.Interfaces;
using Editor.Utilities;

namespace Editor.ConfigProperties
{
    public class File : PropertyBase
    {
        private const string FileName = "file";
        private const string AppendToFileName = "appendToFile";
        private const string TypeName = "type";
        private const string PatternStringTypeName = "log4net.Util.PatternString";
        private readonly IMessageBoxService mMessageBoxService;
        private readonly IHistoryManager mHistoryManager;

        public File(ReadOnlyCollection<IProperty> container, IMessageBoxService messageBoxService, IHistoryManagerFactory historyManagerFactory)
            : base(container, GridLength.Auto)
        {
            Open = new Command(OpenFile);
            mMessageBoxService = messageBoxService;
            mHistoryManager = historyManagerFactory.CreateFilePathHistoryManager();
            HistoricalFiles = mHistoryManager.Get();
        }

        public ICommand Open { get; }

        public IEnumerable<string> HistoricalFiles { get; }

        private string mFilePath;

        public string FilePath
        {
            get => mFilePath;
            set
            {
                if (value == mFilePath)
                {
                    return;
                }

                mFilePath = value;
                OnPropertyChanged();
            }
        }

        private bool mPatternString;

        public bool PatternString
        {
            get => mPatternString;
            set
            {
                if (value == mPatternString)
                {
                    return;
                }

                mPatternString = value;
                OnPropertyChanged();
            }
        }

        private bool mOverwrite;

        public bool Overwrite
        {
            get => mOverwrite;
            set
            {
                if (value == mOverwrite)
                {
                    return;
                }

                mOverwrite = value;
                OnPropertyChanged();
            }
        }

        private void OpenFile()
        {
            bool? showDialog = mMessageBoxService.ShowOpenFileDialog(out string filePath);

            if (showDialog.IsTrue())
            {
                FilePath = filePath;
            }
        }

        public override void Load(XmlNode originalNode)
        {
            string file = originalNode.GetValueAttributeValueFromChildElement(FileName);
            if (!string.IsNullOrEmpty(file))
            {
                FilePath = file;
            }

            PatternString = originalNode[FileName]?.Attributes[TypeName]?.Value == PatternStringTypeName;

            string appendToFile = originalNode.GetValueAttributeValueFromChildElement(AppendToFileName);
            if (!string.IsNullOrEmpty(appendToFile) && bool.TryParse(appendToFile, out bool append))
            {
                Overwrite = !append;
            }
        }

        public override bool TryValidate(IMessageBoxService messageBoxService)
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                messageBoxService.ShowError("A file must be assigned to this appender.");
                return false;
            }

            return base.TryValidate(messageBoxService);
        }

        public override void Save(XmlDocument xmlDoc, XmlNode newNode)
        {
            XmlElement fileElement = xmlDoc.CreateElementWithValueAttribute(FileName, FilePath);

            if (PatternString)
            {
                fileElement.AppendAttribute(xmlDoc, TypeName, PatternStringTypeName);
            }

            fileElement.AppendTo(newNode);

            //"appendToFile" is true by default, so we only need to change it to false if Overwrite is true
            if (Overwrite)
            {
                xmlDoc.CreateElementWithValueAttribute(AppendToFileName, "false").AppendTo(newNode);
            }

            mHistoryManager.Save(FilePath);
        }
    }
}
