using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Media;

namespace SudokuMultimodal
{
    public class Cuadrante
    {
        public Border UI { get; private set; }

        public Cuadrante(Sudoku s, int cuad, Action<int, int, int> solicitudCambioNúmero, Action<int, int> solicitudSeleccionada, Action<int, int> solicitudCambioTinta)
        {
            var ug = new UniformGrid() { Rows = Sudoku.Tamaño / 3, Columns = Sudoku.Tamaño / 3 };
            UI = new Border()
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                Child = ug
            };

            for (int i = 0; i < Sudoku.Tamaño; ++i)
            {
                int f, c;
                Sudoku.CuadrantePosicionAFilaColumna(cuad, i, out f, out c);
                var celda = new Celda(s[f, c], (n) => solicitudCambioNúmero(f, c, n), () => solicitudSeleccionada(f, c), () => solicitudCambioTinta(f, c));
                _celdas[i] = celda;
                ug.Children.Add(celda.UI);
            }
        }

        #region public

        public void PonerTinta(int pos)
        {
            _celdas[pos].TintaHabilitada = true;
        }

        public void QuitarTinta(int pos)
        {
            _celdas[pos].TintaHabilitada = false;
        }

        public void PonerNúmeroEnPos(int pos, int número)
        {
            _celdas[pos].PonerNúmero(número);
        }

        public void QuitarNúmeroEnPos(int pos)
        {
            _celdas[pos].QuitarNúmero();
        }

        public void PonerPosibleEnPos(int pos, int número)
        {
            _celdas[pos].PonerPosible(número);
        }

        public void QuitarPosibleEnPos(int pos, int número)
        {
            _celdas[pos].QuitarPosible(número);
        }

        public void QuitaTodosPosibles()
        {
            for (int pos = 0; pos < Sudoku.Tamaño; ++pos)
                _celdas[pos].QuitarTodosPosibles();
        }

        public void SeleccionaCelda(int pos)
        {
            _celdas[pos].EstáSeleccionada = true;
        }

        public void DeseleccionaCelda(int pos)
        {
            _celdas[pos].EstáSeleccionada = false;
        }


        #endregion

        #region private

        Celda[] _celdas = new Celda[Sudoku.Tamaño];

        #endregion
    }
}
