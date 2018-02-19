﻿//-----------------------------------------------------------------------
// <copyright file="RelayCommand.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows.Input;

namespace MyToolkit.Command
{
    /// <summary>Provides an implementation of the <see cref="ICommand" /> interface. </summary>
    public class RelayCommand : CommandBase
    {
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        /// <summary>Initializes a new instance of the <see cref="RelayCommand" /> class. </summary>
        /// <param name="execute">The action to execute. </param>
        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="RelayCommand" /> class. </summary>
        /// <param name="execute">The action to execute. </param>
        /// <param name="canExecute">The predicate to check whether the function can be executed. </param>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>Gets a value indicating whether the command can execute in its current state. </summary>
        public override bool CanExecute => _canExecute == null || _canExecute();

        /// <summary>Defines the method to be called when the command is invoked. </summary>
        protected override void Execute()
        {
            _execute();
        }
    }

    /// <summary>Provides an implementation of the <see cref="ICommand" /> interface. </summary>
    /// <typeparam name="T">The type of the command parameter. </typeparam>
    public class RelayCommand<T> : CommandBase<T>
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        /// <summary>Initializes a new instance of the <see cref="RelayCommand" /> class. </summary>
        /// <param name="execute">The action to execute. </param>
        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="RelayCommand" /> class. </summary>
        /// <param name="execute">The action to execute. </param>
        /// <param name="canExecute">The predicate to check whether the function can be executed. </param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>Gets a value indicating whether the command can execute in its current state. </summary>
        [DebuggerStepThrough]
        public override bool CanExecute(T parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>Defines the method to be called when the command is invoked. </summary>
        protected override void Execute(T parameter)
        {
            _execute(parameter);
        }
    }
}