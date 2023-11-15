using Skyline.DataMiner.Analytics.GenericInterface;
using System;

[GQIMetaData(Name = "Format date/time")]
public class FormatDateTimeOperator : IGQIRowOperator, IGQIInputArguments
{
    // Arguments
    private readonly GQIColumnDropdownArgument _columnArg;
    private readonly GQIStringArgument _formatArg;

    // Argument values
    private GQIColumn _column;
    private string _format; // A .NET standard or custom format string

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatDateTimeOperator"/> class.
    /// Initializes both the column and format input arguments.
    /// Called by GQI and should be parameterless.
    /// </summary>
    public FormatDateTimeOperator()
    {
        // Note: here we allow both DateTime and TimeSpan columns to format
        _columnArg = new GQIColumnDropdownArgument("Column")
        {
            IsRequired = true,
            Types = new[]
            {
                GQIColumnType.DateTime,
                GQIColumnType.TimeSpan,
            },
        };

        _formatArg = new GQIStringArgument("Format")
        {
            IsRequired = true,
        };
    }

    /// <summary>
    /// Called by GQI to define the input arguments.
    /// Defines the arguments for the column to format and the format itself.
    /// </summary>
    /// <returns>The defined arguments.</returns>
    public GQIArgument[] GetInputArguments()
    {
        return new GQIArgument[]
        {
            _columnArg,
            _formatArg,
        };
    }

    /// <summary>
    /// Called by GQI to expose the chosen argument values.
    /// </summary>
    /// <param name="args">Collection of chosen argument values.</param>
    /// <returns>Unused.</returns>
    public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
    {
        _column = args.GetArgumentValue(_columnArg);
        var format = args.GetArgumentValue(_formatArg);

        // See: https://learn.microsoft.com/en-us/dotnet/api/system.string.format#control-formatting
        _format = $"{{0:{format}}}";

        return default;
    }

    /// <summary>
    /// Called by GQI to handle each <paramref name="row"/> in turn.
    /// Calculates the new display value using the raw cell value and the chosen <see cref="_format"/>.
    /// </summary>
    /// <param name="row">The next row that needs to be handled.</param>
    public void HandleRow(GQIEditableRow row)
    {
        // Note: use the non-generic method to retrieve the value as object
        // That way, we don't need to know the specific type
        var value = row.GetValue(_column.Name);

        try
        {
            // Create & assign the new display value based on the raw value
            var displayValue = string.Format(_format, value);
            row.SetDisplayValue(_column, displayValue);
        }
        catch (FormatException ex)
        {
            // Rethrow as a GenIfException to give a nicer error message
            throw new GenIfException("Invalid date/time format.", ex);
        }
    }
}