using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WPF_Test.Models;

namespace WPF_Test.Vista.UserControls
{
    public partial class DynamicDataGrid : UserControl
    {
        private bool _isUpdatingTriggerColumn = false;
        private bool _isProcessingPropertyChange = false;

        public ObservableCollection<string> ColumnHeaders
        {
            get => (ObservableCollection<string>)GetValue(ColumnHeadersProperty);
            set => SetValue(ColumnHeadersProperty, value);
        }
        public static readonly DependencyProperty ColumnHeadersProperty =
            DependencyProperty.Register(nameof(ColumnHeaders),
                typeof(ObservableCollection<string>), typeof(DynamicDataGrid),
                new PropertyMetadata(null, OnColumnHeadersChanged));

        public Dictionary<string, string> ColumnPropertyMap
        {
            get => (Dictionary<string, string>)GetValue(ColumnPropertyMapProperty);
            set => SetValue(ColumnPropertyMapProperty, value);
        }
        public static readonly DependencyProperty ColumnPropertyMapProperty =
            DependencyProperty.Register(nameof(ColumnPropertyMap),
                typeof(Dictionary<string, string>), typeof(DynamicDataGrid),
                new PropertyMetadata(null));

        public string TriggerColumn
        {
            get => (string)GetValue(TriggerColumnProperty);
            set => SetValue(TriggerColumnProperty, value);
        }
        public static readonly DependencyProperty TriggerColumnProperty =
            DependencyProperty.Register(nameof(TriggerColumn),
                typeof(string), typeof(DynamicDataGrid),
                new PropertyMetadata(null));

        public IEnumerable<string> ServicioComboOptions
        {
            get => (IEnumerable<string>)GetValue(ServicioComboOptionsProperty);
            set => SetValue(ServicioComboOptionsProperty, value);
        }
        public static readonly DependencyProperty ServicioComboOptionsProperty =
            DependencyProperty.Register(nameof(ServicioComboOptions),
                typeof(IEnumerable<string>), typeof(DynamicDataGrid),
                new PropertyMetadata(null));

        public ObservableCollection<OrdenRow> Rows { get; } = new();

        private const string TransferenciaColumnKey = "TRANSFERENCIA";

        // Propiedades que son ComboBox (mapeadas a colecciones con ServicioComboOptions)
        private static readonly HashSet<string> ComboBoxProperties = new()
        {
            nameof(OrdenRow.Servicio),
            nameof(OrdenRow.MedioPago)
        };

        // Propiedades que son dinero
        private static readonly HashSet<string> MoneyProperties = new()
        {
            nameof(OrdenRow.Efectivo),
            nameof(OrdenRow.Seña),
            nameof(OrdenRow.Tarjeta)
        };

