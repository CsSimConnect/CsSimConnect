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

using CsSimConnect;
using CsSimConnect.UIComponents.Domain;
using Rakis.Logging;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CsSimConnectUI
{
    /// <summary>
    /// Interaction logic for SelectSimDialog.xaml
    /// </summary>
    public partial class SelectSimDialog : Window
    {
        private static readonly Logger log = Logger.GetLogger(typeof(AIListViewModel));

        public SelectSimDialog()
        {
            DataContext = new SimulatorsModel();

            InitializeComponent();
        }

        private void SelectSimulator(SelectableSimulator sim)
        {
            log.Info?.Log("Selected {0}", sim.Name);
            SimConnect.SetFlightSimType(sim.Sim.Type);
            _ = SimConnect.Instance;

            new MainWindow().Show();
            Close();
        }

        private void SelectSimulatorByStackPanel(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            log.Info?.Log("Selected a {0}", sender.GetType().Name);
            if ((sender is StackPanel panel) && (panel.Parent is Grid grid))
            {
                foreach (var child in grid.Children)
                {
                    if (child is Button)
                    {
                        SelectSimulatorByButton(child, null);
                        break;
                    }
                }
            }
        }

        private void SelectSimulatorByButton(object sender, RoutedEventArgs e)
        {
            if ((sender is Button selectedButton) && (selectedButton.DataContext is KeyValuePair<string,SelectableSimulator> pair))
            {
                SelectSimulator(pair.Value);
            }
        }
    }
}
