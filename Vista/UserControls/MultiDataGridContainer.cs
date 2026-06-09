using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace WPF_Test.Vista.UserControls
{
    public class MultiDataGridContainer : UserControl
    {
        // Propiedades para mantener múltiples DataGrids
        public ObservableCollection<DynamicDataGrid> DataGrids
        {
            get => (ObservableCollection<DynamicDataGrid>)GetValue(DataGridsProperty);
            set => SetValue(DataGridsProperty, value);
        }

        public static readonly DependencyProperty DataGridsProperty =
            DependencyProperty.Register(
                nameof(DataGrids),
                typeof(ObservableCollection<DynamicDataGrid>),
                typeof(MultiDataGridContainer),
                new PropertyMetadata(null));

        // Indice del DataGrid actual
        public int CurrentDataGridIndex
        {
            get => (int)GetValue(CurrentDataGridIndexProperty);
            set => SetValue(CurrentDataGridIndexProperty, value);
        }

        public static readonly DependencyProperty CurrentDataGridIndexProperty =
            DependencyProperty.Register(
                nameof(CurrentDataGridIndex),
                typeof(int),
                typeof(MultiDataGridContainer),
                new PropertyMetadata(0));

        public DynamicDataGrid CurrentDataGrid
        {
            get
            {
                if (DataGrids != null && CurrentDataGridIndex >= 0 && CurrentDataGridIndex < DataGrids.Count)
                    return DataGrids[CurrentDataGridIndex];
                return null;
            }
        }

        public MultiDataGridContainer()
        {
            DataGrids = new ObservableCollection<DynamicDataGrid>();
        }
    }
}
