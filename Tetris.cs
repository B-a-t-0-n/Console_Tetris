using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace Console_Tetris
{
    internal class Tetris
    {
        // 1 - клетка занята 2 - клетка активна 0 - клетка пустая  3 - центр
        public int Score { get; private set; }
        private bool _isWorking;
        private int _speed;
        private Random _randFigure;
        private int _numFigure;
        private int _numCollor;
        private ConsoleColor[] collorFigure;
        private int[,] matrix;
        private TimerCallback tmUbdateScreen;
        private Timer? timerUbteteScreen;
        private TimerCallback tmUbdateMatrix;
        private Timer? timerUbteteMatrix;
        public int numberOfTurns;

        public Tetris() 
        {
            _isWorking = true;
            _speed = 700;
            Score = 0;
            _randFigure = new Random();
            collorFigure = new ConsoleColor[]
            {
                ConsoleColor.Red,
                ConsoleColor.Blue,
                ConsoleColor.DarkRed,
                ConsoleColor.Magenta,
                ConsoleColor.Green,
                ConsoleColor.Cyan,
                ConsoleColor.DarkYellow
            };
            matrix = new int[20, 10];

            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 10; j++)
                    matrix[i, j] = 0;
            


            tmUbdateScreen = new TimerCallback(Timer_tick_Ubdate_Screen);
            tmUbdateMatrix = new TimerCallback(Timer_Tick_Ubdate_Matrix);

        } 
       

        public void StartGame()
        {
            Console.CursorVisible = false;
            MenuOutput();
            timerUbteteScreen = new Timer(tmUbdateScreen, null, 0, 100);
            timerUbteteMatrix = new Timer(tmUbdateMatrix, null, 0, _speed);
            Management();
            Console.ReadLine();

        }

        private void GameOver()
        {
            if (matrix[1,4] == 1)
            {
                
                timerUbteteScreen.Dispose();
                timerUbteteMatrix.Dispose();
                Thread.Sleep(1000);
                Console.Clear();
                Console.WriteLine("\n============================\n" +
                                    "|press Entr to Restart Game|\n" +
                                    "============================");
                Console.ReadLine();
                Console.Clear();
                Tetris tetris = new Tetris();
                tetris.StartGame();
            }
        }

        private void Timer_Tick_Ubdate_Matrix(object? a)
        {
            bool isActive = false;

            foreach(var item in matrix)
            {
                if(item >= 2)
                    isActive = true;
            }

            if (isActive)
            {
                if (Check_Moving(0, 1))
                {
                    for (int i = 19; i >= 0; i--)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            if (matrix[i, j] >= 2)
                            {
                                matrix[i + 1, j] = matrix[i, j];
                                matrix[i, j] = 0;
                            }
                        }
                    }
                }
                else
                {
                    Fixing();
                }
            }
            else
            {
                Check_Line();
                GameOver();
                _numFigure = _randFigure.Next(1, 8);
                _numCollor = _randFigure.Next(1, 7);
                matrix[1, 4] = 3;
                InsertShape(GetFigure(_numFigure));
                numberOfTurns = 1;
            }
        }

        private void Timer_tick_Ubdate_Screen(object? a)
        {
            Console.SetCursorPosition(0, 0);
            GameScreen();
        }

        private void Fixing()
        {
            for (int i = 19; i >= 0; i--)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (matrix[i,j] >= 2)
                        matrix[i,j] = 1;
                }
            }
        }

        private void Check_Line()
        {
            
            for(int i = 0; i < 20; i++)
            {
                bool isFilled = true;
                for (int j = 0; j < 10; j++)
                {
                    if (matrix[i,j] != 1)
                    {
                        isFilled = false;
                    }
                    
                }
                if (isFilled)
                {
                    for(int j = 0; j < 10; j++)
                        matrix[i,j] = 0;

                    for(int n = i; n > 0; n--)
                    {
                        for(int m = 0; m < 10; m++)
                        {
                            matrix[n,m] = matrix[n - 1,m];
                            matrix[n - 1,m] = 0;
                        }
                    }
                    Score += 10;
                    if (Score >= 100 && Score % 100 == 0)
                    {
                        _speed -= 100;
                    }
                }
            }

        }

        private void Management()
        {
            while (_isWorking)
            {
                var key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.LeftArrow:
                        Moving(-1, 0);
                        break;
                    case ConsoleKey.RightArrow:
                        Moving(1, 0);
                        break;
                    case ConsoleKey.UpArrow:
                        Rotate();
                        break;
                    case ConsoleKey.DownArrow:
                        Timer_Tick_Ubdate_Matrix(null);
                        break;
                }
            }
        }
        
        private void Rotate()
        {
            int[,] matrixFigure = GetFigure(_numFigure);
            int length = matrixFigure.GetLength(0);
            if (_numFigure != 1)
            {
                for (int m = 0; m < numberOfTurns; m++)
                {
                    for (int i = 0; i < length / 2; i++)
                    {
                        for (int j = i; j < length - i - 1; j++)
                        {
                            int temp = matrixFigure[i, j];
                            matrixFigure[i, j] = matrixFigure[length - 1 - j, i];
                            matrixFigure[length - 1 - j, i] = matrixFigure[length - 1 - i, length - 1 - j];
                            matrixFigure[length - 1 - i, length - 1 - j] = matrixFigure[j, length - 1 - i];
                            matrixFigure[j, length - 1 - i] = temp;
                        }
                    }
                }
                numberOfTurns++;
                InsertShape(matrixFigure);
            }
        }

        void SearchForCenter(out int row, out int columns)
        {
            row = 0;
            columns = 0;
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (matrix[i, j] == 3)
                    {
                        row = i;
                        columns = j;
                    }
                }
            }
        }

        bool CheckInputMatrix(int[,] matrixFigure, int row, int columns)
        {
            int length = matrixFigure.GetLength(0);
            try
            {
                for (int i = -1; i < length - 1; i++)
                {
                    for (int j = -1; j < length - 1; j++)
                    {
                        if (matrix[row + i, columns + j] == 1 && matrixFigure[i + 1, j + 1] != 0)
                            return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void InsertShape(int[,] matrixFigure)
        {
            int length = matrixFigure.GetLength(0);
            int centerRowFigure, centerColumnFigure;
            SearchForCenter(out centerRowFigure, out centerColumnFigure);
            if (CheckInputMatrix(matrixFigure,centerRowFigure,centerColumnFigure))
            {
                if (_numFigure == 5 && numberOfTurns % 2 == 0)
                {
                    centerColumnFigure--;
                }

                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (matrix[i,j] >= 2)
                            matrix[i,j] = 0;
                    }
                }

                for (int i = -1; i < length - 1; i++)
                {
                    for (int j = -1; j < length - 1; j++)
                    {
                        if (matrixFigure[i + 1, j + 1] != 0)
                            matrix[centerRowFigure + i, centerColumnFigure + j] = matrixFigure[i + 1, j + 1];
                    }
                }
            }
            
        }

        private void Moving(int x, int y)
        {
            if(Check_Moving(x, y))
            {
                for (int i = 0; i < 20; i++)
                {
                    if(x == -1)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            if (matrix[i, j] >= 2)
                            {
                                matrix[i + y, j + x] = matrix[i, j];
                                matrix[i, j] = 0;
                            }
                        }
                    }
                    else if(x == 1)
                    {
                        for (int j = 9; j >= 0; j--)
                        {
                            if (matrix[i, j] >= 2)
                            {
                                matrix[i + y, j + x] = matrix[i, j];
                                matrix[i, j] = 0;
                            }
                        }
                    }
                }
            }
        }

        private bool Check_Moving(int x, int y)
        {
            try
            {
                bool hasPlace = true;
                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (matrix[i,j] >= 2 && matrix[i+y,j+x] == 1)
                        {
                            hasPlace = false; 
                            break;
                        }
                    }
                }
                return hasPlace;
            }
            catch
            {
                return false;
            }
        }

        private void MenuOutput()
        {
            bool isSelected = false;//проверяет пользователь выбрал что то или нет
            bool isStart = true;//отслеживание кнопки
            
            do
            {
                //вывод кнопок
                if (isStart)
                    Console.BackgroundColor = ConsoleColor.Blue;
                Console.Write("Play");
                if (isStart)
                    Console.BackgroundColor = ConsoleColor.Black;
                Console.Write("\t");
                if (!isStart)
                    Console.BackgroundColor = ConsoleColor.Blue;
                Console.WriteLine("Exit");
                if (!isStart)
                    Console.BackgroundColor = ConsoleColor.Black;
                //выбор кнопок и обработчик на нажатие по кнопке
                var key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.LeftArrow:
                        isStart = true;
                        break;
                    case ConsoleKey.RightArrow:
                        isStart = false;
                        break;
                    case ConsoleKey.Enter:
                        if (isStart)
                            isSelected = true;//начало игры
                        else
                            System.Environment.Exit(1);//выход
                        break;
                }
                Console.Clear();
            } while (!isSelected);
        }

        private void GameScreen()
        {
            //счёт
            string scoreScreen = $"* Score::{Score} *";
            for (int i = 0; i < scoreScreen.Length; i++)
                Console.Write("*");
            Console.Write($"\n{scoreScreen}\n");
            for (int i = 0; i < scoreScreen.Length; i++)
                Console.Write("*");
            Console.WriteLine();
            //поле
            Console.BackgroundColor = ConsoleColor.DarkGray;
            for (int i = 0; i < 22; i++)
                Console.Write(" ");
            Console.ResetColor();
            Console.WriteLine();
            for (int i = 0; i < 20; i++)
            {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.Write(" ");
                Console.ResetColor();
                for (int j = 0; j < 10; j++)
                {
                    if (matrix[i, j] == 0)
                        Console.BackgroundColor = ConsoleColor.White;
                    else
                        Console.BackgroundColor = collorFigure[_numCollor];
                    Console.Write("  ");
                }
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(" ");
                Console.ResetColor();
            }
            Console.BackgroundColor = ConsoleColor.DarkGray;
            for (int i = 0; i < 22; i++)
                Console.Write(" ");
            Console.ResetColor();
        }

        public int[,] GetFigure(int numFigure)
        {
            switch (numFigure)
            {
                case 1:
                    return new int[3, 3] { { 2, 2, 0 },//[][]
                                           { 2, 3, 0 },//[][]
                                           { 0, 0, 0 } };
                case 2:
                    return new int[3, 3] { { 2, 2, 0 },//[][]
                                           { 0, 3, 2 },//  [][]
                                           { 0, 0, 0 } };
                case 3:
                    return new int[3, 3] { { 0, 2, 0 },//  []
                                           { 2, 3, 2 },//[][][]
                                           { 0, 0, 0 } };
                case 4:
                    return new int[3, 3] { { 0, 2, 2 },//  [][]
                                           { 2, 3, 0 },//[][]
                                           { 0, 0, 0 } };
                case 5:
                    return new int[4, 4] { { 0, 0, 0, 0 },//[][][][]
                                           { 2, 3, 2, 2 },
                                           { 0, 0, 0, 0 },
                                           { 0, 0, 0, 0 } };
                case 6:
                    return new int[3, 3] { { 2, 0, 0 },//[]
                                           { 2, 3, 2 },//[][][]
                                           { 0, 0, 0 } };
                case 7:
                    return new int[3, 3] { { 0, 0, 2 },//    []
                                           { 2, 3, 2 },//[][][]
                                           { 0, 0, 0 } };
            }
            return new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        }
    }
}
