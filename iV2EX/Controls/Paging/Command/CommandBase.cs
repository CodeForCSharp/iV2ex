﻿//-----------------------------------------------------------------------
// <copyright file="CommandBase.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows.Input;
using MyToolkit.Model;

namespace MyToolkit.Command
{
    /// <summary>Provides a base implementation of the <see cref="ICommand" /> interface. </summary>
    public abstract class CommandBase : ObservableObject, ICommand
    {
        /// <summary>Gets a value indicating whether the command can execute in its current state. </summary>
        public abstract bool CanExecute { get; }

        /// <summary>Occurs when changes occur that affect whether or not the command should execute. </summary>
        public event EventHandler CanExecuteChanged;

        void ICommand.Execute(object parameter)
        {
            Execute();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute;
        }

        /// <summary>Defines the method to be called when the command is invoked. </summary>
        protected abstract void Execute();

        /// <summary>
        ///     Tries to execute the command by checking the <see cref="CanExecute" /> property
        ///     and executes the command only when it can be executed.
        /// </summary>
        /// <returns>True if command has been executed; false otherwise. </returns>
        public bool TryExecute()
        {
            if (!CanExecute)
                return false;
            Execute();
            return true;
        }

        /// <summary>Triggers the CanExecuteChanged event and a property changed event on the CanExecute property. </summary>
        public virtual void RaiseCanExecuteChanged()
        {
            RaisePropertyChanged("CanExecute");

            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }

    /// <summary>Provides an implementation of the <see cref="ICommand" /> interface. </summary>
    /// <typeparam name="T">The type of the command parameter. </typeparam>
    public abstract class CommandBase<T> : ICommand
    {
        [DebuggerStepThrough]
        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute((T) parameter);
        }

        void ICommand.Execute(object parameter)
        {
            Execute((T) parameter);
        }

        /// <summary>Occurs when changes occur that affect whether or not the command should execute. </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>Gets a value indicating whether the command can execute in its current state. </summary>
        [DebuggerStepThrough]
        public abstract bool CanExecute(T parameter);

        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        protected abstract void Execute(T parameter);

        /// <summary>
        ///     Tries to execute the command by calling the <see cref="CanExecute" /> method
        ///     and executes the command only when it can be executed.
        /// </summary>
        /// <returns>True if command has been executed; false otherwise. </returns>
        public bool TryExecute(T parameter)
        {
            if (!CanExecute(parameter))
                return false;

            Execute(parameter);
            return true;
        }

        /// <summary>Triggers the CanExecuteChanged event. </summary>
        public virtual void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}