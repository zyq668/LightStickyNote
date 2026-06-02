using System.Globalization;
using System.Windows;
using System.Windows.Input;
using LightStickyNote.App.ViewModels;

namespace LightStickyNote.App;

public partial class ReminderEditWindow : Window
{
    public ReminderEditWindow(TaskItemViewModel item)
    {
        InitializeComponent();
        TaskItem = item;
        TaskTextBlock.Text = item.Text;
        ClearReminderButton.Visibility = item.HasReminder ? Visibility.Visible : Visibility.Collapsed;

        var initial = item.ReminderAt?.LocalDateTime ?? DateTime.Now.AddHours(1);
        ReminderDateTextBox.Text = initial.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        HourTextBox.Text = initial.Hour.ToString("00", CultureInfo.InvariantCulture);
        MinuteTextBox.Text = initial.Minute.ToString("00", CultureInfo.InvariantCulture);
        CountdownValueTextBox.Text = "1";
    }

    public TaskItemViewModel TaskItem { get; }

    public DateTimeOffset? ReminderAt { get; private set; }

    public bool ReminderCleared { get; private set; }

    private void Header_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void CountdownRadio_OnChecked(object sender, RoutedEventArgs e)
    {
        CountdownValueTextBox.Focus();
        CountdownValueTextBox.SelectAll();
    }

    private void ClearReminderButton_OnClick(object sender, RoutedEventArgs e)
    {
        ReminderCleared = true;
        DialogResult = true;
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        ErrorTextBlock.Visibility = Visibility.Collapsed;

        if (CustomDateTimeRadio.IsChecked == true)
        {
            if (!DateTime.TryParseExact(
                    ReminderDateTextBox.Text,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date) ||
                !TryParseTimePart(HourTextBox.Text, 23, out var hour) ||
                !TryParseTimePart(MinuteTextBox.Text, 59, out var minute))
            {
                ShowError("请输入有效的日期和时间。");
                return;
            }

            var local = new DateTime(
                date.Year,
                date.Month,
                date.Day,
                hour,
                minute,
                0,
                DateTimeKind.Local);

            var reminderAt = new DateTimeOffset(local);
            if (reminderAt <= DateTimeOffset.Now)
            {
                ShowError("提醒时间需要晚于当前时间。");
                return;
            }

            ReminderAt = reminderAt;
            DialogResult = true;
            return;
        }

        if (!int.TryParse(CountdownValueTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount) ||
            amount <= 0)
        {
            ShowError("倒计时需要填写大于 0 的数字。");
            return;
        }

        ReminderAt = GetCountdownUnit() switch
        {
            CountdownUnit.Minutes => DateTimeOffset.Now.AddMinutes(amount),
            CountdownUnit.Hours => DateTimeOffset.Now.AddHours(amount),
            _ => DateTimeOffset.Now.AddDays(amount)
        };

        DialogResult = true;
    }

    private CountdownUnit GetCountdownUnit()
    {
        if (CountdownHoursRadio.IsChecked == true)
        {
            return CountdownUnit.Hours;
        }

        if (CountdownDaysRadio.IsChecked == true)
        {
            return CountdownUnit.Days;
        }

        return CountdownUnit.Minutes;
    }

    private void ShowError(string message)
    {
        ErrorTextBlock.Text = message;
        ErrorTextBlock.Visibility = Visibility.Visible;
    }

    private static bool TryParseTimePart(string? text, int maxValue, out int value)
    {
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) &&
            value >= 0 &&
            value <= maxValue)
        {
            return true;
        }

        value = 0;
        return false;
    }

    private enum CountdownUnit
    {
        Minutes,
        Hours,
        Days
    }
}
