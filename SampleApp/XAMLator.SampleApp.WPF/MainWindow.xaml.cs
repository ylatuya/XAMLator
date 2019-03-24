using Xamarin.Forms.Platform.WPF;

namespace XAMLator.SampleApp.WPF
{
    public partial class MainWindow : FormsApplicationPage
    {
        public MainWindow()
        {
            InitializeComponent();
            Xamarin.Forms.Forms.Init();
            LoadApplication(new SampleApp.App());
        }
    }
}