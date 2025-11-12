using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IAAprendizajeAnimacion
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PocicionarValores();
            pictureBoxStop.Top = pictureBoxAnimada.Top;
            pictureBoxStop.Left = pictureBoxAnimada.Left;
            pictureBoxStop.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBoxStop.Visible = true;
            pictureBoxAnimada.Visible = false;
            pictureBoxButonRun.Visible = false;
            pictureBoxButonStop.Visible = false;
            VariablesVisibles(false);
            IsStop = true;
            IsExecuting = false;
            RefrescarAnimacion();
        }

        private void VariablesVisibles(bool visible)
        {
            labelError.Visible = visible;
            labelPeso.Visible = visible;
            labelSesgo.Visible = visible;
            listBoxEntrada.Visible = visible;
            listBoxSalida.Visible = visible;
        }

        /// <summary>
        /// Pociciona los valorews sobre la animacion.
        /// </summary>
        private void PocicionarValores()
        {
            int deltaX = pictureBoxAnimada.Width / 100;
            int deltaY = pictureBoxAnimada.Height / 100;

            listBoxEntrada.Left = pictureBoxAnimada.Left + 4 * deltaX;
            listBoxEntrada.Top = pictureBoxAnimada.Top + 40 * deltaY;

            listBoxSalida.Left = pictureBoxAnimada.Left + 75 * deltaX;
            listBoxSalida.Top = pictureBoxAnimada.Top + 40 * deltaY;

            labelPeso.Left = pictureBoxAnimada.Left + 30 * deltaX;
            labelPeso.Top = pictureBoxAnimada.Top + 45 * deltaY;

            labelSesgo.Left = pictureBoxAnimada.Left + 30 * deltaX;
            labelSesgo.Top = pictureBoxAnimada.Top + 58 * deltaY;

            labelError.Left = pictureBoxAnimada.Left + 62 * deltaX;
            labelError.Top = pictureBoxAnimada.Top + 86 * deltaY;

            this.Width = pictureBoxAnimada.Left + pictureBoxAnimada.Width + 10 * deltaX;
            this.Height = pictureBoxAnimada.Top + pictureBoxAnimada.Height + 15 * deltaY;
        }

        private void RefrescarAnimacion()
        {            
            pictureBoxStop.Visible = IsStop;
            pictureBoxAnimada.Visible = !IsStop;
            buttonFrenarContinuar.Text = (IsStop) ? "Ejecutar" : "Frenar";
            if(IsStop)            
                buttonFrenarContinuar.Image = pictureBoxButonRun.Image;            
            else
                buttonFrenarContinuar.Image = pictureBoxButonStop.Image;
        }

        private async Task Play()
        {
            VariablesVisibles(true);
            RefrescarAnimacion();
            Single peso = Single.Parse(textBoxPesoIni.Text);            
            Single sesgo = Single.Parse(textBoxSesgoIni.Text);
            Single delta = 10F;
            Single iteraciones = 10000000;
            bool finalizar = false;
            const Single ERROR_MAXIMO = .0001F;

            List<Salida> salidas = GetSalidas(DatosDeEntrenamiento, peso, sesgo);
            Single errorActual = ErrorCuadraticoMedio(salidas);
            do
            {
                await Retardo();

                Error errorMinimo = GetMinErrorAllDirecciones(peso, sesgo, delta);
                Single nuevoError = errorMinimo.ErrorCuadraticoMedio;

                if (nuevoError < errorActual)
                {
                    //disminuye el error cuadrático medio
                    peso = errorMinimo.Peso;
                    sesgo = errorMinimo.Sesgo;
                    errorActual = errorMinimo.ErrorCuadraticoMedio;

                    RefrescarIU(peso, sesgo, errorActual);
                }
                else
                {
                    //NO disminuye el error cuadrático medio
                    delta = delta * Convert.ToSingle(.5);
                }

                iteraciones--;

                finalizar = iteraciones <= 0 || errorActual < ERROR_MAXIMO;
            } while (!finalizar);

            //Fin del entrenamiento e inicio de la evaluación.
            Single errorDeEvaluacion = Evaluar(DatosDeEvalidacion, peso, sesgo);
        }

        private void RefrescarIU(Single peso, Single sesgo, Single errorActual)
        {
            labelPeso.Text = peso.ToString();
            labelSesgo.Text = sesgo.ToString();
            labelError.Text = errorActual.ToString("F10");

            //Salida
            listBoxSalida.Items.Clear();
            List<Salida> salidas = GetSalidas(DatosDeEntrenamiento, peso, sesgo);
            foreach (Salida salida in salidas)
            {
                listBoxSalida.Items.Add($"{salida.Muestra.Salida} - {salida.Valor}");
            }

            //Entrada
            listBoxEntrada.Items.Clear();            
            foreach (Muestra muestra in DatosDeEntrenamiento)
            {
                listBoxEntrada.Items.Add($"{muestra.Entrada}");
            }

            RefrescarAnimacion();
        }

        private async Task Retardo()
        {
            if (!IsStop)
                await Task.Delay(10);
            else
            {
                while (IsStop)
                {
                    await Task.Delay(1000);
                }
            }                
        }

        private Single Evaluar(List<Muestra> datosDeEvalidacion, Single peso, Single sesgo)
        {
            List<Salida> salidas = GetSalidas(datosDeEvalidacion, peso, sesgo);
            Single errorActual = ErrorCuadraticoMedio(salidas);
            return errorActual;
        }

        private List<Muestra> DatosDeEntrenamiento = new List<Muestra>()
        {
            new Muestra { Entrada = 0, Salida = 32 },
            new Muestra { Entrada = 10, Salida = 50 },
            new Muestra { Entrada = -10, Salida = 14 },
            new Muestra { Entrada = 100, Salida = 212 },
            new Muestra { Entrada = 300, Salida = 572 }
        };

        private List<Muestra> DatosDeEvalidacion = new List<Muestra>()
        {
            new Muestra { Entrada = -5, Salida = 23 },
            new Muestra { Entrada = 25, Salida = 77 },
            new Muestra { Entrada = 15, Salida = 59 },
            new Muestra { Entrada = 30, Salida = 86 },
            new Muestra { Entrada = -20, Salida = -4 },
            new Muestra { Entrada = 5, Salida = 41 },
            new Muestra { Entrada = 2, Salida = 35.6F },
            new Muestra { Entrada = 18, Salida = 64.4F },
            new Muestra { Entrada = 40, Salida = 104 },
            new Muestra { Entrada = -1, Salida = 30.2F }
        };

        private Error GetMinErrorAllDirecciones(Single peso, Single sesgo, Single delta)
        {
            int[] direcciones = { 0, 1, -1 };
            List<Error> errores = new List<Error>();
            foreach (int direccionPeso in direcciones)
            {
                foreach (int direccionSesgo in direcciones)
                {
                    bool hayAvance = !(direccionPeso == 0 && direccionSesgo == 0);
                    if (hayAvance)
                    {
                        Single deltaPeso = direccionPeso * delta;
                        Single deltaSesgo = direccionSesgo * delta;

                        Single pesoNuevo = peso + deltaPeso;
                        Single sesgoNuevo = sesgo + deltaSesgo;

                        List<Salida> salidas = GetSalidas(DatosDeEntrenamiento, pesoNuevo, sesgoNuevo);
                        float ecm = ErrorCuadraticoMedio(salidas);

                        Error error = new Error(pesoNuevo, sesgoNuevo, ecm);

                        errores.Add(error);
                    }
                }
            }
            var minimo = errores.Min(e => e.ErrorCuadraticoMedio);
            Error errorMinimo = errores.First(e => e.ErrorCuadraticoMedio == minimo);
            return errorMinimo;
        }

        private List<Salida> GetSalidas(List<Muestra> muestras, Single peso, Single sesgo)
        {
            List<Salida> salidas = new List<Salida>();
            foreach (Muestra muestra in muestras)
            {
                Single valorCalculado = Formula(peso, sesgo, muestra);
                Salida salida = new Salida() { Muestra = muestra, Valor = valorCalculado };
                salidas.Add(salida);
            }
            return salidas;
        }

        private Single ErrorCuadraticoMedio(List<Salida> salidas)
        {
            Single error = 0;
            foreach (Salida salida in salidas)
            {
                Single diferencia = Convert.ToSingle(Math.Pow(salida.Valor - salida.Muestra.Salida, 2));
                error += diferencia;
            }
            error = error / salidas.Count;
            return error;
        }

        private Single Formula(Single peso, Single sesgo, Muestra muestra)
        {
            //(10 °C × 1.8) + 32 = 50 °F
            //(10 °C × 9/5) + 32 = 50 °F
            Single valor = peso * muestra.Entrada + sesgo;
            return valor;
        }

        private async void buttonFrenarContinuar_Click(object sender, EventArgs e)
        {
            if(IsExecuting)
            {
                IsStop = !IsStop;                               
            }
            else
            {
                IsExecuting = true;
                IsStop = false;
                RefrescarAnimacion();
                await Play();
                IsStop = true;
                IsExecuting = false;                
            }
            RefrescarAnimacion();
        }

        private bool IsStop;
        private bool IsExecuting;
    }
}
