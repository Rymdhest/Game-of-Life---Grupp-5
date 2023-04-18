using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Diagnostics;
using static Game_of_Life.Program;
using System.Timers;

namespace Game_of_Life
{
    internal class Program
    {
        public static int CurrentState;
        public const int MenuState = 0;
        public const int GameState = 1;
        public const int QuitState = 2;
        public static gameBoard gameField;
        public static Coordinate cursor = new();
        public static int cursor_x = 0, cursor_y = 0;
        public static int DEFAULT_WIDTH = 40, DEFAULT_HEIGHT = 24;

        public class Coordinate
        {
            public int x = 0, y = 0;
            public void Set(int x, int y)
            {
                this.x = x; this.y = y;
            }
            public void Reset()
            {
                x = 0; y = 0;
            }
        }
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

            public int height { get; private set; }
            public int width { get; private set; }
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
            public void ClearActiveTable()
            {
                activeTable = new bool[height, width];
            }
            public void Step()
            {
                calculateGeneration();
                activeTable = inactiveTable;
                inactiveTable = new bool[height, width];
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
                string output = "";
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        if (activeTable[i, j]) output += "■";
                        else output += " ";
                        if (i == cursor.y && j == cursor.x) output += "<";
                        else output += " ";
                    }


                    output += "\n";
                }

                output += "\n\n" +
                    "Controls: \n" +
                    "spacebar - Runs the simulation one step\n" +
                    "a        - Runs the simulation until cancelled with spacebar\n" +
                    "← → ↑ ↓  - Move marker\n" +
                    "s        - Toggle dead/alive for selected square\n" +
                    "c        - Clear the gameboard\n" +
                    "escape   - Return to menu\n";
                Console.WriteLine(output);
                Console.WindowTop = 0;
            }
            public void calculateGeneration()
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        int livecount = 0;
                        for (int x = i - 1; x <= i + 1; x++)
                        {
                            for (int y = j - 1; y <= j + 1; y++)
                            {
                                if (x < 0 || y < 0 || x >= height || y >= width || (x == i && y == j)) //denna rad skippar räkna med cellen man står på och cellerna utanför rutnätet
                                {
                                    continue;
                                }
                                if (activeTable[x, y] == true)
                                {
                                    livecount++;
                                }
                            }
                        }
                        if (activeTable[i, j] == false && livecount >= 3) //Döda celler med 3 eller fler grannar återupplivas
                        {
                            inactiveTable[i, j] = true;
                        }
                        else if (activeTable[i, j] == true && (livecount == 2 || livecount == 3)) //Levande celler med 2 eller 3 grannar lever vidare
                        {
                            inactiveTable[i, j] = true;
                        }
                        else
                        {
                            inactiveTable[i, j] = false; //Celler med none of the above fortsätter vara döda
                        }
                    }
                }
            }
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
                        gameField.PrintTable();
                    }
                    else if (KeyInfo.Key.ToString() == "Escape")
                    {
                        CurrentState = MenuState;
                        menu.PrintMenu();
                    }
                    else if (KeyInfo.Key.ToString() == "RightArrow" && cursor.x < gameField.width - 1)
                    {
                        cursor.x++;
                        gameField.PrintTable();
                    }
                    else if (KeyInfo.Key.ToString() == "LeftArrow" && cursor.x > 0)
                    {
                        cursor.x--;
                        gameField.PrintTable();
                    }
                    else if (KeyInfo.Key.ToString() == "DownArrow" && cursor.y < gameField.height - 1)
                    {
                        cursor.y++;
                        gameField.PrintTable();
                    }
                    else if (KeyInfo.Key.ToString() == "UpArrow" && cursor.y > 0)
                    {
                        cursor.y--;
                        gameField.PrintTable();
                    }
                    else if (KeyInfo.Key.ToString().ToLower() == "s")
                    {
                        gameField.SetActiveTableValue(cursor.y, cursor.x, !gameField.GetActiveTableValue(cursor.y, cursor.x));
                    }
                    else if (KeyInfo.Key.ToString().ToLower() == "c")
                    {
                        gameField.ClearActiveTable();
                    }
                    else if (KeyInfo.Key.ToString().ToLower() == "a")
                    {
                        while (!Console.KeyAvailable)
                        {
                            gameField.Step();
                            gameField.PrintTable();
                            Thread.Sleep(100);
                        }
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
                cursor.Reset();
                gameField = new gameBoard(DEFAULT_HEIGHT, DEFAULT_WIDTH);
                Console.SetWindowSize(DEFAULT_WIDTH + 50, DEFAULT_HEIGHT + 10);
                Console.WindowLeft = 0;
                Console.WindowTop = 0;
                Program.CurrentState = Program.GameState;
            }
            if (SelectedOption == 1)
            {
                // ladda nytt spel från fil
                cursor.Reset();
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