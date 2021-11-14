using System;
using System.IO;
using System.Text.Json;
using commands;
using commands.extensions;
using commands.tools;

namespace commandsTest
{
    public static class Examples
    {
        public static ICommand EvaluateFromFileSystem(String fileToRead, String pathToPersistTo)
        {
            var builder = ((Func<FileInfo>)(() => GetFileToProcess(fileToRead))).StartBuilder();
            var completedBuilder = builder.Add(ReadFile)
                                          .AddComputationEngine()
                                          .Add(content => PersistToFile(pathToPersistTo, content));
            return completedBuilder.Build();
        }
        public static ICommand EvaluateFromRemoteLocation(Uri dataSource, Uri resultTarget)
        {
            var builder = ((Func<Byte[]>)(() => ReadFrom(dataSource))).StartBuilder();
            var completedBuilder = builder.AddComputationEngine()
                                          .Add(content => UploadTo(resultTarget, content));
            return completedBuilder.Build();
        }
        public static IBuilder<Byte[]> AddComputationEngine(this IBuilder<Byte[]> builder)
        {
            return builder.Add(Deserialize).Add(Parse).Add(Compute).Add(Serialize);
        }
        private static FileInfo GetFileToProcess(String path)
        {
            return new FileInfo(path);
        }

        private static Byte[] ReadFile(FileInfo file)
        {
            return File.ReadAllBytes(file.FullName);
        }
        private static Byte[] ReadFrom(Uri remote)
        {
            // ToDo: get content from remote uri
            return new Byte[0];
        }

        private static Addition Deserialize(Byte[] json)
        {
            return JsonSerializer.Deserialize<Addition>(json);
        }

        private static (Addition, BinaryOperation) Parse(Addition expression)
        {
            static BinaryOperation Lex(String binaryOperation) => binaryOperation switch
            {
                "+" => new(binaryOperation, (l, r) => l + r),
                "-" => new(binaryOperation, (l, r) => l - r),
                "*" => new(binaryOperation, (l, r) => l * r),
                "/" => new(binaryOperation, (l, r) => l / r),
                _ => throw new NotSupportedException($"'{binaryOperation}' is not supported")
            };
            return (expression, Lex(expression.Operation));
        }

        private static Result Compute((Addition e, BinaryOperation op) operation)
        {
            var left = operation.e.LeftOperand;
            var right = operation.e.RightOperand;
            var result = operation.op.Evaluate(left, right);
            var text = $"{left}{operation.op.Symbol}{right}={result}";
            return new() { Expression = text, Value = result };
        }

        private static Byte[] Serialize(Result result)
        {
            return JsonSerializer.SerializeToUtf8Bytes(result);
        }

        private static void PersistToFile(String path, Byte[] content)
        {
            File.WriteAllBytes(path, content);
        }
        private static void UploadTo(Uri remote, Byte[] content)
        {
            // ToDo: upload content to remote uri
        }
    }

    internal sealed class Addition
    {
        public Double LeftOperand { get; set; }
        public String Operation { get; set; }
        public Double RightOperand { get; set; }
    }

    internal sealed class BinaryOperation
    {
        private readonly Func<Double, Double, Double> operation;
        public String Symbol { get; }

        public BinaryOperation(String symbol, Func<Double, Double, Double> operation)
        {
            Symbol = symbol;
            this.operation = operation;
        }

        public Double Evaluate(Double leftOperand, Double rightOperand)
        {
            return this.operation(leftOperand, rightOperand);
        }
    }

    internal sealed class Result
    {
        public String Expression { get; set; }
        public Double Value { get; set; }
    }
}