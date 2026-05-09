using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.DirectoryServices;
using System.Windows;
using System.Windows.Controls;
using WPF_Test.Models;

namespace WPF_Test.Vista.UserControls
{
    public partial class DynamicDataGrid : UserControl
    {
        // Propiedad: Columnas definidas desde el XAML
        public static readonly DependencyProperty ColumnHeadersProperty =
            DependencyProperty.Register(
                nameof(ColumnHeaders),
                typeof(ObservableCollection<string>),
                typeof(DynamicDataGrid),
                new PropertyMetadata(null, OnColumnHeadersChanged));

        public ObservableCollection<string> ColumnHeaders
        {
            get => (ObservableCollection<string>)GetValue(ColumnHeadersProperty);
            set => SetValue(ColumnHeadersProperty, value);
        }

        // Propiedad: columna que dispara nueva fila
        public static readonly DependencyProperty TriggerColumnProperty =
            DependencyProperty.Register(
                nameof(TriggerColumn),
                typeof(string),
                typeof(DynamicDataGrid),
                new PropertyMetadata(null));

        /// <summary>
        /// Nombre de la propiedad del modelo que, al cambiar, genera una nueva fila.
        /// Ej: "NroOrden"
        /// </summary>
        public string TriggerColumn
        {
            get => (string)GetValue(TriggerColumnProperty);
            set => SetValue(TriggerColumnProperty, value);
        }

        // Colección interna de filas
        public ObservableCollection<OrdenRow> Rows { get; } = new();

        public DynamicDataGrid()
        {
            InitializeComponent();
            ColumnHeaders = new ObservableCollection<string>();
            AddNewRow(); // fila inicial

            Loaded += (s, e) => RebuildColumns();
        }

        // ── Cuando cambian los headers, regenerar columnas ──
        private static void OnColumnHeadersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DynamicDataGrid control && control.IsLoaded)
                control.RebuildColumns();
        }

        private void RebuildColumns()
        {
            InnerGrid.Columns.Clear();

            // Mapeo header → propiedad del modelo (por índice/posición)
            string[] propertyNames = {
                nameof(OrdenRow.NroOrden),
                nameof(OrdenRow.Efectivo),
                nameof(OrdenRow.Seña),
                nameof(OrdenRow.Tarjeta),
                nameof(OrdenRow.Transferencia)
            };

            int i = 0;
            foreach (var header in ColumnHeaders)
            {
                if (i >= propertyNames.Length) break;

                InnerGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = header,
                    Binding = new System.Windows.Data.Binding(propertyNames[i]),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                });
                i++;
            }
        }

        // ── Agregar fila y suscribirse a sus cambios ──
        private void AddNewRow()
        {
            var row = new OrdenRow();
            row.PropertyChanged += Row_PropertyChanged;
            Rows.Add(row);
        }

        private void Row_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Solo reacciona a la columna disparadora configurada
            if (e.PropertyName != TriggerColumn) return;

            var changedRow = (OrdenRow)sender;
            int index = Rows.IndexOf(changedRow);

            if (!string.IsNullOrWhiteSpace(changedRow.NroOrden))
            {
                // Si es la última fila y tiene valor → agregar nueva
                if (index == Rows.Count - 1)
                    AddNewRow();
            }
            else
            {
                // Si el campo trigger se vació, eliminar filas vacías sobrantes
                // (excepto la primera o cualquiera que no sea la última)
                RemoveTrailingEmptyRows();
            }
        }

        private void RemoveTrailingEmptyRows()
        {
            // Recorre desde el final hacia atrás, eliminando filas vacías
            // pero siempre dejando al menos una fila
            for (int i = Rows.Count - 1; i >= 1; i--)
            {
                if (IsRowEmpty(Rows[i]))
                {
                    Rows[i].PropertyChanged -= Row_PropertyChanged;
                    Rows.RemoveAt(i);
                }
                else
                {
                    break; // Encuentra una fila con datos, se detiene
                }
            }
        }

        private bool IsRowEmpty(OrdenRow row)
        {
            return string.IsNullOrWhiteSpace(row.NroOrden)
                && string.IsNullOrWhiteSpace(row.Efectivo)
                && string.IsNullOrWhiteSpace(row.Seña)
                && string.IsNullOrWhiteSpace(row.Tarjeta)
                && string.IsNullOrWhiteSpace(row.Transferencia);
        }
    }
    }