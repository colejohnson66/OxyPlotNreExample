using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Threading.Tasks;

namespace OxyPlotNreExample.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public PlotController Controller { get; } = new();
    public PlotModel Model { get; } = new();
    private readonly LinearAxis XAxis = new()
    {
        AbsoluteMaximum = 100,
        AbsoluteMinimum = 0,
        MajorStep = 10,
        Maximum = 100,
        Minimum = 0,
        Position = AxisPosition.Bottom,
        Title = "X",
    };
    private readonly LinearAxis YAxis = new()
    {
        AbsoluteMaximum = 100,
        AbsoluteMinimum = 0,
        MajorStep = 10,
        Maximum = 100,
        Minimum = 0,
        Position = AxisPosition.Left,
        Title = "X",
    };
    private readonly LineSeries Series = new()
    {
        StrokeThickness = 2,
    };

    private readonly object _lock = new();

    public MainWindowViewModel()
    {
        Model.Axes.Add(XAxis);
        Model.Axes.Add(YAxis);

        Model.Series.Add(Series);

        // add random data
        for (int i = 0; i < 100; i++)
            Series.Points.Add(new(Random.Shared.Next(0, 100), Random.Shared.Next(0, 100)));


        RxApp.MainThreadScheduler.Schedule(() => Model.InvalidatePlot(true));

        // two loops that would clash
        RxApp.TaskpoolScheduler.Schedule(Loop);
        RxApp.TaskpoolScheduler.Schedule(Loop);
    }

    private async void Loop()
    {
        while (true)
        {
            lock (_lock)
            {
                Model.Annotations.Clear();
                Model.Annotations.Add(new TextAnnotation
                {
                    Text = "idk",
                    TextPosition = new(50, 50),
                });
            }

            Exception? ex = Model.GetLastPlotException();
            if (ex is not null)
            {
                Debugger.Break();
                return;
            }

            RxApp.MainThreadScheduler.Schedule(() => Model.InvalidatePlot(true));

            ex = Model.GetLastPlotException();
            if (ex is not null)
            {
                Debugger.Break();
                return;
            }

            // cause random delay so the threads will get out of sync quickly
            await Task.Delay(Random.Shared.Next(0, 100));
        }
    }
}
