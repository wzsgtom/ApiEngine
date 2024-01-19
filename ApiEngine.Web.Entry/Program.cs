using ApiEngine.Core;

Serve.Run(Startup.InjectCore(RunOptions.Default).WithArgs(args));