﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using KY.Core;
using KY.Core.DataAccess;
using KY.Core.Dependency;
using KY.Core.Extension;
using KY.Core.Module;
using KY.Generator.Command;
using KY.Generator.Command.Extensions;
using KY.Generator.Configuration;
using KY.Generator.Configurations;
using KY.Generator.Languages;
using KY.Generator.Mappings;
using KY.Generator.Models;
using KY.Generator.Module;
using KY.Generator.Output;
using KY.Generator.Syntax;
using KY.Generator.Transfer.Readers;
using KY.Generator.Transfer.Writers;

namespace KY.Generator
{
    public class Generator : IGeneratorRunSyntax
    {
        private IOutput output;
        private readonly DependencyResolver resolver;
        private CommandConfiguration command;
        private readonly GeneratorEnvironment environment;
        private IList<ModuleBase> Modules { get; }

        public Generator()
        {
            Logger.CatchAll();
            Logger.Trace($"KY-Generator v{Assembly.GetCallingAssembly().GetName().Version}");
            Logger.Trace("Current Directory: " + Environment.CurrentDirectory);
            Logger.Trace("Log Directory: " + Logger.File.Path);

            NugetPackageDependencyLoader.Activate();

            this.output = new FileOutput(AppDomain.CurrentDomain.BaseDirectory);
            this.resolver = new DependencyResolver();
            this.resolver.Bind<ITypeMapping>().ToSingleton<TypeMapping>();
            this.resolver.Bind<CommandRunner>().ToSelf();
            this.resolver.Bind<ModuleFinder>().ToSelf();
            this.resolver.Bind<IConfigurationReaderVersion>().To<ConfigurationReaderVersion2>();
            this.resolver.Bind<ConfigurationMapping>().ToSingleton();
            this.resolver.Bind<ConfigurationRunner>().ToSelf();
            this.resolver.Bind<ModelWriter>().ToSelf();
            this.resolver.Bind<GeneratorEnvironment>().ToSingleton();
            this.environment = this.resolver.Get<GeneratorEnvironment>();
            StaticResolver.Resolver = this.resolver;

            ModuleFinder moduleFinder = this.resolver.Get<ModuleFinder>();
            this.InitializeModules(moduleFinder.Modules);
            this.Modules = moduleFinder.Modules;
        }

        public static Generator Initialize()
        {
            return new Generator();
        }

        public Generator PreloadModules(string path, string moduleFileNameSearchPattern = default)
        {
            ModuleFinder moduleFinder = this.resolver.Get<ModuleFinder>();
            List<ModuleBase> loadedModules = moduleFinder.LoadFrom(path, moduleFileNameSearchPattern);
            this.InitializeModules(loadedModules);
            return this;
        }

        public Generator PreloadModule<T>() where T : ModuleBase
        {
            return this;
        }

        public Generator SetOutput(IOutput newOutput)
        {
            this.output = newOutput;
            return this;
        }

        public Generator SetOutput(string path)
        {
            return this.SetOutput(new FileOutput(path));
        }

        public Generator RegisterCommand<T>() where T : IGeneratorCommand
        {
            this.resolver.Bind<IGeneratorCommand>().To<T>();
            return this;
        }

        public Generator RegisterCommand(IGeneratorCommand generator)
        {
            this.resolver.Bind<IGeneratorCommand>().To(generator);
            return this;
        }

        public Generator RegisterReader<TConfiguration, TReader>(string name)
            where TConfiguration : ConfigurationBase
            where TReader : ITransferReader
        {
            this.resolver.Get<ConfigurationMapping>().Map<TConfiguration, TReader>(name);
            return this;
        }

        public Generator RegisterWriter<TConfiguration, TWriter>(string name)
            where TConfiguration : ConfigurationBase
            where TWriter : ITransferWriter
        {
            this.resolver.Get<ConfigurationMapping>().Map<TConfiguration, TWriter>(name);
            return this;
        }

        public IGeneratorRunSyntax ReadConfiguration(string path)
        {
            Logger.Trace($"Read configuration from {path}");
            this.Modules.OfType<GeneratorModule>().ForEach(x => x.BeforeConfigure());
            this.command = new CommandConfiguration("run");
            this.command.Parameters.Add(new CommandValueParameter("path", path));
            return this;
        }

        public IGeneratorRunSyntax ParseConfiguration(string configuration)
        {
            Logger.Trace($"Parse configuration {configuration}");
            this.Modules.OfType<GeneratorModule>().ForEach(x => x.BeforeConfigure());
            this.command = new CommandConfiguration("run");
            this.command.Parameters.Add(new CommandValueParameter("configuration", configuration));
            return this;
        }

        public IGeneratorRunSyntax ParseCommand(params string[] parameters)
        {
            return this.ParseCommand(CommandParameterParser.Parse(parameters).ToList());
        }

