using Presentation.Fretboard;

namespace Presentation
{
    public partial class MainWindow 
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new Vm();
        }
    }
}
