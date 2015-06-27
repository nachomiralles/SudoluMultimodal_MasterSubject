using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Speech.Recognition;
using WiimoteLib;
using WiiGestureLib;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SudokuMultimodal
{
   
    public partial class MainWindow : Window
    {

        Wiimote wiiMote;
        GestureCapturer gestureCapturer;
        GestureRecognizer gestureRecognizer;
        SpeechRecognitionEngine speechRecognizer;
        int _ultimaFila = 0; 
        int _ultimaColumna = 0; 
        int _ultimoValor = 0; 
        public MainWindow()
        {
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            InitializeComponent();
        }
        // RATON
        public void FuncionalidadRaton()
        {

            RowDefinition rd = new RowDefinition();
            rd.Height = new GridLength(200);
            numerosGrid.RowDefinitions.Add(rd);
            for (int i = 1; i <= 11; i++)
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength(200);
                numerosGrid.ColumnDefinitions.Add(cd);
            }

            FontFamily fuente = new FontFamily("Comic Sans MS");
            for (int i = 1; i <= 11; i++)
            {
                Button b = new Button();
                b.Name = "boton" + i;
                b.FontFamily = fuente;
                b.FontSize = 160;


                if (i == 11)
                {
                    b.Content = ((char)0x2205).ToString();
                }
                else if (i == 10)
                {
                    b.Content = ((char)0x21D0).ToString();
                }
                else
                {
                    b.Content = i;
                }
                numerosGrid.Children.Add(b);

                b.Click += b_Click;
                Grid.SetColumn(b, i - 1);
                Grid.SetRow(b, 0);

            }
        }

        void b_Click(object sender, RoutedEventArgs e)
        {
            Button botonClicado = sender as Button;
            if (botonClicado.Content.ToString() == ((char)0x2205).ToString())
            {
                //BORRAR
                _ultimaFila = _filaActual;
                _ultimaColumna = _columnaActual;
                _ultimoValor = _s[_filaActual,_columnaActual];
                _s[_filaActual, _columnaActual] = 0;
                
            }
            else if (botonClicado.Content.ToString() == ((char)0x21D0).ToString())
            {
                //DESHACER
                PonSelecciónEn(_ultimaFila, _ultimaColumna);
                _s[_filaActual, _columnaActual] = _ultimoValor;
                _ultimaFila = _filaActual;
                _ultimaColumna = _columnaActual;
            }
            else
            {
                int numero = int.Parse(botonClicado.Content.ToString());
                _ultimaFila = _filaActual;
                _ultimaColumna = _columnaActual;
                _ultimoValor = _s[_filaActual, _columnaActual];
                _s[_filaActual, _columnaActual] = numero;
            }
            ActualizaPosibles();
        }
        //Fin  RATON


        // VOZ
        void FuncionalidadVoz()
        {
            Grammar grammar = new Grammar("gramaticaSudoku.xml");
            speechRecognizer = new SpeechRecognitionEngine();
            speechRecognizer.LoadGrammar(grammar);
            speechRecognizer.SpeechRecognized += SpeechRecognized;
            speechRecognizer.SetInputToDefaultAudioDevice();
            speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {

            var num = int.Parse(e.Result.Semantics["numero"].Value.ToString());
            if (num == -1)
            {
                //DESHACER
                PonSelecciónEn(_ultimaFila, _ultimaColumna);
                _s[_filaActual, _columnaActual] = _ultimoValor;
                _ultimaFila = _filaActual;
                _ultimaColumna = _columnaActual;
            }
            else if (num == 0)
            {
                //BORRAR
                _ultimaFila = _filaActual;
                _ultimaColumna = _columnaActual;
                _ultimoValor = _s[_filaActual, _columnaActual];
                _s[_filaActual, _columnaActual] = 0;
            }
            else
            {
                //NUMERO
                if (e.Result.Semantics.Count > 1)
                {
                    var col = int.Parse(e.Result.Semantics["columna"].Value.ToString());
                    var fil = int.Parse(e.Result.Semantics["fila"].Value.ToString());
                    PonSelecciónEn(fil - 1, col - 1);
                }
                _ultimaFila = _filaActual;
                _ultimaColumna = _columnaActual;
                _ultimoValor = _s[_filaActual, _columnaActual];
                _s[_filaActual, _columnaActual] = num;
            }
            ActualizaPosibles();

        }
        // Fin  VOZ

        //WiiMOTE
        public void FuncionalidadWiiMote()
        {
            wiiMote = new Wiimote();
            gestureCapturer = new GestureCapturer();
            gestureRecognizer = new GestureRecognizer();
            cargarFicheroGestos();
            wiiMote.WiimoteChanged += new EventHandler<WiimoteChangedEventArgs>(wm_WiimoteChanged);
            gestureCapturer.GestureCaptured += new GestureCapturedEventHandler(gc_GestureCaptured);
            gestureRecognizer.GestureRecognized += new GestureRecognizedEventHandler(gr_GestureRecognized);
            wiiMote.Connect();

        }

        private void cargarFicheroGestos()
        {
            List<Gesture> gestures;

            using (Stream stream = new FileStream("gestosWii", FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter bf = new BinaryFormatter();
                gestures = bf.Deserialize(stream) as List<Gesture>;
            }

            foreach (Gesture g in gestures)
            {
                gestureRecognizer.AddPrototype(g);
            }
        }

        void gr_GestureRecognized(string gestureName)
        {
            Dispatcher.BeginInvoke(new Action<String>(RecibeEstado), gestureName);
        }

        void gc_GestureCaptured(Gesture gesture)
        {
            gestureRecognizer.OnGestureCaptured(gesture);
        }

        void wm_WiimoteChanged(object sender, WiimoteChangedEventArgs e)
        {

            if (e.WiimoteState.ButtonState.A)
                Dispatcher.BeginInvoke(new Action(click_botonA));
            if (e.WiimoteState.ButtonState.Up)
                Dispatcher.BeginInvoke(new Action(wiimote_ARRIBA));
            if (e.WiimoteState.ButtonState.Down)
                Dispatcher.BeginInvoke(new Action(wiimote_ABAJO));
            if (e.WiimoteState.ButtonState.Left)
                Dispatcher.BeginInvoke(new Action(wiimote_IZQUIERDA));
            if (e.WiimoteState.ButtonState.Right)
                Dispatcher.BeginInvoke(new Action(wiimote_DERECHA));

            gestureCapturer.OnWiimoteChanged(e.WiimoteState);
        }

        void click_botonA()
        {
            _s[_filaActual, _columnaActual] = 0;
            ActualizaPosibles();
        }

        void wiimote_ARRIBA()
        {
            if (_filaActual > 0)
            {
                PonSelecciónEn(_filaActual - 1, _columnaActual);
            }
        }

        void wiimote_ABAJO()
        {
            if (_filaActual < Sudoku.Tamaño - 1)
            {
                PonSelecciónEn(_filaActual + 1, _columnaActual);
            }
        }

        void wiimote_IZQUIERDA()
        {
            if (_columnaActual > 0)
                PonSelecciónEn(_filaActual, _columnaActual - 1);
        }

        void wiimote_DERECHA()
        {
            if (_columnaActual < Sudoku.Tamaño - 1)
                PonSelecciónEn(_filaActual, _columnaActual + 1);
        }

        void RecibeEstado(string numero)
        {
            int valor = int.Parse(numero);
            SolicitudCambioNúmero(_filaActual, _columnaActual, valor);
        }
        // Fin WiiMOTE

        //TINTA
        void PonTintaEn(int fil, int col)
        {
            if (_filaActual >= 0 && _columnaActual >= 0)
            {
                int cuad, pos;
                Sudoku.FilaColumnaACuadrantePosicion(_filaActual, _columnaActual, out cuad, out pos);
                _cuadrantes[cuad].QuitarTinta(pos);
            }

            int cuad2, pos2;
            Sudoku.FilaColumnaACuadrantePosicion(fil, col, out cuad2, out pos2);
            _cuadrantes[cuad2].PonerTinta(pos2);
        }

        void SolicitudTinta(int fila, int col)
        {
            PonTintaEn(fila, col);
        }

        //Fin TINTA

        #region private

        Sudoku _s;
        Cuadrante[] _cuadrantes;
        UniformGrid _ug;
        int _filaActual, _columnaActual;
        bool mostrarPosibles;

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FuncionalidadVoz(); // VOZ
            FuncionalidadRaton(); // RATON
            FuncionalidadWiiMote(); // WiiMOTE


            mostrarPosibles = false;
            KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            _ug = new UniformGrid() { Rows = Sudoku.Tamaño / 3, Columns = Sudoku.Tamaño / 3, Background = Brushes.WhiteSmoke };
            mainGrid.Children.Add(_ug);
            _cuadrantes = new Cuadrante[Sudoku.Tamaño];


            NuevaPartida();
        }

        void NuevaPartida() 
        {
            _filaActual = _columnaActual = -1;
            _s = new Sudoku();
            _s.CeldaCambiada += CuandoCeldaCambiada;

            ActualizarVistaSudoku();

            _s[0, 0] = 2;
            _s[4, 4] = 1;
            _s[7, 7] = 5;
            _s[1, 7] = 8;
            _s[7, 1] = 4;
            _s[3, 2] = 6;
            _s[5, 6] = 9;
        }

        void ReiniciarPartida()
        {
            _s.Reiniciar();
            ActualizarVistaSudoku();
            FuncionalidadVoz(); 
        }

        void ActualizarVistaSudoku()
        {
            _ug.Children.Clear();

            for (int cuad = 0; cuad < Sudoku.Tamaño; ++cuad)
            {
                var cuadrante = new Cuadrante(_s, cuad, SolicitudCambioNúmero, SolicitudSeleccionada, SolicitudTinta);
                _cuadrantes[cuad] = cuadrante;
                _ug.Children.Add(cuadrante.UI);
            }

            ActualizaPosibles();
            PonSelecciónEn(0, 0);
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                case Key.Back:
                    {
                        _s[_filaActual, _columnaActual] = 0;
                        ActualizaPosibles();
                    }
                    break;
                case Key.Up:
                    if (_filaActual > 0)
                        PonSelecciónEn(_filaActual - 1, _columnaActual);
                    break;
                case Key.Down:
                    if (_filaActual < Sudoku.Tamaño - 1)
                        PonSelecciónEn(_filaActual + 1, _columnaActual);
                    break;
                case Key.Left:
                    if (_columnaActual > 0)
                        PonSelecciónEn(_filaActual, _columnaActual - 1);
                    break;
                case Key.Right:
                    if (_columnaActual < Sudoku.Tamaño - 1)
                        PonSelecciónEn(_filaActual, _columnaActual + 1);
                    break;
                case Key.D1:
                case Key.NumPad1:
                case Key.D2:
                case Key.NumPad2:
                case Key.D3:
                case Key.NumPad3:
                case Key.D4:
                case Key.NumPad4:
                case Key.D5:
                case Key.NumPad5:
                case Key.D6:
                case Key.NumPad6:
                case Key.D7:
                case Key.NumPad7:
                case Key.D8:
                case Key.NumPad8:
                case Key.D9:
                case Key.NumPad9:
                    {
                        var num = int.Parse(new string(e.Key.ToString()[1], 1));
                        _ultimaFila = _filaActual;
                        _ultimaColumna = _columnaActual;
                        _ultimoValor = _s[_filaActual, _columnaActual];
                        _s[_filaActual, _columnaActual] = num;

                        ActualizaPosibles();
                        e.Handled = true;
                    }
                    break;
                default:
                    break;
            }
        }

        void PonSelecciónEn(int fil, int col)
        {
            if (_filaActual >= 0 && _columnaActual >= 0)
            {
                int cuad, pos;
                Sudoku.FilaColumnaACuadrantePosicion(_filaActual, _columnaActual, out cuad, out pos);
                _cuadrantes[cuad].DeseleccionaCelda(pos);
                _cuadrantes[cuad].QuitarTinta(pos);
            }

            int cuad2, pos2;
            Sudoku.FilaColumnaACuadrantePosicion(fil, col, out cuad2, out pos2);
            _cuadrantes[cuad2].SeleccionaCelda(pos2);
            _filaActual = fil;
            _columnaActual = col;
        }

        void ActualizaPosibles()
        {
            for (int cuad = 0; cuad < Sudoku.Tamaño; ++cuad)
                _cuadrantes[cuad].QuitaTodosPosibles();

            if (!mostrarPosibles) return;

            for (int f = 0; f < Sudoku.Tamaño; ++f)
                for (int c = 0; c < Sudoku.Tamaño; ++c)
                {
                    int cuad, pos;
                    Sudoku.FilaColumnaACuadrantePosicion(f, c, out cuad, out pos);
                    foreach (var num in _s.PosiblesEnCelda(f, c))
                        _cuadrantes[cuad].PonerPosibleEnPos(pos, num);
                }
        }

        void CuandoCeldaCambiada(int fila, int columna, int nuevoNúmero)
        {
            int cuad, pos;
            Sudoku.FilaColumnaACuadrantePosicion(fila, columna, out cuad, out pos);
            if (nuevoNúmero > 0)
                _cuadrantes[cuad].PonerNúmeroEnPos(pos, nuevoNúmero);
            else
                _cuadrantes[cuad].QuitarNúmeroEnPos(pos);
            ActualizaPosibles();
        }

        void SolicitudCambioNúmero(int fila, int col, int número)
        {
            _s[fila, col] = número;
        }

        void SolicitudSeleccionada(int fila, int col)
        {
            PonSelecciónEn(fila, col);
        }

        void botónNuevoClick(object sender, RoutedEventArgs e)
        {
            NuevaPartida();
        }

        void botónReiniciarClick(object sender, RoutedEventArgs e)
        {
            ReiniciarPartida();
        }

        void checkboxVerPosiblesClick(object sender, RoutedEventArgs e)
        {
            mostrarPosibles = (sender as CheckBox).IsChecked == true;
            ActualizaPosibles();
        }

        #endregion
    }
}
