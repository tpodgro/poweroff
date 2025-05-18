using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Globalization;

namespace ShutdownTimerWPF
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private DateTime targetTime;
        private DateTime selectedDate;
        private int selectedHours = 23;
        private int selectedMinutes = 30;
        private int currentYear, currentMonth;
        private Button[,] calendarButtons;

        public MainWindow()
        {
            InitializeComponent();

            // Initialisation avec la date/heure actuelle
            InitializeWithCurrentTime();

            // Configuration de l'UI
            SetupUI();
        }

        private void InitializeWithCurrentTime()
        {
            selectedDate = DateTime.Now;
            currentYear = selectedDate.Year;
            currentMonth = selectedDate.Month;
            selectedHours = selectedDate.Hour;
            selectedMinutes = selectedDate.Minute;

            UpdateTimeDisplay();
            UpdateDateDisplay();
        }

        private void SetupUI()
        {
            InitializeCalendarGrid();
            UpdateCalendarDisplay();
            SizeChanged += MainWindow_SizeChanged;
            Loaded += Window_Loaded;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustUIForCurrentSize();
        }

        private void AdjustUIForCurrentSize()
        {
            // Ajustements responsifs
            CurrentTimeDisplay.FontSize = ActualWidth < 320 ? 36 : 48;
            CurrentDateDisplay.FontSize = ActualWidth < 320 ? 12 : 14;
            InputPanel.Margin = new Thickness(0, ActualHeight < 500 ? 5 : 10, 0, 0);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Clip = new RectangleGeometry(new Rect(0, 0, this.ActualWidth, this.ActualHeight), 20, 20);
            AdjustUIForCurrentSize();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        #region Time Picker Methods

        private void UpdateTime(int hoursDelta, int minutesDelta)
        {
            selectedHours = (selectedHours + hoursDelta + 24) % 24;
            selectedMinutes = (selectedMinutes + minutesDelta + 60) % 60;
            UpdateTimeDisplay();
        }

        private void HoursUpButton_Click(object sender, RoutedEventArgs e) => UpdateTime(1, 0);
        private void HoursDownButton_Click(object sender, RoutedEventArgs e) => UpdateTime(-1, 0);
        private void MinutesUpButton_Click(object sender, RoutedEventArgs e) => UpdateTime(0, 5);
        private void MinutesDownButton_Click(object sender, RoutedEventArgs e) => UpdateTime(0, -5);

        private void UpdateTimeDisplay()
        {
            HoursDisplay.Text = selectedHours.ToString("D2");
            MinutesDisplay.Text = selectedMinutes.ToString("D2");
            CurrentTimeDisplay.Text = $"{selectedHours:D2}:{selectedMinutes:D2}";
        }

        #endregion

        #region Date Picker Methods

        private void ChangeMonth(int delta)
        {
            currentMonth += delta;
            if (currentMonth > 12)
            {
                currentMonth = 1;
                currentYear++;
            }
            else if (currentMonth < 1)
            {
                currentMonth = 12;
                currentYear--;
            }
            UpdateCalendarDisplay();
        }

        private void PreviousMonthButton_Click(object sender, RoutedEventArgs e) => ChangeMonth(-1);
        private void NextMonthButton_Click(object sender, RoutedEventArgs e) => ChangeMonth(1);

        private void InitializeCalendarGrid()
        {
            // Configuration de la grille 6x7
            for (int i = 0; i < 6; i++)
                CalendarGrid.RowDefinitions.Add(new RowDefinition());

            for (int i = 0; i < 7; i++)
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition());

            // Création des boutons
            calendarButtons = new Button[6, 7];
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    var btn = new Button
                    {
                        Style = (Style)FindResource("CalendarDayButton"),
                        Content = "",
                        Tag = (row, col)
                    };
                    btn.Click += CalendarDayButton_Click;
                    Grid.SetRow(btn, row);
                    Grid.SetColumn(btn, col);
                    CalendarGrid.Children.Add(btn);
                    calendarButtons[row, col] = btn;
                }
            }
        }

        private void UpdateCalendarDisplay()
        {
            // Mise à jour de l'en-tête
            MonthYearDisplay.Text = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(currentMonth).ToLower()} {currentYear}";

            // Calcul des jours
            var firstDay = new DateTime(currentYear, currentMonth, 1);
            int firstDayOfWeek = ((int)firstDay.DayOfWeek + 6) % 7;
            int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
            DateTime today = DateTime.Today;

            // Réinitialisation des boutons
            foreach (Button btn in calendarButtons)
            {
                btn.Content = "";
                btn.Style = (Style)FindResource("CalendarDayButton");
                btn.IsEnabled = false;
            }

            // Remplissage des jours
            int day = 1;
            for (int i = 0; i < 6 && day <= daysInMonth; i++)
            {
                for (int j = 0; j < 7 && day <= daysInMonth; j++)
                {
                    if (i == 0 && j < firstDayOfWeek) continue;

                    var btn = calendarButtons[i, j];
                    btn.Content = day.ToString();
                    var currentDate = new DateTime(currentYear, currentMonth, day);

                    if (currentDate >= today)
                    {
                        btn.IsEnabled = true;
                        if (selectedDate.Date == currentDate)
                            btn.Style = (Style)FindResource("SelectedCalendarDayButton");
                    }
                    else
                    {
                        btn.Style = (Style)FindResource("DisabledCalendarDayButton");
                    }

                    day++;
                }
            }
        }

        private void CalendarDayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content.ToString() != "")
            {
                int day = int.Parse(btn.Content.ToString());
                selectedDate = new DateTime(currentYear, currentMonth, day, selectedHours, selectedMinutes, 0);

                UpdateDateDisplay();
                UpdateCalendarDisplay(); // Rafraîchit tout le calendrier pour la sélection
            }
        }

        private void UpdateDateDisplay()
        {
            CurrentDateDisplay.Text = selectedDate.ToString("dd/MM/yyyy");
        }

        #endregion

        #region DateTime Picker Panel Methods

        private void ToggleDateTimePicker(bool show)
        {
            DateTimePickerOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            if (show)
            {
                UpdateCalendarDisplay();
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2));
                DateTimePickerPanel.BeginAnimation(OpacityProperty, fadeIn);
            }
        }

        private void DateTimePickerLink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => ToggleDateTimePicker(true);

        private void CloseDateTimePicker_Click(object sender, RoutedEventArgs e)
            => ToggleDateTimePicker(false);

        private void ApplyDateTimeSelection_Click(object sender, RoutedEventArgs e)
        {
            UpdateTimeDisplay();
            UpdateDateDisplay();
            ToggleDateTimePicker(false);
        }

        #endregion

        #region Preset Methods

        private void SetPreset(int minutes)
        {
            var now = DateTime.Now.AddMinutes(minutes);
            selectedDate = now.Date;
            selectedHours = now.Hour;
            selectedMinutes = now.Minute;

            UpdateTimeDisplay();
            UpdateDateDisplay();
            Confirm_Click(null, null);
        }

        private void Preset5_Click(object sender, RoutedEventArgs e) => SetPreset(5);
        private void Preset30_Click(object sender, RoutedEventArgs e) => SetPreset(30);
        private void Preset60_Click(object sender, RoutedEventArgs e) => SetPreset(60);

        #endregion

        #region Shutdown Methods

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            targetTime = new DateTime(
                selectedDate.Year,
                selectedDate.Month,
                selectedDate.Day,
                selectedHours,
                selectedMinutes,
                0
            );

            if (targetTime <= DateTime.Now)
            {
                MessageBox.Show("Veuillez sélectionner une date et une heure dans le futur.",
                              "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int seconds = (int)(targetTime - DateTime.Now).TotalSeconds;
            Process.Start(new ProcessStartInfo("shutdown", $"/s /t {seconds}") { CreateNoWindow = true });

            // Transition UI
            AnimatePanelTransition(InputPanel, CountdownPanel);
            ShutdownDateText.Text = targetTime.ToString("dd/MM/yyyy");

            // Timer
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, evt) => UpdateCountdownDisplay();
            timer.Start();
        }

        private void UpdateCountdownDisplay()
        {
            var remaining = targetTime - DateTime.Now;
            if (remaining.TotalSeconds <= 0)
            {
                timer.Stop();
                CountdownText.Text = "00:00";
            }
            else
            {
                CountdownText.Text = remaining.TotalHours >= 1
                    ? $"{(int)remaining.TotalHours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}"
                    : $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("shutdown", "/a") { CreateNoWindow = true });
            timer?.Stop();
            AnimatePanelTransition(CountdownPanel, InputPanel);
        }

        private void AnimatePanelTransition(UIElement hideElement, UIElement showElement)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));

            fadeOut.Completed += (s, args) =>
            {
                hideElement.Visibility = Visibility.Collapsed;
                showElement.Visibility = Visibility.Visible;
                showElement.BeginAnimation(OpacityProperty, fadeIn);
            };

            hideElement.BeginAnimation(OpacityProperty, fadeOut);
        }

        #endregion

        #region Window Control Methods

        private void Minimize_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
            fadeOut.Completed += (s, args) => Close();
            BeginAnimation(OpacityProperty, fadeOut);
        }

        #endregion
    }
}