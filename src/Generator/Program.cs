﻿// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using CppAst;

namespace Generator;

public static class Program
{
    public static int Main(string[] args)
    {
        string outputPath = AppContext.BaseDirectory;
        if (args.Length > 0)
        {
            outputPath = args[0];
        }

        if (!Path.IsPathRooted(outputPath))
        {
            outputPath = Path.Combine(AppContext.BaseDirectory, outputPath);
        }

        if (!outputPath.EndsWith("Generated"))
        {
            outputPath = Path.Combine(outputPath, "Generated");
        }

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        string? headerFile = Path.Combine(AppContext.BaseDirectory, "vulkan", "vulkan.h");
        var options = new CppParserOptions
        {
            ParseMacros = true,
            Defines =
                {
                    "VK_USE_PLATFORM_ANDROID_KHR",
                    "VK_USE_PLATFORM_IOS_MVK",
                    "VK_USE_PLATFORM_MACOS_MVK",
                    "VK_USE_PLATFORM_METAL_EXT",
                    "VK_USE_PLATFORM_VI_NN",
                    //"VK_USE_PLATFORM_WAYLAND_KHR",
                    //"VK_USE_PLATFORM_WIN32_KHR",
                    //"VK_USE_PLATFORM_SCREEN_QNX",
                    "VK_ENABLE_BETA_EXTENSIONS"
                }
        };

        var compilation = CppParser.ParseFile(headerFile, options);

        // Print diagnostic messages
        if (compilation.HasErrors)
        {
            foreach (var message in compilation.Diagnostics.Messages)
            {
                if (message.Type == CppLogMessageType.Error)
                {
                    var currentColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(message);
                    Console.ForegroundColor = currentColor;
                }
            }

            return 0;
        }

        bool generateFuncFile = false;
        if (generateFuncFile)
        {
            File.Delete("Vk.txt");
            foreach (var func in compilation.Functions)
            {
                var signature = new System.Text.StringBuilder();
                var argSignature = CsCodeGenerator.GetParameterSignature(func, true);
                signature
                    .Append(func.ReturnType.GetDisplayName())
                    .Append(" ")
                    .Append(func.Name)
                    .Append("(")
                    .Append(argSignature)
                    .Append(")");
                File.AppendAllText("Vk.txt", signature.ToString() + Environment.NewLine);
            }
        }

        CsCodeGenerator.Generate(compilation, outputPath);
        return 0;
    }

    private static IEnumerable<string> ResolveWindowsSdk(string version)
    {
        var path = @"C:\Program Files (x86)\Windows Kits\10";
        yield return $@"{path}\Include\{version}\shared";
        yield return $@"{path}\Include\{version}\um";
        yield return $@"{path}\Include\{version}\ucrt";
        yield return $@"{path}\Include\{version}\winrt";
    }
}
