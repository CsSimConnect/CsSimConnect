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
using CsSimConnect.AI;
using CsSimConnectUI.Domain;
using System.Windows;

namespace CsSimConnectUI
{

    /// <summary>
    /// Interaction logic for CreateAIDialog.xaml
    /// </summary>
    public partial class CreateAIDialog : Window
    {

        private CreateAIViewModel model = new();

        public CreateAIDialog()
        {
            DataContext = model;

            InitializeComponent();
        }

        private void DoCreate(object sender, RoutedEventArgs e)
        {
            if (model.Validated)
            {
                AircraftBuilder bld = AircraftBuilder.Builder(model.Title)
                    .WithTailNumber(model.TailNumber)
                    .AtAirport(model.AirportId);
                AIManager.Instance.Create((ParkedAircraft)bld.Build());

                Close();
            }
        }

        private void DoCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
