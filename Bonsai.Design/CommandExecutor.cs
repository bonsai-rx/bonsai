using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bonsai.Design
{
    public class CommandExecutor : Component
    {
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
            history.Clear();
            currentCommand = -1;
            OnStatusChanged(EventArgs.Empty);
        }

        public void Execute(Action command, Action undo)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            command();
            if (undo != null)
            {
                history.RemoveRange(
                  ++currentCommand,
                  history.Count - currentCommand
                );
                history.Add(new Command(command, undo));
                OnStatusChanged(EventArgs.Empty);
            }
            else Clear();
        }

        public void Undo()
        {
            if (CanUndo)
            {
                history[currentCommand--].Undo();
                OnStatusChanged(EventArgs.Empty);
            }
        }

        public void Redo()
        {
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
            private readonly Action execute;
            private readonly Action undo;

            public Command(Action execute, Action undo)
            {
                this.execute = execute;
                this.undo = undo;
            }

            public Action Execute
            {
                get { return this.execute; }
            }

            public Action Undo
            {
                get { return this.undo; }
            }
        }
    }
}
