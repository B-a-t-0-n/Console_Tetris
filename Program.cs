using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection.PortableExecutable;

namespace Console_Tetris
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Tetris tetris = new Tetris();
            tetris.StartGame();
            
        }
    }
}