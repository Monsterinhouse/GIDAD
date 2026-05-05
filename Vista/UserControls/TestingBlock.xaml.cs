using System.Windows.Controls;

namespace WPF_Test.Vista.UserControls
{
    /// <summary>
    /// Lógica de interacción para TestingBlock.xaml
    /// </summary>
    public partial class TestingBlock : UserControl
    {
        public TestingBlock()
        {
            InitializeComponent();
        }

        private string placeholderText;
        public string PlaceholderText
        {
            get { return placeholderText; }
            set 
            {
                placeholderText = value;
                btn.Content = placeholderText;
            }
        }

    }
}
