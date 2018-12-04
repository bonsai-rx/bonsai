using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bonsai.Design
{
    public class CommandExecutor : Component
    {
        Command composite;
        private int currentCommand = -1;
        private readonly List<Command> history = new List<Command>();

        public event EventHandler StatusChanged;

        public bool CanUndo
        {
            get { return currentCommand >= 0; }
        }

        public bool CanRedo
        {
            get { return currentCommand < history.Count - 1; }
        }

        public void Clear()
        {
            if (composite != null)
            {
                throw new InvalidOperationException("EndComposite must be called before clearing the command history.");
            }

            history.Clear();
            currentCommand = -1;
            OnStatusChanged(EventArgs.Empty);
        }

        public void BeginCompositeCommand()
        {
            if (composite != null)
            {
                throw new InvalidOperationException("EndComposite must be called before creating a new composite command.");
            }

            composite = new Command(null, () => { });
        }

        public void Execute(Action command, Action undo)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
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

        public void EndCompositeCommand()
        {
            if (composite == null)
            {
                throw new InvalidOperationException("BeginComposite must be called before this operation.");
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

        public void Undo()
        {
            Undo(true);
        }

        public void Undo(bool allowRedo)
        {
            if (composite != null)
            {
                throw new InvalidOperationException("EndComposite must be called before any undo/redo operations.");
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

        public void Redo()
        {
            if (composite != null)
            {
                throw new InvalidOperationException("EndComposite must be called before any undo/redo operations.");
            }

            if (CanRedo)
            {
                history[++currentCommand].Execute();
                OnStatusChanged(EventArgs.Empty);
            }
        }

        protected virtual void OnStatusChanged(EventArgs e)
        {
            var handler = StatusChanged;
            if (handler != null)
            {
                handler(this, e);
            }
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
