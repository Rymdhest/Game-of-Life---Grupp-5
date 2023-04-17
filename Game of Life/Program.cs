﻿
using System.Runtime.ConstrainedExecution;

namespace Game_of_Life
{
    internal class Program
    {
        public static int CurrentState;
        public const int MenuState = 0;
        public const int GameState = 1;
        public const int QuitState = 2;

        /*Den här funktionen initierar en dubbel array. Dubbla arrayer är som 2D tabeller, första värden
        * är antal rader, andra värden är antal kolumner. Position i tabell kan man komma åt genom att ange rad och kolumn
        * i den dubble arrayen, t.ex table [1,3] blir platsen på första raden, tredje kolumnen.*/
        /*TODO: Vi kommer förmodligen behöva två dubbelarrayer. En "gammal" och en "ny" så att den nya arrayen tar emot de nya värden utan att ändra värden
         * på den gamla (om den ändra värden på grannceller under tiden som den loopar igenom så kommer inte spelet antagligen fungera som den ska). 
         * När den har loopat klar igenom array borde värden i nya arrayen överföras till gamla osv.*/
        public static string[,] initializeTable()
        {
            string[,] table = new string[25, 40];
            return table;
        }

        //Den här funktionen går igenom varje möjlig position i tabellen och tilldelar den strängen "□ " som sedan skrivs ut.
        /*TODO: för den automatiska körläget behöver vi nog ändra det så att strängen som tilldelas är randomiserad värde 
         * och väljer mellan "□ " och "■ ". För den manuella behöver vi en funktion där användaren uppger vilka positioner
         * som blir "■ "*/
        public static void PrintTable(string[,] table)
        {
            for (int i = 0; i < table.GetLength(0); i++)
            {
                for (int j = 0; j < table.GetLength(1); j++)
                {
                    Console.Write("□ ");
                }
                Console.WriteLine();
            }
        }


        static void Main(string[] args)
        {
            CurrentState = MenuState;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Menu menu= new Menu();
            menu.PrintMenu();
            string[,] GameField = initializeTable();
            while (CurrentState != QuitState)
            {
                if (CurrentState == MenuState)
                {
                    menu.checkInput();
                    menu.PrintMenu();
                } else if (CurrentState == GameState)
                {
                    Console.Clear();
                    PrintTable(GameField); //TODO implementera spelet på riktigt. bara fulkod här
                }
            }
        }
    }
    public class Menu
    {
        private string[] Options;
        private int SelectedOption;
        private string selectedPrefixMarker = "-> ";

        public Menu()
        {
            SelectedOption = 0;
            Options = new string[]{"Play New Game","Load Game", "Quit"};
        }
        public void PrintMenu()
        {
            Console.Clear();
            Console.CursorVisible = false;
            string emptySpace = "";
            for (int i = 0; i < selectedPrefixMarker.Length; i++)
            {
                emptySpace += " ";
            }

            for (int i = 0; i<Options.Length; i++)
            {
                if (i==SelectedOption)
                {
                    Console.WriteLine(selectedPrefixMarker + Options[i]);
                } else
                {

                    Console.WriteLine(emptySpace + Options[i]);
                }
            }
        }
        private void IncrementSelectedOption()
        {
            if (SelectedOption < Options.Length-1) {
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
        private void ApplySelectedOption()
        {
            if (SelectedOption == 0)
            {
                Program.CurrentState = Program.GameState;
            }
            if (SelectedOption == 1)
            {
                //TODO lägg till ladda nytt spel från fil
            }
            if (SelectedOption == 2)
            {
                Program.CurrentState = Program.QuitState;
            }
        }
    }
}