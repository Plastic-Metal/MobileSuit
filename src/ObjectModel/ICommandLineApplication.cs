﻿namespace PlasticMetal.MobileSuit.ObjectModel
{
    /// <summary>
    ///     A CommandLineApplication which accept string[] args as startup args
    /// </summary>
    public interface ICommandLineApplication
    {
        /// <summary>
        ///     Default startup function, which will be runned if no other method fits the given args
        /// </summary>
        /// <param name="args">The commandline from shell/other program</param>
        public int SuitStartUp(string[]? args);
    }

    /// <summary>
    ///     A CommandLineApplication which accept string[] args as startup args
    /// </summary>
    public interface ICommandLineApplication<in TArgument> : ICommandLineApplication
        where TArgument : IDynamicParameter
    {
        /// <summary>
        ///     Default startup function, which will be run if no other method fits the given args
        /// </summary>
        /// <param name="arg">The commandline parsed.</param>
        public int SuitStartUp(TArgument arg);
    }
}