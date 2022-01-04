using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Bonsai.Design
{
    /// <summary>
    /// Represents a command execution stack with support for undo and redo operations.
    /// </summary>
    public class CommandExecutor : Component
    {
        Command composite;
        private int currentCommand = -1;
        private readonly List<Command> history = new List<Command>();

        /// <summary>
        /// Occurs when the command execution stack has changed, either by executing
        /// a new command, or calling undo or redo operations.
        /// </summary>
        public event EventHandler StatusChanged;

        /// <summary>
        /// Gets a value indicating whether there are any commands available to undo on
        /// the command execution stack.
        /// </summary>
        public bool CanUndo
        {
            get { return currentCommand >= 0; }
        }

        /// <summary>
        /// Gets a value indicating whether there are any commands available to redo
        /// on the command execution stack.
        /// </summary>
        public bool CanRedo
        {
            get { return currentCommand < history.Count - 1; }
        }

        /// <summary>
        /// Clears the entire command execution history.
        /// </summary>
        public void Clear()
        {
            if (composite != null)
            {
                throw new InvalidOperationException($"{nameof(EndCompositeCommand)} must be called before clearing the command history.");
            }

            history.Clear();
            currentCommand = -1;
            OnStatusChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Marks the beginning of a composite command execution.
        /// </summary>
        /// <remarks>
        /// Every command executed until <see cref="EndCompositeCommand"/> will be considered
        /// part of the composite action. Calling undo or redo on a composite action will affect
        /// all commands in the composite, as if they were effectively part of a single command.
        /// </remarks>
        public void BeginCompositeCommand()
        {
            if (composite != null)
            {
                throw new InvalidOperationException($"{nameof(EndCompositeCommand)} must be called before creating a new composite command.");
            }

            composite = new Command(null, () => { });
        }

        /// <summary>
        /// Specifies a new action for immediate execution, together with the optional
        /// undo action which reverses the effects of the command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="undo">
        /// The undo action which reverses the effects of the <paramref name="command"/>.
        /// If no undo action is specified, the entire command history up to the execution
        /// of this command will be cleared.
        /// </param>
        public void Execute(Action command, Action undo)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            command();
            if (composite != null)
            {
                composite.Execute += command;
                if (composite.Undo != null && undo != null)
                {
                    var previousUndo = composite.Undo;
                    composite.Undo = () =>
                    {
                        undo();
                        previousUndo();
                    };
                }
                else composite.Undo = null;
            }
            else
            {
                composite = new Command(command, undo);
                EndCompositeCommand();
            }
        }

        /// <summary>
        /// Marks the end of a composite command execution.
        /// </summary>
        public void EndCompositeCommand()
        {
            if (composite == null)
            {
                throw new InvalidOperationException($"{nameof(BeginCompositeCommand)} must be called before this operation.");
            }

            if (composite.Execute == null)
            {
                throw new InvalidOperationException("A composite command must have at least one action defined.");
            }

            if (composite.Undo != null)
            {
                history.RemoveRange(
                    ++currentCommand,
                    history.Count - currentCommand
                );
                history.Add(composite);
                OnStatusChanged(EventArgs.Empty);
                composite = null;
            }
            else
            {
                composite = null;
                Clear();
            }
        }

        /// <summary>
        /// Undo the effects of the previously executed command.
        /// </summary>
        public void Undo()
        {
            Undo(true);
        }

        /// <summary>
        /// Undo the effects of the previously executed command, with
        /// optional support for redo operations.
        /// </summary>
        /// <param name="allowRedo">
        /// If this parameter is <see langword="true"/>, redo operations will be allowed
        /// after undoing the previous command. Otherwise, all the forward history, including
        /// the command being undone will be cleared.
        /// </param>
        public void Undo(bool allowRedo)
        {
            if (composite != null)
            {
                throw new InvalidOperationException($"{nameof(EndCompositeCommand)} must be called before any undo/redo operations.");
            }

            if (CanUndo)
            {
                history[currentCommand--].Undo();
                if (!allowRedo)
                {
                    history.RemoveRange(
                        currentCommand + 1,
                        history.Count - currentCommand - 1
                    );
                }
                OnStatusChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Redo the effects of the command which was previously undone.
        /// </summary>
        public void Redo()
        {
            if (composite != null)
            {
                throw new InvalidOperationException($"{nameof(EndCompositeCommand)} must be called before any undo/redo operations.");
            }

            if (CanRedo)
            {
                history[++currentCommand].Execute();
                OnStatusChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the <see cref="StatusChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnStatusChanged(EventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        private class Command
        {
            public Command(Action execute, Action undo)
            {
                Execute = execute;
                Undo = undo;
            }

            public Action Execute { get; set; }

            public Action Undo { get; set; }
        }
    }
}
