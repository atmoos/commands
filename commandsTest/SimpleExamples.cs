using System;
using System.Text.Json;
using commands;
using commands.extensions;
using commands.tools;

namespace commandsTest
{
    internal sealed class SimpleExamples
    {
        public ICommand<Byte[], Addition> CreateCommandFormMethod()
        {
            static Addition SerializeMethod(Byte[] json)
            {
                return JsonSerializer.Deserialize<Addition>(json);
            }

            return FuncExtensions.ToCommand<Byte[], Addition>(SerializeMethod);
        }

        public IBuilder<Addition> AddCommandToBuilder(IBuilder<Byte[]> builder)
        {
            static Addition SerializeMethod(Byte[] json)
            {
                return JsonSerializer.Deserialize<Addition>(json);
            }

            return builder.Add(SerializeMethod);
        }
    }
}