        public IGeneratorRunSyntax ParseCommand(List<ICommandParameter> parameters)
        {
            CommandParameter commandParameter = parameters.OfType<CommandParameter>().First();
            Logger.Trace($"Parse command {commandParameter.Name}");
            this.Modules.OfType<GeneratorModule>().ForEach(x => x.BeforeConfigure());
            this.command = new CommandConfiguration(commandParameter.Name);
            this.command.Parameters.AddRange(parameters.Where(x => x != commandParameter));
            CommandValueParameter outputParameter = this.command.Parameters.OfType<CommandValueParameter>().FirstOrDefault(x => x.Name.Equals("output", StringComparison.CurrentCultureIgnoreCase));
            if (outputParameter != null)
            {
                this.SetOutput(outputParameter.Value);
            }
            this.InitializeEnvironment(parameters);
            return this;
        }

        public IGeneratorRunSyntax ParseAttributes(string assemblyName)
        {
            Logger.Trace($"Read attributes from assembly {assemblyName}");
            this.Modules.OfType<GeneratorModule>().ForEach(x => x.BeforeConfigure());
            this.command = new CommandConfiguration("run-by-attributes");
            this.command.Parameters.Add(new CommandValueParameter("assembly", assemblyName));
            return this;
        }

        public IGeneratorRunSyntax SetParameters(params string[] parameters)
        {
            if (parameters.Length == 0)
            {
                Logger.Error("No parameters found. Provide at least a command or a path to a configuration file. Generation aborted!");
                return this;
            }
            List<ICommandParameter> commandParameters = CommandParameterParser.Parse(parameters).ToList();
            CommandParameter commandParameter = commandParameters.OfType<CommandParameter>().FirstOrDefault();
            if (commandParameter == null)
            {
                Logger.Error("No command found. Provide at least a command or a path to a configuration file. Generation aborted!");
                return this;
            }
            if (commandParameters.Count >= 2 && commandParameters[0].Name.Contains(":\\") && commandParameters[1].Name.Contains(":\\"))
            {
                Logger.Warning("Legacy output parameter found. Please use -output=\"...\" instead. Generator will fix this for you ;-)");
                commandParameters[1] = new CommandValueParameter("output", commandParameters[1].Name);
            }
            this.InitializeEnvironment(commandParameters);
            if (FileSystem.FileExists(commandParameter.Name))
            {
                if (commandParameter.Name.EndsWith(".json", StringComparison.CurrentCultureIgnoreCase))
                {
                    this.ReadConfiguration(commandParameter.Name);
                }
                else
                {
                    this.ParseAttributes(commandParameter.Name);
                }
                this.SetOutput(FileSystem.Parent(commandParameter.Name));
                this.command.Parameters.AddRange(commandParameters.Where(x => x != commandParameter));
                return this;
            }
            CommandValueParameter fallbackParameter = commandParameters.OfType<CommandValueParameter>().FirstOrDefault(x => x.Name.Equals("assembly", StringComparison.CurrentCultureIgnoreCase));
            if (!this.environment.IsBeforeBuild && fallbackParameter != null && FileSystem.FileExists(fallbackParameter.Value))
            {
                this.ParseAttributes(fallbackParameter.Value);
                this.SetOutput(FileSystem.Parent(commandParameter.Name));
                this.command.Parameters.AddRange(commandParameters.Where(x => x != commandParameter && x != fallbackParameter));
                this.command.Environment.Parameters = commandParameters;
                return this;
            }
            if (commandParameter.Name.Contains(":\\"))
            {
                Action<string> log = this.environment.IsBeforeBuild ? (Action<string>)Logger.Trace : message => Logger.Error(message);
                log($"'{commandParameter.Name}' not found");
                log("Create a generator.json in your project root or use [Generate] attributes");
                log("See our Documentation: https://generator.ky-programming.de/");
                return this;
            }
            return this.ParseCommand(parameters);
        }

