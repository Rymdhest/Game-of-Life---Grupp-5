using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Diagnostics;
using static Game_of_Life.Program;

namespace Game_of_Life
{
    internal class Program
    {
        public static int CurrentState;
        public const int MenuState = 0;
        public const int GameState = 1;
        public const int QuitState = 2;
        public static gameBoard gameField;

        public static gameBoard LoadGameFromFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            int height = lines.Length;
            int width = lines[0].Length;


            gameBoard loadedGame = new gameBoard(height, width, true);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (lines[i][j] == '1')
                    {
                        loadedGame.SetActiveTableValue(i, j, true);
                    }
                    else
                    {
                        loadedGame.SetActiveTableValue(i, j, false);
                    }

                }
            }
            return loadedGame;
        }

        public class gameBoard
        {
            public void SetActiveTableValue(int i, int j, bool value)
            {
                activeTable[i, j] = value;
            }

            public bool GetActiveTableValue(int i, int j)
            {
                return activeTable[i, j];
            }

            private int height, width;
            private bool[,] activeTable;
            private bool[,] inactiveTable;

            public gameBoard(int height, int width, bool randomize = false) 
            {
                //FIXME storleken på spelplanen får inte vara större än konsollrutan. då fungerar inte Console.clear() som vi vill.
                this.height = height;
                this.width = width;
                activeTable = new bool[height, width];
                inactiveTable = new bool[height, width];
                if (randomize) this.Randomize(height, width);
            }
            public void Step()
            {
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                        inactiveTable[i, j] = SquareStep(i, j);
                activeTable = inactiveTable;
                inactiveTable = new bool[height, width];
            }
            private bool SquareStep(int i, int j)
            {
                //NYI: Check status for activeBoard[i,j] for dead or alive. Check status of neighbours. Return dead (false) or alive (true) according to ruleset.
                return !activeTable[i, j]; //DEBUG: Placeholder method to make sure each square changes status each step to test Step() functionality.
            }
            public void Randomize(int height, int width)
            {
                Random rand = new Random();
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                        activeTable[i, j] = rand.NextDouble() > 0.5;
                }
            }
            public void PrintTable() 
            {
                Console.Clear();
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        if (activeTable[i, j]) Console.Write("■ ");
                        else Console.Write("□ ");
                    }
                    

                    Console.WriteLine();
                }
                
                Console.WriteLine("\n\n" +
                    "Controls: \n" +
                    "Spacebar - Runs the simulation one step\n" +
                    "Escape - Terminates the application");
                
            }
        }
        public static void calculateGeneration(string[,] table)
        {
            int rows = table.GetLength(0);
            int columns = table.GetLength(1);
            string[,] newGen = new string[rows, columns];//Skapa array för att tilldela de nya värden
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    int livecount = 0;
                    int deadcount = 0;
                    for (int x = i - 1; x <= i + 1; x++)
                    {
                        for (int y = j - 1; y <= j + 1; y++)
                        {

                            if (x < 0 || y < 0 || x >= rows || y >= columns || (x == i && y == j)) //denna rad skippar räkna med cellen man står på och cellerna utanför rutnätet
                            {
                                continue;
                            }
                            if (table[x, y] == "■")
                            {
                                livecount++;
                            }
                            else if (table[x, y] == "□")
                            {
                                deadcount++;
                            }
                        }
                    }
                    if (table[i, j] == "□" && livecount >= 3) //Döda celler med 3 eller fler grannar återupplivas
                    {
                        newGen[i, j] = "■";
                    }
                    else if (table[i, j] == "■" && (livecount == 2 || livecount == 3)) //Levande celler med 2 eller 3 grannar lever vidare
                    {
                        newGen[i, j] = "■";
                    }
                    else
                    {
                        newGen[i, j] = "□"; //Celler med none of the above fortsätter vara döda
                    }
                }
            }
            Array.Copy(newGen, table, newGen.Length);//Copierar nya arrayen till gamla när allt är klarräknad
        }


        static void Main(string[] args)
        {
            CurrentState = MenuState;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Menu menu = new Menu();
            menu.PrintMenu();
            while (CurrentState != QuitState)
            {
                if (CurrentState == MenuState)
                {
                    menu.checkInput();
                    menu.PrintMenu();
                }
                else if (CurrentState == GameState)
                {

                    gameField.PrintTable();
                    ConsoleKeyInfo KeyInfo = Console.ReadKey();
                    if (KeyInfo.Key.ToString() == "Spacebar")
                    {
                        gameField.Step();
                        gameField.PrintTable(); //TODO implementera spelet på riktigt. bara fulkod här
                    }
                    
                    else if (KeyInfo.Key.ToString() == "Escape")
                    {
                        CurrentState = MenuState;
                        menu.PrintMenu();
                    }

                }
            }
        }
    }
    public class Menu
    {
        private string[] Options;
        private int SelectedOption;
        private readonly string selectedPrefixMarker = "-> ";

        public Menu()
        {
            SelectedOption = 0;
            Options = new string[] { "New Game", "Load Game", "Quit" };
        }
        public void PrintMenu()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Clear();
            Console.CursorVisible = false;
            string emptySpace = "";
            for (int i = 0; i < selectedPrefixMarker.Length; i++)
            {
                emptySpace += " ";
            }

            for (int i = 0; i < Options.Length; i++)
            {
                if (i == SelectedOption)
                {
                    Console.WriteLine(selectedPrefixMarker + Options[i]);
                }
                else
                {

                    Console.WriteLine(emptySpace + Options[i]);
                }
            }
            Console.ResetColor();
        }
        private void IncrementSelectedOption()
        {
            if (SelectedOption < Options.Length - 1)
            {
                SelectedOption++;
            }
        }
        private void DecrementSelectedOption()
        {
            if (SelectedOption > 0)
            {
                SelectedOption--;
            }
        }
        public void checkInput()
        {
            ConsoleKeyInfo KeyInfo = Console.ReadKey();
            if (KeyInfo.Key.ToString() == "UpArrow") DecrementSelectedOption();
            if (KeyInfo.Key.ToString() == "DownArrow") IncrementSelectedOption();
            if (KeyInfo.Key.ToString() == "Enter") ApplySelectedOption();

        }
        public void ApplySelectedOption()
        {
            if (SelectedOption == 0)
            {
                Program.CurrentState = Program.GameState;
                gameField = new gameBoard(24, 40);
            }
            if (SelectedOption == 1)
            {
                // ladda nytt spel från fil
                string filePath = Path.GetFullPath("..\\..\\..\\gameoflife2.txt");
                Program.gameField = Program.LoadGameFromFile(filePath);
                Program.CurrentState = Program.GameState;
            }
            if (SelectedOption == 2)
            {
                Program.CurrentState = Program.QuitState;
            }
        }
    }
}