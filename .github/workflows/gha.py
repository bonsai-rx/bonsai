#!/usr/bin/env python3
# GitHub Actions Utility Functions
# https://docs.github.com/en/actions/reference/workflow-commands-for-github-actions
import io
import os
import sys

errors_were_printed = False

def fail_if_errors():
    if errors_were_printed:
        print("Exiting due to previous errors.")
        sys.exit(1)

def print_error(message):
    global errors_were_printed
    errors_were_printed = True
    print(f"::error::{message}")

def print_warning(message):
    print(f"::warning::{message}")

def print_notice(message):
    print(f"::notice::{message}")

def print_debug(message):
    print(f"::debug::{message}")

def github_file_command(command, message):
    command = f"GITHUB_{command}"
    command_file = os.getenv(command)

    if command_file is None:
        print_error(f"Missing required GitHub environment variable '{command}'")
        sys.exit(1)

    if not os.path.exists(command_file):
        print_error(f"'{command}' points to non-existent file '{command_file}')")
        sys.exit(1)
    
    with open(command_file, 'a') as command_file_handle:
        command_file_handle.write(message)
        command_file_handle.write('\n')

def set_output(name, value):
    if isinstance(value, bool):
        value = "true" if value else "false"
    github_file_command("OUTPUT", f"{name}<<GHA_PY_EOF\n{value}\nGHA_PY_EOF")

def set_environment_variable(name, value):
    github_file_command("ENV", f"{name}={value}")

def add_path(path):
    github_file_command("PATH", path)

class JobSummary:
    def __init__(self):
        self.file: io.TextIOBase | None = None

        summary_file_path_var = "GITHUB_STEP_SUMMARY"
        summary_file_path = os.getenv(summary_file_path_var)
        if summary_file_path is None:
            print_warning(f"Failed to open step summary file, {summary_file_path_var} is not set.")
            return

        try:
            self.file = open(summary_file_path, 'a')
        except Exception as ex:
            print_warning(f"Failed to open step summary file '{summary_file_path}': {ex}")

    def __enter__(self):
        return self
    
    def write_line(self, line: str = '') -> None:
        if self.file is None:
            return
        
        self.file.write(line)
        self.file.write('\n')
    
    def __exit__(self, exc_type, exc_val, exc_tb) -> None:
        if self.file is not None:
            self.file.__exit__(exc_type, exc_val, exc_tb)

if __name__ == "__main__":
    args = sys.argv

    def pop_arg():
        global args
        if len(args) == 0:
            print_error("Bad command line, not enough arguments specified.")
            sys.exit(1)
        result = args[0]
        args = args[1:]
        return result
    
    def done_parsing():
        if len(args) > 0:
            print_error("Bad command line, too many arguments specified.")
            sys.exit(1)
    
    pop_arg() # Skip script name
    command = pop_arg()
    if command == "print_error":
        message = pop_arg()
        done_parsing()
        print_error(message)
    elif command == "print_warning":
        message = pop_arg()
        done_parsing()
        print_warning(message)
    elif command == "print_notice":
        message = pop_arg()
        done_parsing()
        print_notice(message)
    elif command == "set_output":
        name = pop_arg()
        value = pop_arg()
        done_parsing()
        set_output(name, value)
    elif command == "set_environment_variable":
        name = pop_arg()
        value = pop_arg()
        done_parsing()
        set_environment_variable(name, value)
    elif command == "add_path":
        path = pop_arg()
        done_parsing()
        add_path(path)
    else:
        print_error(f"Unknown command '{command}'")
        sys.exit(1)
    
    fail_if_errors()
