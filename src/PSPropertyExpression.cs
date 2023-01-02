using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace InteractiveSelect;

internal class PSPropertyExpressionResult
{
    public Exception? Exception { get; set; }
    public object? Result { get; set; }

    public static PSPropertyExpressionResult FromResult(object? result)
        => new PSPropertyExpressionResult(null, result);

    public static PSPropertyExpressionResult FromException(Exception? exception)
        => new PSPropertyExpressionResult(exception, null);

    private PSPropertyExpressionResult(Exception? exception, object? result)
    {
        this.Exception = exception;
        this.Result = result;
    }
}

public class PSPropertyExpression
{
    private readonly string? name;
    private readonly ScriptBlock? scriptBlock;

    public PSPropertyExpression(string name)
    {
        this.name = name;
    }

    public PSPropertyExpression(ScriptBlock scriptBlock)
    {
        this.scriptBlock = scriptBlock;
    }

    internal List<PSPropertyExpressionResult> GetValues(PSObject? target)
    {
        var results = new List<PSPropertyExpressionResult>(1);

        if (!string.IsNullOrEmpty(name))
        {
            if (target is not null)
            {
                var value = target.Properties[name].Value;
                results.Add(PSPropertyExpressionResult.FromResult(value));
            }
        }
        else if (scriptBlock is not null)
        {
            try
            {
                var scriptResults = scriptBlock.InvokeWithInputObject(target);
                results.Capacity = scriptResults.Count;
                foreach (var result in scriptResults)
                {
                    results.Add(PSPropertyExpressionResult.FromResult(result));
                }
            }
            catch (RuntimeException ex)
            {
                results.Add(PSPropertyExpressionResult.FromException(ex));
            }
        }

        return results;
    }
}
