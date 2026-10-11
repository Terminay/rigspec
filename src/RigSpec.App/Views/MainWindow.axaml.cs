using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Threading;
using RigSpec.App.ViewModels;

namespace RigSpec.App.Views;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _animationTimer = new() { Interval = TimeSpan.FromMilliseconds(16) };
    private readonly DispatcherTimer _fadeTimer = new() { Interval = TimeSpan.FromMilliseconds(16) };
    private Ellipse[] _spinnerDots = [];
    private TimeSpan _animationElapsed;
    private TimeSpan _fadeElapsed;
    private bool _initialLoadComplete;
    private bool _fadeStarted;

    public MainWindow()
    {
        InitializeComponent();
        _spinnerDots =
        [
            SpinnerDot1, SpinnerDot2, SpinnerDot3, SpinnerDot4,
            SpinnerDot5, SpinnerDot6, SpinnerDot7, SpinnerDot8
        ];

        _animationTimer.Tick += (_, _) => AnimateSplash();
        _fadeTimer.Tick += (_, _) =>
        {
            _fadeElapsed += _fadeTimer.Interval;
            var progress = Math.Min(_fadeElapsed.TotalSeconds / 0.55, 1);
            var easedProgress = progress * progress * (3 - 2 * progress);
            SplashScreen.Opacity = 1 - easedProgress;

            if (progress >= 1)
            {
                _fadeTimer.Stop();
                SplashScreen.IsVisible = false;
            }
        };

        DataContextChanged += (_, _) => ObserveInitialLoad();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (_initialLoadComplete)
        {
            BeginSplashFade();
        }
        else
        {
            _animationTimer.Start();
        }
    }

    private void ObserveInitialLoad()
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.IsBusy) && !viewModel.IsBusy)
            {
                _initialLoadComplete = true;
                if (IsVisible)
                {
                    BeginSplashFade();
                }
            }
        };

        if (!viewModel.IsBusy)
        {
            _initialLoadComplete = true;
        }
    }

    private void BeginSplashFade()
    {
        if (_fadeStarted)
        {
            return;
        }

        _fadeStarted = true;
        _animationTimer.Stop();
        _fadeElapsed = TimeSpan.Zero;
        _fadeTimer.Start();
    }

    private void AnimateSplash()
    {
        _animationElapsed += _animationTimer.Interval;
        var seconds = _animationElapsed.TotalSeconds;
        var pulse = (Math.Sin(seconds * Math.PI * 2 / 3.2) + 1) / 2;

        var activeDot = seconds * 2.2;
        for (var i = 0; i < _spinnerDots.Length; i++)
        {
            var distance = (i - activeDot + _spinnerDots.Length * 2) % _spinnerDots.Length;
            distance = Math.Min(distance, _spinnerDots.Length - distance);
            _spinnerDots[i].Opacity = 0.16 + Math.Max(0, 1 - distance / 3.5) * 0.84;
        }

        PrimaryGlow.Opacity = 0.12 + pulse * 0.06;
        SecondaryGlow.Opacity = 0.09 + (1 - pulse) * 0.06;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            if (e.Key is Key.OemPlus or Key.Add)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.IncreaseFontSize();
                    e.Handled = true;
                }
            }
            else if (e.Key is Key.OemMinus or Key.Subtract or Key.OemPeriod)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.DecreaseFontSize();
                    e.Handled = true;
                }
            }
            else if (e.Key is Key.D0 or Key.NumPad0)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.ResetFontSize();
                    e.Handled = true;
                }
            }
        }
    }
}