        public bool Run()
        {
            bool result;
            try
            {
                if (this.command == null)
                {
                    // If we are in before build action, we do not return a error, else the build will always fail before the build is started
                    return this.environment.IsBeforeBuild;
                }
                List<ILanguage> languages = this.resolver.Get<List<ILanguage>>();
                this.command.ReadFromParameters(this.command.Parameters, languages);
                result = this.resolver.Get<CommandRunner>().Run(this.command, this.output);
                if (this.environment.SwitchContext)
                {
                    if (this.environment.SwitchToArchitecture != null && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        // TODO: Check other possibilities to run x86 in not Windows environments
                        Logger.Error($"Can not start {this.environment.SwitchToArchitecture} process. Your system does not support this process type.");
                        result = false;
                    }
                    else
                    {
                        if (this.environment.SwitchToArchitecture != null)
                        {
                            Logger.Trace($"Different assembly architecture found. Switching to {this.environment.SwitchToArchitecture}...");
                        }
                        if (this.environment.SwitchToFramework != null)
                        {
                            Logger.Trace($"Different assembly framework found. Switching to {this.environment.SwitchToFramework}...");
                        }
                        Logger.Trace("===============================");
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        string location = Assembly.GetEntryAssembly()?.Location ?? throw new InvalidOperationException("No location found");
                        Regex regex = new Regex(@"(?<separator>[\\/])(?<framework>net[^\\/]+)[\\/]");
                        Match match = regex.Match(location);
                        if (!match.Success)
                        {
                            throw new InvalidOperationException($"Invalid location {location}. Location has to include the framework to switch the context");
                        }
                        string framework = match.Groups["framework"].Value;
                        string separator = match.Groups["separator"].Value;
                        string switchedFramework = (this.environment.SwitchToFramework != null && this.environment.SwitchToFramework.IsFramework() && this.environment.SwitchToFramework.Version.Major <= 4 ? "net461" : framework)
                                                   + (this.environment.SwitchToArchitecture != null ? $"-{this.environment.SwitchToArchitecture.ToString().ToLower()}" : "");
                        location = location.Replace(separator + framework + separator, separator + switchedFramework + separator).Replace(".dll", ".exe");
                        if (FileSystem.FileExists(location))
                        {
                            startInfo.FileName = location;
                            startInfo.Arguments += string.Join(" ", this.command.Environment.Parameters);
                            if (this.environment.SwitchToArchitecture != null)
                            {
                                startInfo.Arguments += $" -switchedFromArchitecture=\"{this.environment.SwitchToArchitecture}\"";
                            }
                            if (this.environment.SwitchToFramework != null)
                            {
                                startInfo.Arguments += $" -switchedFromFramework=\"{this.environment.SwitchToFramework}\"";
                            }
                            //startInfo.UseShellExecute = false;
                            //startInfo.RedirectStandardOutput = true;
                            //startInfo.RedirectStandardError = true;
                            Process process = Process.Start(startInfo);
                            process.OutputDataReceived += (sender, args) => Logger.Trace(">> " + args.Data);
                            process.ErrorDataReceived += (sender, args) => Logger.Error(">> " + args.Data);
                            process.WaitForExit();
                            Logger.Trace($"{this.environment.SwitchToArchitecture?.ToString() ?? this.environment.SwitchToFramework?.Identifier} process exited with code {process.ExitCode}");
                            result = process.ExitCode == 0;
                        }
                        else
                        {
                            Logger.Error($"Can not start {this.environment.SwitchToArchitecture} process. File \"{location}\" not found. Try to update to .net Core Framework 3.0 or later.");
                            result = false;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                result = false;
            }
            finally
            {
                Logger.Trace("===============================");
            }
            if (!result && Logger.ErrorTargets.Contains(Logger.MsBuildOutput))
            {
                Logger.Error($"See the full log in: {Logger.File.Path}");
            }
            return result;
        }

        public static void InitializeLogger(string[] parameters)
        {
            Logger.AllTargets.Add(Logger.VisualStudioOutput);
            if (parameters.Any(parameter => parameter.ToLowerInvariant().Contains("forwardlogging")))
            {
                ForwardConsoleTarget target = new ForwardConsoleTarget();
                Logger.AllTargets.Clear();
                Logger.AllTargets.Add(target);
                Logger.TraceTargets.Clear();
                Logger.TraceTargets.Add(target);
                Logger.ErrorTargets.Clear();
                Logger.ErrorTargets.Add(target);
            }
            if (parameters.Any(parameter => "msbuild".Equals(parameter)))
            {
                Logger.Trace("MsBuild trace mode activated");
                Logger.WarningTargets.Add(Logger.MsBuildOutput);
                Logger.ErrorTargets.Add(Logger.MsBuildOutput);
                Logger.WarningTargets.Remove(Logger.VisualStudioOutput);
                Logger.ErrorTargets.Remove(Logger.VisualStudioOutput);
            }
        }

        private void InitializeModules(IEnumerable<ModuleBase> modules)
        {
            List<ModuleBase> list = modules.ToList();
            list.ForEach(module => this.resolver.Bind<ModuleBase>().To(module));
            list.ForEach(module => module.Initialize());
            list.ForEach(module => Logger.Trace($"{module.GetType().Name.Replace("Module", "")}-{module.GetType().Assembly.GetName().Version} module loaded"));
        }

        private void InitializeEnvironment(List<ICommandParameter> parameters)
        {
            this.environment.IsBeforeBuild = parameters.GetBool("beforeBuild");
        }
    }
}