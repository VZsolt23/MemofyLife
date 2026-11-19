using MemofyLife.Common.Theming;

namespace MemofyLife
{
    public partial class App : Application
    {
        private readonly ThemeService _theme;

        public App(ThemeService theme)
        {
            InitializeComponent();
            _theme = theme;

            // Az első oldal előtt: a színerőforrások már a választott sémát tükrözik.
            _theme.Initialize(this);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            // Az állapotsor színe csak élő Activity mellett állítható.
            window.Created += (_, _) => _theme.ApplySystemBars();
            window.Resumed += (_, _) => _theme.ApplySystemBars();
            return window;
        }
    }
}
