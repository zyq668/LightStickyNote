using System.Threading;
using System.Windows;
using System.Windows.Data;
using LightStickyNote.App.Models;
using LightStickyNote.App.Services;

namespace LightStickyNote.App.Tests;

public sealed class WindowPlacementServiceTests
{
    [Fact]
    public void Apply_PreservesTopmostBinding_WithoutChangingWindowOpacity()
    {
        Exception? capturedException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var settings = new AppSettings
                {
                    AlwaysOnTop = true,
                    Opacity = 0.92
                };
                var window = new Window
                {
                    DataContext = settings,
                    Opacity = 1.0
                };

                BindingOperations.SetBinding(
                    window,
                    Window.TopmostProperty,
                    new Binding(nameof(AppSettings.AlwaysOnTop)));

                new WindowPlacementService().Apply(window, new Note(), settings);

                settings.Opacity = 0.72;
                settings.AlwaysOnTop = false;

                Assert.True(BindingOperations.IsDataBound(window, Window.TopmostProperty));
                Assert.Equal(1.0, window.Opacity);
                Assert.False(window.Topmost);
            }
            catch (Exception ex)
            {
                capturedException = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (capturedException is not null)
        {
            throw capturedException;
        }
    }
}
