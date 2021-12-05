/*
 * Copyright (c) 2021. Bert Laverman
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Windows;

namespace AutoPilotController
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private MainWindow mainWindow;

        public SettingsDialog(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            DataContext = AutoPilotSettings.Instance;

            InitializeComponent();
        }

        private void DoCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DoOk(object sender, RoutedEventArgs e)
        {
            AutoPilotSettings.Instance.Store();
            mainWindow.ProcessSettings();

            Close();
        }

    }
}
