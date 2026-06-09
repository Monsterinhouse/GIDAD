using System.Collections.Generic;
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

        // ── ColumnHeaders ──
        public ObservableCollection<string> ColumnHeaders
        {
            get => (ObservableCollection<string>)GetValue(ColumnHeadersProperty);
            set => SetValue(ColumnHeadersProperty, value);
        }

        public static readonly DependencyProperty ColumnHeadersProperty =
            DependencyProperty.Register(
                nameof(ColumnHeaders),
                typeof(ObservableCollection<string>),
                typeof(DynamicDataGrid),
                new PropertyMetadata(null, OnColumnHeadersChanged));

        // ── TriggerColumn ──
        public string TriggerColumn
        {
            get => (string)GetValue(TriggerColumnProperty);
            set => SetValue(TriggerColumnProperty, value);
        }

        public static readonly DependencyProperty TriggerColumnProperty =
            DependencyProperty.Register(
                nameof(TriggerColumn),
                typeof(string),
                typeof(DynamicDataGrid),
                new PropertyMetadata(null));

        // ── ServicioComboOptions ──
        public IEnumerable<string> ServicioComboOptions
        {
            get => (IEnumerable<string>)GetValue(ServicioComboOptionsProperty);
            set => SetValue(ServicioComboOptionsProperty, value);
        }

        public static readonly DependencyProperty ServicioComboOptionsProperty =
            DependencyProperty.Register(
                nameof(ServicioComboOptions),
                typeof(IEnumerable<string>),
                typeof(DynamicDataGrid),
                new PropertyMetadata(null));

        // ── Colección interna de filas ──
        public ObservableCollection<OrdenRow> Rows { get; } = new();

        // Clave interna para identificar la columna de Transferencia
        private const string TransferenciaColumnKey = "TRANSFERENCIA";

        // ── Cuando cambian los headers, regenerar columnas ──
        private static void OnColumnHeadersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DynamicDataGrid control && control.IsLoaded)
                control.RebuildColumns();
        }

        private void RebuildColumns()
        {
            if (ColumnHeaders == null || InnerGrid == null) return;

            InnerGrid.Columns.Clear();

            string[] propertyNames =
            {
                nameof(OrdenRow.NroOrden),
                nameof(OrdenRow.Servicio),
                nameof(OrdenRow.Efectivo),
                nameof(OrdenRow.Seña),
                nameof(OrdenRow.Tarjeta),
                TransferenciaColumnKey      // señal para columna doble
            };

            int i = 0;
            foreach (var header in ColumnHeaders)
            {
                if (i >= propertyNames.Length) break;

                var propName = propertyNames[i];

                // CORRECCIÓN: if / else if / else correctamente encadenados
                if (propName == TransferenciaColumnKey)
                {
                    InnerGrid.Columns.Add(BuildTransferenciaColumn(header));
                }
                else if (propName == nameof(OrdenRow.Servicio) && ServicioComboOptions != null)
                {
                    InnerGrid.Columns.Add(BuildServicioColumn(header));
                }
                else
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

                i++;
            }
        }

        private DataGridTemplateColumn BuildTransferenciaColumn(string header)
        {
            // ── Vista (solo lectura): muestra Monto — Nombre ──
            var displayTemplate = new DataTemplate();
            var displayPanel = new FrameworkElementFactory(typeof(StackPanel));
            displayPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var montoText = new FrameworkElementFactory(typeof(TextBlock));
            montoText.SetBinding(TextBlock.TextProperty,
                new Binding(nameof(OrdenRow.TransferenciaMonto)));

            var sep = new FrameworkElementFactory(typeof(TextBlock));
            sep.SetValue(TextBlock.TextProperty, " — ");

            var nombreText = new FrameworkElementFactory(typeof(TextBlock));
            nombreText.SetBinding(TextBlock.TextProperty,
                new Binding(nameof(OrdenRow.TransferenciaNombre)));

            displayPanel.AppendChild(montoText);
            displayPanel.AppendChild(sep);
            displayPanel.AppendChild(nombreText);
            displayTemplate.VisualTree = displayPanel;

            // ── Edición: TextBox de monto + TextBox de nombre ──
            var editTemplate = new DataTemplate();
            var editPanel = new FrameworkElementFactory(typeof(StackPanel));

            var montoBox = new FrameworkElementFactory(typeof(TextBox));
            montoBox.SetBinding(TextBox.TextProperty,
                new Binding(nameof(OrdenRow.TransferenciaMonto))
                {
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

            var nombreBox = new FrameworkElementFactory(typeof(TextBox));
            nombreBox.SetBinding(TextBox.TextProperty,
                new Binding(nameof(OrdenRow.TransferenciaNombre))
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

        private DataGridTemplateColumn BuildServicioColumn(string header)
        {
            // ── Vista (solo lectura) ──
            var displayTemplate = new DataTemplate();
            var displayFactory = new FrameworkElementFactory(typeof(TextBlock));
            displayFactory.SetBinding(TextBlock.TextProperty,
                new Binding(nameof(OrdenRow.Servicio)));
            displayTemplate.VisualTree = displayFactory;

            // ── Edición: ComboBox ──
            var editTemplate = new DataTemplate();
            var comboFactory = new FrameworkElementFactory(typeof(ComboBox));
            comboFactory.SetValue(ComboBox.ItemsSourceProperty, ServicioComboOptions);
            comboFactory.SetBinding(ComboBox.SelectedItemProperty,
                new Binding(nameof(OrdenRow.Servicio))
                {
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.TwoWay
                });

            // Abre el dropdown automáticamente al entrar en edición
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
                else
                {
                    break;
                }
            }
        }

        private void ClearRow(OrdenRow row)
        {
            row.NroOrden = null;
            row.Servicio = null;
            row.Efectivo = null;
            row.Seña = null;
            row.Tarjeta = null;
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
                && string.IsNullOrWhiteSpace(row.TransferenciaMonto)
                && string.IsNullOrWhiteSpace(row.TransferenciaNombre);
        }

        public bool CanEditField(OrdenRow row, string propertyName)
        {
            if (propertyName == TriggerColumn) return true;

            return !string.IsNullOrWhiteSpace(
                (string)row.GetType().GetProperty(TriggerColumn)?.GetValue(row));
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