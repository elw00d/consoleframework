using System;

namespace ConsoleFramework.Events
{
    ///<summary>
    /// An interface that allows an application author to define a method to be invoked.
    ///</summary>
    public interface ICommand
    {
        /// <summary>
        /// Raised when the ability of the command to execute has changed.
        /// </summary>
        event EventHandler CanExecuteChanged;

        /// <summary>
        /// Returns whether the command can be executed.
        /// </summary>
        /// <param name="parameter">A parameter that may be used in executing the command. This parameter may be ignored by some implementations.</param>
        /// <returns>true if the command can be executed with the given parameter and current state. false otherwise.</returns>
        bool CanExecute(object parameter);

        /// <summary>
        /// Defines the method that should be executed when the command is executed.
        /// </summary>
        /// <param name="parameter">A parameter that may be used in executing the command. This parameter may be ignored by some implementations.</param>
        void Execute(object parameter);
    }
}
