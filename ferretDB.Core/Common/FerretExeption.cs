using System;
using System.Collections.Generic;
using System.Text;

namespace ferretDB.Core.Common;

/// <summary>
/// Categorizes the specific reason the database engine failed.
/// </summary>
public enum FerretErrorCode
{
    GenericError,
    SyntaxError,
    TableNotFound,
    ColumnNotFound,
    TypeMismatch,
    ConstraintViolation,
    StorageError
}

/// <summary>
/// The base exception for all custom errors thrown by the ferretDB engine.
/// </summary>
public class FerretException : Exception
{
    public FerretErrorCode ErrorCode { get; }

    // Primary constructor for standard error messages
    public FerretException(string message, FerretErrorCode errorCode = FerretErrorCode.GenericError)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    // Overload for wrapping inner exceptions (e.g., if a file read fails in the Storage layer)
    public FerretException(string message, Exception innerException, FerretErrorCode errorCode = FerretErrorCode.GenericError)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    // Override ToString to make CLI output cleaner
    public override string ToString()
    {
        return $"[{ErrorCode}] {Message}";
    }
}