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

        private string contentText;
        public string ContentText
        {
            get { return contentText; }
            set 
            {
                contentText = value;
                btn.Content = contentText;
            }
        }

    }
}
