namespace Splat
{
    using System;
    using System.ComponentModel;

    internal interface IFullLogger : ILogger
    {
        void Debug([Localizable(false)] string message);
        void Debug<T>(T value);
        void Debug<T>(IFormatProvider formatProvider, T value);
        void Debug([Localizable(false)] string message, params object[] args);
        void Debug<TArgument>([Localizable(false)] string message, TArgument argument);
        void Debug(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args);
        void Debug<TArgument>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument argument);
        void Debug<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2);
        void Debug<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2);
        void Debug<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3);
        void Debug<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3);
        void DebugException([Localizable(false)] string message, Exception exception);
        void Error([Localizable(false)] string message);
        void Error<T>(T value);
        void Error<T>(IFormatProvider formatProvider, T value);
        void Error([Localizable(false)] string message, params object[] args);
        void Error<TArgument>([Localizable(false)] string message, TArgument argument);
        void Error(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args);
        void Error<TArgument>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument argument);
        void Error<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2);
        void Error<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2);
        void Error<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3);
        void Error<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3);
        void ErrorException([Localizable(false)] string message, Exception exception);
        void Fatal([Localizable(false)] string message);
        void Fatal<T>(T value);
        void Fatal<T>(IFormatProvider formatProvider, T value);
        void Fatal([Localizable(false)] string message, params object[] args);
        void Fatal<TArgument>([Localizable(false)] string message, TArgument argument);
        void Fatal(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args);
        void Fatal<TArgument>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument argument);
        void Fatal<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2);
        void Fatal<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2);
        void Fatal<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3);
        void Fatal<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3);
        void FatalException([Localizable(false)] string message, Exception exception);
        void Info([Localizable(false)] string message);
        void Info<T>(T value);
        void Info<T>(IFormatProvider formatProvider, T value);
        void Info([Localizable(false)] string message, params object[] args);
        void Info<TArgument>([Localizable(false)] string message, TArgument argument);
        void Info(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args);
        void Info<TArgument>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument argument);
        void Info<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2);
        void Info<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2);
        void Info<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3);
        void Info<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3);
        void InfoException([Localizable(false)] string message, Exception exception);
        void Warn([Localizable(false)] string message);
        void Warn<T>(T value);
        void Warn<T>(IFormatProvider formatProvider, T value);
        void Warn([Localizable(false)] string message, params object[] args);
        void Warn<TArgument>([Localizable(false)] string message, TArgument argument);
        void Warn(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args);
        void Warn<TArgument>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument argument);
        void Warn<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2);
        void Warn<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2);
        void Warn<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3);
        void Warn<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3);
        void WarnException([Localizable(false)] string message, Exception exception);
    }
}

