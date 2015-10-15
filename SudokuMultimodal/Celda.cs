using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Media;
using System.Windows.Ink;
using System.Timers;

namespace SudokuMultimodal
{
    public class Celda
    {
        bool _tintaHabilitada;
        InkCanvas canvasDeTinta = new InkCanvas() { Visibility = Visibility.Hidden };
        public Border UI { get; private set; }
        Action _solicitudCambioTinta;
        InkAnalyzer m_analyzer;
        Timer temp;

        public bool EstáSeleccionada
        {
            get { return _estáSeleccionado; }
            set
            {
                _estáSeleccionado = value;
                selecciónBorde.Visibility = _estáSeleccionado ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public bool TintaHabilitada
        {
            get { return _tintaHabilitada; }
            set
            {
                _tintaHabilitada = value;
                if (_modificable)
                {
                    if (_tintaHabilitada || (_tintaHabilitada && _estáSeleccionado))
                    {
                        _textBlock.Visibility = Visibility.Hidden;
                        canvasDeTinta.Visibility = _tintaHabilitada ? Visibility.Visible : Visibility.Hidden;
                    }
                    else
                    {
                        _textBlock.Visibility = Visibility.Visible;
                        canvasDeTinta.Visibility = _tintaHabilitada ? Visibility.Visible : Visibility.Hidden;
                    }
                }
            }
        }

        public Celda(int número, Action<int> solicitudCambioNúmero, Action solicitudSeleccionada, Action solicitudTinta)
        {
            _solicitudCambioNúmero = solicitudCambioNúmero;
            _solicitudSeleccionada = solicitudSeleccionada;

            _solicitudCambioTinta = solicitudTinta;

            UI = new Border() { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0.5), Background=Brushes.Transparent };
            UI.MouseDown += new System.Windows.Input.MouseButtonEventHandler(UI_MouseDown);
            UI.MouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(UI_MouseRightButtonDown);
            var grid = new Grid();
            UI.Child = grid;
            grid.Children.Add(canvasDeTinta);



            grid.Children.Add(_uniformGrid);
            for (int i = 0; i < Sudoku.Tamaño; ++i)
                _uniformGrid.Children.Add(new TextBlock()
                {
                    FontFamily = _fuente,
                    FontSize = 40,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                });
            grid.Children.Add(_textBlock);
            grid.Children.Add(selecciónBorde);
            _modificable = número == 0;
            _textBlock.Foreground = _modificable ? Brushes.Blue : Brushes.Black;
            if (número != 0)
                ForzarPonerNúmero(número);



            
        }

        void UI_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _solicitudSeleccionada();
        }

        #region public

        public void FuncionalidadTinta()
        {
            temp = new System.Timers.Timer();
            temp.Elapsed += new ElapsedEventHandler(temp_Elapsed);
            temp.Interval = 1000;
            temp.Enabled = false;

            m_analyzer = new InkAnalyzer();
            m_analyzer.AnalysisModes = AnalysisModes.AutomaticReconciliationEnabled;
            m_analyzer.ResultsUpdated += new ResultsUpdatedEventHandler(m_analyzer_ResultsUpdated);

            canvasDeTinta.StrokeErasing += new InkCanvasStrokeErasingEventHandler(canvasDeTinta_StrokeErasing);
            canvasDeTinta.StrokeCollected += new InkCanvasStrokeCollectedEventHandler(canvasDeTinta_StrokeCollected);
            canvasDeTinta.PreviewMouseDown += new System.Windows.Input.MouseButtonEventHandler(canvasDeTinta_PreviewMouseDown);
        }

        void temp_Elapsed(object sender, ElapsedEventArgs e)
        {
            m_analyzer.BackgroundAnalyze();
        }

        void canvasDeTinta_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            temp.Stop();
        }

        void canvasDeTinta_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            m_analyzer.AddStroke(e.Stroke);
            temp.Start();
        }

        void canvasDeTinta_StrokeErasing(object sender, InkCanvasStrokeErasingEventArgs e)
        {
            m_analyzer.RemoveStroke(e.Stroke);
        }

        void m_analyzer_ResultsUpdated(object sender, ResultsUpdatedEventArgs e)
        {
            if (e.Status.Successful)
            {
                ContextNodeCollection nodes = ((InkAnalyzer)sender).FindLeafNodes();

                foreach (ContextNode node in nodes)
                {
                    if (node is InkWordNode)
                    {
                        InkWordNode t = node as InkWordNode;

                        var num = t.GetRecognizedString();
                        int valor;
                        bool isNum = int.TryParse(num, out valor);

                        if (isNum)
                        {
                            valor = Convert.ToInt32(num);
                            if (valor > 0 && valor < 10)
                            {
                                _solicitudCambioNúmero(valor);
                            }

                        }

                       
                    }
                    else
                    {
                        m_analyzer.RemoveStrokes(canvasDeTinta.Strokes);
                        canvasDeTinta.Strokes.Clear();
                    }
                    m_analyzer.RemoveStrokes(canvasDeTinta.Strokes);
                    canvasDeTinta.Strokes.Clear();
                }
            }
            m_analyzer.RemoveStrokes(canvasDeTinta.Strokes);
            canvasDeTinta.Strokes.Clear();
        }

        void UI_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _solicitudCambioTinta();
        }

        public void PonerNúmero(int número)
        {
            if (!_modificable) return;
            ForzarPonerNúmero(número);
        }

        public void QuitarNúmero()
        {
            if (!_modificable) return;
            _textBlock.Text = "";
            _textBlock.Visibility = Visibility.Hidden;
            _uniformGrid.Visibility = Visibility.Visible;
        }

        public void PonerPosible(int número)
        {
            if (!_modificable) return;
            (_uniformGrid.Children[número - 1] as TextBlock).Text = número.ToString();
        }

        public void QuitarPosible(int número)
        {
            if (!_modificable) return;
            (_uniformGrid.Children[número - 1] as TextBlock).Text = "";
        }

        public void QuitarTodosPosibles()
        {
            for (int número = 1; número <= Sudoku.Tamaño; ++número)
                QuitarPosible(número);
        }

        #endregion

        #region private

        Action<int> _solicitudCambioNúmero;
        Action _solicitudSeleccionada;
        static FontFamily _fuente = new FontFamily("Comic Sans MS");
        bool _estáSeleccionado;

        Border selecciónBorde = new Border() { BorderBrush = Brushes.Red, BorderThickness = new Thickness(2), Visibility = Visibility.Hidden };

        UniformGrid _uniformGrid = new UniformGrid() { Rows = Sudoku.Tamaño / 3, Columns = Sudoku.Tamaño / 3 };

        TextBlock _textBlock = new TextBlock()
        {
            Visibility = Visibility.Hidden,
            FontFamily = _fuente,
            FontSize = 160,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        bool _modificable;

        void ForzarPonerNúmero(int número)
        {
            _textBlock.Visibility = Visibility.Visible;
            _textBlock.Text = número.ToString();
            _uniformGrid.Visibility = Visibility.Hidden;
        }

        #endregion
    }
}
