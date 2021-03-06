﻿using NHM.Common;
using NHMCore;
using NHMCore.Configs;
using NHMCore.Utils;
using NiceHashMiner.Views.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NiceHashMiner.Views.Settings
{
    /// <summary>
    /// Interaction logic for SettingsGeneral.xaml
    /// </summary>
    public partial class SettingsGeneral : UserControl
    {
        public SettingsGeneral()
        {
            InitializeComponent();
            LanguageSettings.Visibility = AppRuntimeSettings.ShowLanguage ? Visibility.Visible : Visibility.Collapsed;
            ThemeSettings.Visibility = AppRuntimeSettings.ThemeSettingsEnabled ? Visibility.Visible : Visibility.Collapsed;
            CredentialsSettings.Instance.PropertyChanged += (s, e) =>
            {
                if(e.PropertyName == "BitcoinAddress")
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        textBoxBTCAddress.Text = CredentialsSettings.Instance.BitcoinAddress;
                    });
                }
            };
            if (CredentialsSettings.Instance.IsBitcoinAddressValid)
            {
                textBoxBTCAddress.Text = CredentialsSettings.Instance.BitcoinAddress;
                textBoxBTCAddress.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxBTCAddress.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            }
            if (CredentialsSettings.Instance.IsWorkerNameValid)
            {
                textBoxWorkerName.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxWorkerName.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            }
        }

        private void AddressHyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

        private void ValidateBTCAddr()
        {
            var trimmedBtcText = textBoxBTCAddress.Text.Trim();
            var btcOK = CredentialValidators.ValidateBitcoinAddress(trimmedBtcText);
            if (btcOK)
            {
                textBoxBTCAddress.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxBTCAddress.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            }
            else
            {
                textBoxBTCAddress.Style = Application.Current.FindResource("InputBoxBad") as Style;
                textBoxBTCAddress.BorderBrush = (Brush)Application.Current.FindResource("RedDangerColorBrush");
            }
        }

        private async void TextBoxBitcoinAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            var trimmedBtcText = textBoxBTCAddress.Text.Trim();
            var result = await ApplicationStateManager.SetBTCIfValidOrDifferent(trimmedBtcText);
            ValidateBTCAddr();
        }

        private void TextBoxBitcoinAddress_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ValidateBTCAddr();
        }

        private void TextBoxBitcoinAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CredentialsSettings.Instance.IsBitcoinAddressValid)
            {
                textBoxBTCAddress.Text = CredentialsSettings.Instance.BitcoinAddress;
                textBoxBTCAddress.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxBTCAddress.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            }
            else
            {
                textBoxBTCAddress.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxBTCAddress.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            }
        }

        private void ValidateWorkername()
        {
            var trimmedWorkername = textBoxWorkerName.Text.Trim();
            var btcOK = CredentialValidators.ValidateWorkerName(trimmedWorkername);
            if (btcOK)
            {
                textBoxWorkerName.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxWorkerName.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            }
            else
            {
                textBoxWorkerName.Style = Application.Current.FindResource("InputBoxBad") as Style;
                textBoxWorkerName.BorderBrush = (Brush)Application.Current.FindResource("RedDangerColorBrush");
            }
        }

        // TODO validator can be outside from setting
        private void TextBoxWorkerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var trimmedWorkerNameText = textBoxWorkerName.Text.Trim();
            var result = ApplicationStateManager.SetWorkerIfValidOrDifferent(trimmedWorkerNameText);
            ValidateWorkername();
        }

        private void TextBoxWorkerName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ValidateWorkername();
        }

        private void TextBoxWorkerName_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (CredentialsSettings.Instance.IsWorkerNameValid)
            //{
            //    //// TODO we break binding if we assing this
            //    //textBoxWorkerName.Text = CredentialsSettings.Instance.WorkerName;
            //    textBoxWorkerName.Style = Application.Current.FindResource("InputBoxGood") as Style;
            //    textBoxWorkerName.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            //}
            //else
            //{
            //    textBoxWorkerName.Style = Application.Current.FindResource("InputBoxGood") as Style;
            //    textBoxWorkerName.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            //}
            ValidateWorkername();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfigManager.GeneralConfigFileCommit();
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var nhmConfirmDialog = new CustomDialog()
            {
                Title = Translations.Tr("Language change"),
                Description = Translations.Tr("Your program will be restarted in order to completely change the language of NiceHash Miner."),
                OkText = Translations.Tr("Ok"),
                CancelVisible = Visibility.Collapsed,
                AnimationVisible = Visibility.Collapsed
            };
            nhmConfirmDialog.OKClick += (s, e1) => CommitGeneralAndRestart();
            nhmConfirmDialog.OnExit += (s, e1) => CommitGeneralAndRestart();
            
            CustomDialogManager.ShowModalDialog(nhmConfirmDialog);
        }

        private void CommitGeneralAndRestart()
        {
            ConfigManager.GeneralConfigFileCommit();
            Task.Run(() => ApplicationStateManager.RestartProgram());
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigManager.GeneralConfigFileCommit();
        }

        private void CreateLogReportButton_Click(object sender, RoutedEventArgs e)
        {
            var nhmConfirmDialog = new CustomDialog()
            { 
                Title = Translations.Tr("Pack and upload log files?"),
                Description = Translations.Tr("This will upload log report to our server."),
                OkText = Translations.Tr("Ok"),
                CancelText = Translations.Tr("Cancel"),
                AnimationVisible = Visibility.Collapsed
            };
            nhmConfirmDialog.OKClick += (s, e1) => ProcessLogReport();
            CustomDialogManager.ShowModalDialog(nhmConfirmDialog);
        }

        private async void ProcessLogReport()
        {
            var uuid = Guid.NewGuid().ToString();
            var success = await Helpers.CreateAndUploadLogReport(uuid);
            if (success) CreateBugUUIDDialog(uuid);
        }

        private void CreateBugUUIDDialog(string uuid)
        {
            var bugUUIDDialog = new CustomDialog()
            {
                Title = Translations.Tr("Bug report uuid"),
                Description = Translations.Tr("Use following UUID for bug reporting.\n{0}", uuid),
                OkText = Translations.Tr("Copy to clipboard"),
                CancelVisible = Visibility.Collapsed,
                AnimationVisible = Visibility.Collapsed
            };

            bugUUIDDialog.OKClick += (s,e) => Clipboard.SetText(uuid);
            CustomDialogManager.ShowModalDialog(bugUUIDDialog);
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var nhmConfirmDialog = new CustomDialog()
            {
                Title = Translations.Tr("Theme change"),
                Description = Translations.Tr("Your program will be restarted in order to completely change the theme of NiceHash Miner."),
                OkText = Translations.Tr("Ok"),
                CancelVisible = Visibility.Collapsed,
                AnimationVisible = Visibility.Collapsed
            };
            nhmConfirmDialog.OKClick += (s, e1) => CommitGeneralAndRestart();
            nhmConfirmDialog.OnExit += (s, e1) => CommitGeneralAndRestart();

            CustomDialogManager.ShowModalDialog(nhmConfirmDialog);
        }
    }
}
