using System;
using System.Collections.Generic;
using System.Linq;
using InteractiveShell.Utils;

namespace InteractiveShell.Example
{
    public class InteractiveShellExample : InteractiveShellBase
    {
        private readonly List<string> _autocompleteList = new List<string>();
 
        public InteractiveShellExample()
        {
            _autocompleteList.Add("apple");
            _autocompleteList.Add("pear");
            _autocompleteList.Add("pineapple");
            _autocompleteList.Add("grapefruit");
            _autocompleteList.Add("chokecherry");
            _autocompleteList.Add("orange");
            _autocompleteList.Add("coconut");
            _autocompleteList.Add("lemon");
        }
        public override string ShellPrefix
        {
            get { return "EXAMPLE> "; }
        }

        protected override void OnEnter(string input)
        {
            if (input == "quit")
            {
                Halt();
                return;
            }

            if (_autocompleteList.Contains(input))
            {
                WriteLine("You entered one of the available fruits!");
            }
            else
            {
                WriteLine("{0} was not found in our list of fruits.", input);
            }
        }

        protected override void OnRun()
        {
            WriteLine("Welcome to the Interactive Shell Example");
            WriteLine();
            WriteLine("Type 'quit' to close the shell.");
            WriteLine("Try pressing 'tab' to autocomplete a fruit");
        }

        protected override void OnHalt()
        {
            WriteLine("Goodbye!");
        }

        protected override bool OnHandleKey(ConsoleKey key)
        {
            if (key == ConsoleKey.UpArrow)
            {
                WriteLine("Pressed up!");

                return true;
            }

            return false;
        }

        protected override string OnAutoComplete(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            return _autocompleteList.FirstOrDefault(c => Glob.Match(c, input + "*"));
        }
    }
}
