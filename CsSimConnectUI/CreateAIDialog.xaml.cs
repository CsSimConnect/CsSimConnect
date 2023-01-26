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
using CsSimConnect.UIComponents.Domain;
using Rakis.Logging;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static CsSimConnect.Util.StringUtil;

namespace CsSimConnectUI
{

    /// <summary>
    /// Interaction logic for CreateAIDialog.xaml
    /// </summary>
    public partial class CreateAIDialog : Window
    {
        private static readonly ILogger log = Logger.GetLogger(typeof(CreateAIDialog));

        private CreateAIViewModel model = new();

        public CreateAIDialog()
        {
            DataContext = model;

            InitializeComponent();
        }

        private void DoCreate(object sender, RoutedEventArgs e)
        {
            string title = aircraftTitle.SelectedValue?.ToString();
            if (IsEmpty(title)) title = aircraftTitle.Text;
            if (!IsEmpty(tailNumber.Text) && !IsEmpty(title) && (model.Parking != null))
            {
                SimulatedAircraft aircraft = AircraftBuilder.Builder(title)
                    .WithTailNumber(tailNumber.Text)
                    .AtPosition(model.Parking.Latitude, model.Parking.Longitude, model.Airport.AltitudeFeet)
                    .WithHeading(model.Parking.Heading)
                    .OnGround().Static().Build();
                AIManager.Instance.Create(aircraft).Subscribe(_ => Dispatcher.Invoke(Close), ShowError);
            }
            else if (IsEmpty(tailNumber.Text))
            {
                log.Error?.Log($"No tailnumber");
                Close();
            }
            else if (IsEmpty(aircraftTitle.SelectedValue?.ToString()))
            {
                log.Error?.Log($"No title");
                Close();
            }
            else if (IsEmpty(aircraftTitle.SelectedValue?.ToString()))
            {
                log.Error?.Log($"No Parking selected");
                Close();
            }
            else
            {
                Close();
            }
        }

        private void CleanupAfterCreate()
        {
            log.Info?.Log($"Done creating");
            Dispatcher.Invoke(Close);
        }
        private void ShowError(Exception exc)
        {
            Dispatcher.Invoke(() => MessageBox.Show("FAILED: " + exc.Message));
        }

        private void DoCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AirportSelected(object sender, RoutedEventArgs e)
        {
            model.LoadParkings(icao.Text);
        }

        private void ParkingSelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            model.SetSelectedParking(parking.SelectedValue.ToString());
        }

        private void KeyUp_ICAO(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool found = false;
            var data = model.ICAOList;

            string query = icao.Text;

            if (query.Length == 0)
            {
                // Clear
                icaoResults.Children.Clear();
                icaoLister.Visibility = Visibility.Collapsed;
            }
            else
            {
                icaoLister.Visibility = Visibility.Visible;
            }

            // Clear the list
            icaoResults.Children.Clear();

            // Add the result
            int maxResults = 10;
            foreach (var airportIcao in data)
            {
                if (airportIcao.Contains(query))
                {
                    // The word starts with this... Autocomplete must work
                    addItem(airportIcao);
                    found = true;
                    if (--maxResults == 0)
                    {
                        break;
                    }
                }
            }

            if (!found)
            {
                icaoResults.Children.Add(new TextBlock() { Text = "No results found." });
            }
        }
        private void addItem(string text)
        {
            TextBlock block = new TextBlock();

            // Add the text
            block.Text = text;

            // A little style...
            block.Margin = new Thickness(2, 3, 2, 3);
            block.Cursor = Cursors.Hand;

            // Mouse events
            block.MouseLeftButtonUp += (sender, e) =>
            {
                icao.Text = (sender as TextBlock).Text;
                icaoLister.Visibility = Visibility.Collapsed;
            };

            block.MouseEnter += (sender, e) =>
            {
                TextBlock b = sender as TextBlock;
                b.Background = Brushes.PeachPuff;
            };

            block.MouseLeave += (sender, e) =>
            {
                TextBlock b = sender as TextBlock;
                b.Background = Brushes.Transparent;
            };
            block.KeyDown += (sender, key) =>
            {
                if (key.Key == Key.Enter)
                {
                    icao.Text = (sender as TextBlock).Text;
                    icaoLister.Visibility = Visibility.Collapsed;
                }
                else if (key.Key == Key.Escape)
                {
                    icaoLister.Visibility = Visibility.Collapsed;
                }
            };
            // Add to the panel
            icaoResults.Children.Add(block);
        }

    }
}
