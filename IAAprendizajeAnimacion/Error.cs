using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAAprendizajeAnimacion
{
    internal class Error
    {
        public Error(Single peso, Single sesgo, Single errorCuadraticoMedio) 
        { 
            this.Peso = peso;
            this.Sesgo = sesgo;
            this.ErrorCuadraticoMedio = errorCuadraticoMedio;
        }

        public Single Peso {  get; set; }
        public Single Sesgo { get; set; }
        public Single ErrorCuadraticoMedio { get; set; }
    }
}
