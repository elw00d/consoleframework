namespace ConsoleFramework.Events
{
    ///<summary>
    /// An interface for classes that know how to invoke a Command.
    ///</summary>
    public interface ICommandSource
    {
        /// <summary>
        /// The command that will be executed when the class is "invoked."
        /// Classes that implement this interface should enable or disable based on the command's CanExecute return value.
        /// </summary>
        ICommand Command {
            get;
            set;
        }

        /// <summary>
        /// The parameter that will be passed to the command when executing the command.
        /// </summary>
        object CommandParameter {
            get;
            set;
        }
    }
}
