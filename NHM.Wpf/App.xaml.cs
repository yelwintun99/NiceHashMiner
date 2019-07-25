﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using log4net.Core;
using NHM.Common;
using NHM.Common.Enums;
using NHM.Wpf.Windows;
using NiceHashMiner;
using NiceHashMiner.Configs;
using NiceHashMiner.Stats;

namespace NHM.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string Tag = "NICEHASH";

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // Set working directory to exe
            var pathSet = false;
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (path != null)
            {
                Paths.SetRoot(path);
                Environment.CurrentDirectory = path;
                pathSet = true;
            }

            // Add common folder to path for launched processes
            const string pathKey = "PATH";
            var pathVar = Environment.GetEnvironmentVariable(pathKey);
            pathVar += $";{Path.Combine(Environment.CurrentDirectory, "common")}";
            Environment.SetEnvironmentVariable(pathKey, pathVar);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            // Set security protocols
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls12 |
                                                   SecurityProtocolType.Ssl3;

            // Initialize config
            ConfigManager.InitializeConfig();

            // Check multiple instances
            if (!ConfigManager.GeneralConfig.AllowMultipleInstances)
            {
                try
                {
                    var current = Process.GetCurrentProcess();
                    if (Process.GetProcessesByName(current.ProcessName).Any(p => p.Id != current.Id))
                    {
                        // Shutdown to exit
                        Shutdown();
                    }
                }
                catch
                { }
            }
            
            // Init logger
            Logger.ConfigureWithFile(ConfigManager.GeneralConfig.LogToFile, Level.Info, ConfigManager.GeneralConfig.LogMaxFileSize);

            if (!pathSet)
            {
                Logger.Warn(Tag, "Path not set to executable");
            }

            // Init ExchangeRateApi
            ExchangeRateApi.ActiveDisplayCurrency = ConfigManager.GeneralConfig.DisplayCurrency;

            Logger.Info(Tag, $"Starting up {ApplicationStateManager.Title}");

            if (ConfigManager.GeneralConfig.agreedWithTOS != ApplicationStateManager.CurrentTosVer)
            {
                Logger.Info(Tag, $"TOS differs! agreed: {ConfigManager.GeneralConfig.agreedWithTOS} != Current {ApplicationStateManager.CurrentTosVer}");

                var eula = new EulaWindow();
                eula.ShowDialog();

                if (ConfigManager.GeneralConfig.agreedWithTOS != ApplicationStateManager.CurrentTosVer)
                {
                    Logger.Error(Tag, "TOS differs AFTER TOS confirmation window");
                    Shutdown();
                }
            }

            // Chose lang
            if (string.IsNullOrEmpty(ConfigManager.GeneralConfig.Language))
            {
                if (Translations.GetAvailableLanguagesNames().Count > 1)
                {
                    // TODO lang window
                }
                else
                {
                    ConfigManager.GeneralConfig.Language = "en";
                    ConfigManager.GeneralConfigFileCommit();
                }
            }

            Translations.SelectedLanguage = ConfigManager.GeneralConfig.Language;

            // Check sys requirements
            var canRun = ApplicationStateManager.SystemRequirementsEnsured();
            if (!canRun) Shutdown();

            // Check 3rd party miners
            if (ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.NOT_SET)
            {
                // TODO 3rd party window
            }

            var main = new MainWindow();
            main.Show();
        }
    }
}