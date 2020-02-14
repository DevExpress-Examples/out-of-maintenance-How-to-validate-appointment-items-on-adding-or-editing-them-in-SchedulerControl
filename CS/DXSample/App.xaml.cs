using System.Data.Entity;
using System.Windows;
using DevExpress.Internal;
using DXSample.Data;

namespace DXSample {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            DbEngineDetector.PatchConnectionStringsAndConfigureEntityFrameworkDefaultConnectionFactory();
            Database.SetInitializer<SchedulingContext>(new SchedulingContextInitializer());
            base.OnStartup(e);
        }
    }
}
