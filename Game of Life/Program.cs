using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Diagnostics;
using static Game_of_Life.Program;
using System.Timers;
using ANSIConsole;

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
                        loadedGame.SetActiveTableValue(i, j, true);
                    else
                        loadedGame.SetActiveTableValue(i, j, false);
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
                if (randomize) this.Randomize();
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
            //Används inte i nuläget:
            public void Randomize()
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
                        if (activeTable[i, j]) output += "■".Color(ConsoleColor.Yellow);
                        else output += "□".Color(ConsoleColor.DarkGreen);
                        if (i == cursor.y && j == cursor.x) output += "<".Color(ConsoleColor.Red);
                        else output += " ";
                    }
                    output += "\n";
                }

                output += "\n" +
                    "Controls: \n" +
                    "← → ↑ ↓  - Move marker                      A - Runs the simulation until cancelled with spacebar\n" +
                    "SPACEBAR - Runs the simulation one step     R - Randomize the gameboard\n" +
                    "C        - Clear the gameboard              S - Toggle dead/alive for selected square \n" +
                    "ESCAPE   - Return to menu\n";
                Console.WriteLine(output);
                Console.WindowTop = 0;
            }
            public void calculateGeneration()
            {
                for (int x = 0; x < height; x++)
                    for (int y = 0; y < width; y++)
                    {
                        int aliveNeighbours = GetNumberOfAliveNeighbours(x, y);
                        if (activeTable[x, y] == false && aliveNeighbours == 3) //Döda celler med 3 grannar återupplivas
                            inactiveTable[x, y] = true;
                        else if (activeTable[x, y] == true && (aliveNeighbours == 2 || aliveNeighbours == 3)) //Levande celler med 2 eller 3 grannar lever vidare
                            inactiveTable[x, y] = true;
                        else
                            inactiveTable[x, y] = false; //Celler med none of the above fortsätter vara döda
                    }
            }
            private int GetNumberOfAliveNeighbours(int x, int y)
            {
                int livecount = 0;
                for (int xOffset = x - 1; xOffset <= x + 1; xOffset++)
                {
                    for (int yOffset = y - 1; yOffset <= y + 1; yOffset++)
                    {
                        if (xOffset < 0 || yOffset < 0 || xOffset >= height || yOffset >= width || (xOffset == x && y == yOffset)) //denna rad skippar räkna med cellen man står på och cellerna utanför rutnätet
                            continue;
                        if (activeTable[xOffset, yOffset] == true)
                            livecount++;
                    }
                }
                return livecount;
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
                    gameField = new gameBoard(22, 40);
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
                    }
                    else if (KeyInfo.Key.ToString() == "Escape")//FIXME: När man kör load game och sen escape så ser menyn lite konstigt ut
                    {
                        CurrentState = MenuState;
                        menu.PrintMenu();
                    }
                    else if (KeyInfo.Key.ToString() == "RightArrow" && cursor.x < gameField.width - 1)
                    {
                        cursor.x++;
                    }
                    else if (KeyInfo.Key.ToString() == "LeftArrow" && cursor.x > 0)
                    {
                        cursor.x--;
                    }
                    else if (KeyInfo.Key.ToString() == "DownArrow" && cursor.y < gameField.height - 1)
                    {
                        cursor.y++;
                    }
                    else if (KeyInfo.Key.ToString() == "UpArrow" && cursor.y > 0)
                    {
                        cursor.y--;
                    }
                    else if (KeyInfo.Key.ToString().ToLower() == "s")
                    {
                        gameField.SetActiveTableValue(cursor.y, cursor.x, !gameField.GetActiveTableValue(cursor.y, cursor.x));
                    }
                    else if (KeyInfo.Key.ToString().ToLower() == "c")
                    {
                        gameField.ClearActiveTable();
                    }
                    else if (KeyInfo.Key.ToString().ToLower() == "r")
                    {
                        gameField.Randomize();
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
}