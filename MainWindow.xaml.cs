using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;

namespace ShutdownTimerWPF
{
    public partial class MainWindow : Window
    {
        private int minutes = 1;
        private DispatcherTimer timer;
        private DateTime targetTime;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Clip = new RectangleGeometry(new Rect(0, 0, this.ActualWidth, this.ActualHeight), 20, 20);
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtValue.Text, out int value) && value > 1)
                TxtValue.Text = (value - 1).ToString();
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtValue.Text, out int value))
                TxtValue.Text = (value + 1).ToString();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtValue.Text, out minutes) || minutes < 1)
            {
                MessageBox.Show("Veuillez entrer un nombre valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int seconds = minutes * 60;
            targetTime = DateTime.Now.AddMinutes(minutes);
            Process.Start("shutdown", $"/s /t {seconds}");

            InputPanel.Visibility = Visibility.Collapsed;
            CountdownPanel.Visibility = Visibility.Visible;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, evt) =>
            {
                var remaining = targetTime - DateTime.Now;
                if (remaining.TotalSeconds <= 0)
                {
                    timer.Stop();
                    CountdownText.Text = "00:00";
                }
                else
                {
                    int totalMinutes = (int)remaining.TotalMinutes;
                    int seconds = remaining.Seconds;
                    CountdownText.Text = $"{totalMinutes:D2}:{seconds:D2}";
                }
            };
            timer.Start();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("shutdown", "/a");
            timer?.Stop();
            CountdownPanel.Visibility = Visibility.Collapsed;
            InputPanel.Visibility = Visibility.Visible;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
