using System;

namespace InteractiveShell
{
    /// <summary>
    /// Base InteractiveShell class.
    /// 
    /// Implement / override the required methods.
    /// </summary>
    public abstract class InteractiveShellBase
    {
        /// <summary>
        /// Get the shell prefix. Will be printed before any user input.
        /// </summary>
        public abstract string ShellPrefix { get; }

        /// <summary>
        /// Is the interactive shell running?
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Current user input.
        /// </summary>
        public string Input { get; protected set; }

        /// <summary>
        /// Previous rendered line (ShellPrefix + input) 
        /// </summary>
        public string PreviousRender { get; protected set; }

        /// <summary>
        /// Current text cursor position (starts after the ShellPrefix).
        /// </summary>
        public int InputPosition { get; protected set; }

        /// <summary>
        /// Called when the user presses the 'tab' key. Return a not-null value to override the currently set input.
        /// </summary>
        protected virtual string OnAutoComplete(string input)
        {
            return null;
        }

        /// <summary>
        /// Called before the main run loop starts.
        /// </summary>
        protected virtual void OnRun()
        {
            
        }

        /// <summary>
        /// Called when the main run loop ends.
        /// </summary>
        protected virtual void OnHalt()
        {
            
        }

        /// <summary>
        /// Called when the user presses the 'enter' key to accept the input.
        /// </summary>
        protected abstract void OnEnter(string input);

        /// <summary>
        /// Change the input. The cursor position will be automaticly set to the max.
        /// </summary>
        /// <param name="input">New input.</param>
        public void ChangeInput(string input)
        {
            Input = input ?? string.Empty;
            InputPosition = Input.Length;
        }

        /// <summary>
        /// Called when a key is pressed. Return true to prevent the default behavior.
        /// </summary>
        /// <param name="key">Pressed key.</param>
        /// <returns>True to disable to default behavior, false otherwise.</returns>
        protected virtual bool OnHandleKey(ConsoleKey key)
        {
            return false;
        }

        /// <summary>
        /// Run the interactive shell. Will not return until Halt() is called.
        /// </summary>
        public bool Run()
        {
            if (IsRunning)
            {
                return false;
            }

            IsRunning = true;
            Input = string.Empty;
            OnRun();
            RenderInitial();

            while (IsRunning)
            {
                ConsoleKeyInfo key = Console.ReadKey(false);
                string input;

                if (OnHandleKey(key.Key))
                {
                    if (!IsRunning)
                    {
                        // No longer running
                        break;
                    }

                    continue;
                }

                switch (key.Key)
                {
                    // Keys that should be ignored
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.Escape:
                    case ConsoleKey.F1:
                    case ConsoleKey.F2:
                    case ConsoleKey.F3:
                    case ConsoleKey.F4:
                    case ConsoleKey.F5:
                    case ConsoleKey.F6:
                    case ConsoleKey.F7:
                    case ConsoleKey.F8:
                    case ConsoleKey.F9:
                    case ConsoleKey.F10:
                    case ConsoleKey.F11:
                    case ConsoleKey.F12:
                        break;

                    case ConsoleKey.Tab:
                        string newInput = OnAutoComplete(Input);

                        if (newInput != null)
                        {
                            ChangeInput(newInput);
                        }

                        break;
                    case ConsoleKey.LeftArrow:
                        // See if we can move to the left
                        if (InputPosition > 0)
                        {
                            InputPosition--;
                        }

                        break;
                    case ConsoleKey.RightArrow:
                        // See if we can move to the right
                        if (InputPosition + 1 < Input.Length)
                        {
                            InputPosition++;
                        }

                        break;

                    case ConsoleKey.Backspace:
                        // See if we can move to the right
                        if (InputPosition > 0)
                        {
                            InputPosition--;
                            input = Input;

                            Input = string.Empty;
                            if (InputPosition > 0)
                            {
                                Input += input.Substring(0, InputPosition);
                            }

                            Input += input.Substring(InputPosition + 1);
                        }

                        break;

                    case ConsoleKey.Enter:
                        InputPosition = 0;

                        input = Input;

                        Input = string.Empty;
                        InputPosition = 0;

                        if (Environment.OSVersion.Platform != PlatformID.Unix && Environment.OSVersion.Platform != PlatformID.MacOSX)
                        {
                            Console.CursorTop++;
                        }

                        Console.CursorLeft = 0;
                        OnEnter(input);

                        break;
                    default:
                        Input += key.KeyChar;
                        InputPosition += key.KeyChar.ToString().Length;
                        break;
                }

                if (!IsRunning)
                {
                    // No longer running
                    break;
                }

                Render();
            }

            OnHalt();
            Console.WriteLine();

            return true;
        }

        /// <summary>
        /// Render the initial shell.
        /// </summary>
        private void RenderInitial()
        {
            if (Console.CursorLeft == 0)
            {
                Console.Write(ShellPrefix);
            }
        }

        /// <summary>
        /// Render the shell.
        /// </summary>
        private void Render()
        {
            string shellPrefix = ShellPrefix;
            int oldLength = (PreviousRender ?? "").Length;
            int newLength = Input.Length + shellPrefix.Length;
            int diffLength = newLength - oldLength;

            // Store the old line
            PreviousRender = shellPrefix + Input;

            // Reset the cursor position
            Console.CursorLeft = 0;

            // Calculate how many rows to move up to get to the 'current' row
            int rows = (int)Math.Ceiling((double)(oldLength / Console.BufferWidth));
            rows = rows > 0 ? rows + 1 : 1;
            rows--;

            if (oldLength > 0 && ((oldLength + 1) % Console.BufferWidth == 0))
            {
                rows++;
            }

            if (diffLength >= 0)
            {
                // Render more or equal amount of text compared to previous

                Console.CursorTop -= rows;
                Console.Write("{0}{1}", shellPrefix, Input);
                Console.CursorLeft = ((InputPosition % Console.BufferWidth) + shellPrefix.Length) % Console.BufferWidth;
            }
            else
            {
                // Less text to render
                Console.CursorTop -= rows;
                Console.Write("{0}{1}", shellPrefix, Input);
                Console.Write("{0}", " ".PadRight(oldLength - newLength, ' '));

                Console.CursorLeft = ((InputPosition % Console.BufferWidth) + shellPrefix.Length) % Console.BufferWidth;
            }
        }

        public void WriteLine(string line, params string[] args)
        {
            line = string.Format(line, args);

            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                line = line.Replace("\t", "        ");
            }

            string previousRender = Input;
            Input = string.Empty;
            Render();

            Console.CursorLeft = 0;
            Console.WriteLine(line.PadRight(ShellPrefix.Length));

            Input = previousRender;
            PreviousRender = string.Empty;
            Render();
        }

        public void WriteLine()
        {
            WriteLine(string.Empty);
        }

        public virtual bool Halt()
        {
            if (!IsRunning)
            {
                return false;
            }

            IsRunning = false;

            return true;
        }
    }
}
