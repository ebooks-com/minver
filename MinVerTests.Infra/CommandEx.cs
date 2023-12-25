using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SimpleExec;

namespace MinVerTests.Infra;

internal static class CommandEx
{
    private static readonly ConcurrentDictionary<string, int> indices = new();

    public static async Task<(string StandardOutput, string StandardError)> ReadLoggedAsync(string name, string args = "", string workingDirectory = "", IEnumerable<KeyValuePair<string, string>>? envVars = null, Func<int, bool>? handleExitCode = null)
    {
        envVars = (envVars ?? Enumerable.Empty<KeyValuePair<string, string>>()).ToList();

        var result = await Command.ReadAsync(
            name,
            args,
            workingDirectory,
            configureEnvironment: env =>
            {
                foreach (var (key, value) in envVars)
                {
                    env[key] = value;
                }
            },
            handleExitCode: handleExitCode).ConfigureAwait(false);

        int index;

        lock (indices)
        {
            index = indices.GetOrAdd(workingDirectory, 0);
            indices[workingDirectory] = index + 1;
        }

        var markdown =
            $@"
# Command read {index}

## Command

### Name

`{name}`

### Args

`{args}`

### Working directory

`{workingDirectory}`

### Environment variables

```text
{string.Join(Environment.NewLine, envVars.Select(pair => $"{pair.Key}={pair.Value}"))}
```

## Result

### StandardOutput (stdout)

```text
{result.StandardOutput}
```

### StandardError (stderr)

```text
{result.StandardError}
```
";

        var markdownFileName = Path.Combine(workingDirectory, $"command-read-{index:D2}.md");

        await File.WriteAllTextAsync(markdownFileName, markdown).ConfigureAwait(false);

        return result;
    }
}
