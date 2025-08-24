﻿namespace Task.Manager.System.Tests.Process;

public partial class ProcessUtilsTests
{
#if __APPLE__
    private void CreateScriptFile(string fileName)
    {
        string script = @"#!/bin/bash\nwhile true; do\n\sleep 1\ndone\n";
        File.WriteAllText(fileName, script);
        
        global::System.IO.File.SetUnixFileMode(fileName, UnixFileMode.UserExecute);
    }
#endif
}
 