        private static void OnColumnHeadersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DynamicDataGrid control && control.IsLoaded)
                control.RebuildColumns();
        }

        private void RebuildColumns()
        {
            if (ColumnHeaders == null || InnerGrid == null) return;

            InnerGrid.Columns.Clear();

            foreach (var header in ColumnHeaders)
            {
                string propName = null;
                ColumnPropertyMap?.TryGetValue(header, out propName);

                if (propName == TransferenciaColumnKey)
                {
                    InnerGrid.Columns.Add(BuildTransferenciaColumn(header));
                }
                else if (propName != null && ComboBoxProperties.Contains(propName) && ServicioComboOptions != null)
                {
                    // CORRECCIÓN: se pasa propName para que el ComboBox bindee a la propiedad correcta
                    InnerGrid.Columns.Add(BuildComboColumn(header, propName));
                }
                else if (propName != null && MoneyProperties.Contains(propName))
                {
                    // CORRECCIÓN: columna de dinero con template propio para edición
                    InnerGrid.Columns.Add(BuildMoneyColumn(header, propName));
                }
                else if (propName != null)
                {
                    InnerGrid.Columns.Add(new DataGridTextColumn
                    {
                        Header = header,
                        Binding = new Binding(propName)
                        {
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        },
                        Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                    });
                }
            }
        }

        // NUEVO: columna con formato de dinero usando DataGridTemplateColumn
        private static DataGridTemplateColumn BuildMoneyColumn(string header, string propName)
        {
            // Lectura: TextBlock formateado
            var displayTemplate = new DataTemplate();
            var displayBlock = new FrameworkElementFactory(typeof(TextBlock));
            displayBlock.SetBinding(TextBlock.TextProperty, new Binding(propName)
            {
                StringFormat = "${0:N2}"
            });
            displayBlock.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
            displayBlock.SetValue(TextBlock.PaddingProperty, new Thickness(0, 0, 6, 0));
            displayTemplate.VisualTree = displayBlock;

            // Edición: TextBox limpio alineado a la derecha
            var editTemplate = new DataTemplate();
            var editBox = new FrameworkElementFactory(typeof(TextBox));
            editBox.SetBinding(TextBox.TextProperty, new Binding(propName)
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            editBox.SetValue(TextBox.TextAlignmentProperty, TextAlignment.Right);
            editTemplate.VisualTree = editBox;

            return new DataGridTemplateColumn
            {
                Header = header,
                CellTemplate = displayTemplate,
                CellEditingTemplate = editTemplate,
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            };
        }

        // CORREGIDO: recibe propName para bindear a la propiedad correcta (Servicio o MedioPago)
        private DataGridTemplateColumn BuildComboColumn(string header, string propName)
        {
            // Lectura
            var displayTemplate = new DataTemplate();
            var displayFactory = new FrameworkElementFactory(typeof(TextBlock));
            displayFactory.SetBinding(TextBlock.TextProperty, new Binding(propName));
            displayTemplate.VisualTree = displayFactory;

            // Edición
            var editTemplate = new DataTemplate();
            var comboFactory = new FrameworkElementFactory(typeof(ComboBox));
            comboFactory.SetValue(ComboBox.ItemsSourceProperty, ServicioComboOptions);
            comboFactory.SetBinding(ComboBox.SelectedItemProperty, new Binding(propName)
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            });
            comboFactory.AddHandler(ComboBox.LoadedEvent,
                new RoutedEventHandler((s, e) => ((ComboBox)s).IsDropDownOpen = true));
            editTemplate.VisualTree = comboFactory;

            return new DataGridTemplateColumn
            {
                Header = header,
                CellTemplate = displayTemplate,
                CellEditingTemplate = editTemplate,
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            };
        }

        private DataGridTemplateColumn BuildTransferenciaColumn(string header)
        {
            var displayTemplate = new DataTemplate();
            var displayPanel = new FrameworkElementFactory(typeof(StackPanel));
            displayPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var montoText = new FrameworkElementFactory(typeof(TextBlock));
            montoText.SetBinding(TextBlock.TextProperty, new Binding(nameof(OrdenRow.TransferenciaMonto))
            {
                StringFormat = "${0:N2}"
            });

            var sep = new FrameworkElementFactory(typeof(TextBlock));
            sep.SetValue(TextBlock.TextProperty, " — ");

            var nombreText = new FrameworkElementFactory(typeof(TextBlock));
            nombreText.SetBinding(TextBlock.TextProperty, new Binding(nameof(OrdenRow.TransferenciaNombre)));

            displayPanel.AppendChild(montoText);
            displayPanel.AppendChild(sep);
            displayPanel.AppendChild(nombreText);
            displayTemplate.VisualTree = displayPanel;

            var editTemplate = new DataTemplate();
            var editPanel = new FrameworkElementFactory(typeof(StackPanel));

            var montoBox = new FrameworkElementFactory(typeof(TextBox));
            montoBox.SetBinding(TextBox.TextProperty, new Binding(nameof(OrdenRow.TransferenciaMonto))
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            montoBox.SetValue(TextBox.TextAlignmentProperty, TextAlignment.Right);

            var nombreBox = new FrameworkElementFactory(typeof(TextBox));
            nombreBox.SetBinding(TextBox.TextProperty, new Binding(nameof(OrdenRow.TransferenciaNombre))
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            editPanel.AppendChild(montoBox);
            editPanel.AppendChild(nombreBox);
            editTemplate.VisualTree = editPanel;

            return new DataGridTemplateColumn
            {
                Header = header,
                CellTemplate = displayTemplate,
                CellEditingTemplate = editTemplate,
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            };
        }

        private void AddNewRow()
        {
            var row = new OrdenRow();
            row.PropertyChanged += Row_PropertyChanged;
            Rows.Add(row);
        }

        private void Row_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isProcessingPropertyChange) return;
            try
            {
                _isProcessingPropertyChange = true;
                if (e.PropertyName != TriggerColumn) return;
                if (_isUpdatingTriggerColumn) return;

                var changedRow = (OrdenRow)sender;
                int index = Rows.IndexOf(changedRow);

                if (!string.IsNullOrWhiteSpace(changedRow.NroOrden))
                {
                    if (index == Rows.Count - 1)
                        AddNewRow();
                }
                else
                {
                    try
                    {
                        _isUpdatingTriggerColumn = true;
                        ClearRow(changedRow);
                    }
                    finally
                    {
                        _isUpdatingTriggerColumn = false;
                    }
                    RemoveTrailingEmptyRows();
                }
            }
            finally
            {
                _isProcessingPropertyChange = false;
            }
        }

        private void RemoveTrailingEmptyRows()
        {
            for (int i = Rows.Count - 1; i >= 1; i--)
            {
                if (IsRowEmpty(Rows[i]))
                {
                    Rows[i].PropertyChanged -= Row_PropertyChanged;
                    Rows.RemoveAt(i);
                }
                else break;
            }
        }

        private void ClearRow(OrdenRow row)
        {
            row.NroOrden = null;
            row.Servicio = null;
            row.Efectivo = null;
            row.Seña = null;
            row.Tarjeta = null;
            row.MedioPago = null;
            row.TransferenciaMonto = null;
            row.TransferenciaNombre = null;
        }

        private bool IsRowEmpty(OrdenRow row)
        {
            return string.IsNullOrWhiteSpace(row.NroOrden)
                && string.IsNullOrWhiteSpace(row.Servicio)
                && string.IsNullOrWhiteSpace(row.Efectivo)
                && string.IsNullOrWhiteSpace(row.Seña)
                && string.IsNullOrWhiteSpace(row.Tarjeta)
                && string.IsNullOrWhiteSpace(row.MedioPago)
                && string.IsNullOrWhiteSpace(row.TransferenciaMonto)
                && string.IsNullOrWhiteSpace(row.TransferenciaNombre);
        }

        public bool CanEditField(OrdenRow row, string propertyName)
        {
            if (propertyName == TriggerColumn) return true;
            return !string.IsNullOrWhiteSpace(
                (string)row.GetType().GetProperty(TriggerColumn)?.GetValue(row));
        }

        // NUEVO: reemplaza todas las filas actuales por las provistas (usado para recuperación)
        public void CargarFilas(IEnumerable<OrdenRow> filas)
        {
            // Desuscribir y limpiar filas actuales
            foreach (var r in Rows.ToList())
            {
                r.PropertyChanged -= Row_PropertyChanged;
                Rows.Remove(r);
            }

            // Cargar las filas recuperadas
            foreach (var f in filas)
            {
                f.PropertyChanged += Row_PropertyChanged;
                Rows.Add(f);
            }

            // Asegurar que siempre haya una fila vacía al final
            if (Rows.Count == 0 || !IsRowEmpty(Rows[Rows.Count - 1]))
                AddNewRow();
        }

        public DynamicDataGrid()
        {
            InitializeComponent();
            ColumnHeaders = new ObservableCollection<string>();
            AddNewRow();
            Loaded += (s, e) => RebuildColumns();
        }
    }
}