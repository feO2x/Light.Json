using System;
using System.Text;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Light.Json.Tests.Tokenization
{
    public sealed class Utf8TriangulationTests
    {
        private readonly ITestOutputHelper _output;
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public Utf8TriangulationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [MemberData(nameof(AsciiInputCharacters))]
        public void SingleByteUtf8Characters(string character)
        {
            var bytes = Encoding.UTF8.GetBytes(character);
            OutputBytes(character, bytes);

            bytes.Should().HaveCount(1);
            (bytes[0] & 0b1000_0000).Should().Be(0);
        }

        private void OutputBytes(string character, byte[] bytes)
        {
            _stringBuilder.Clear();
            foreach (var @byte in bytes)
            {
                var stringRepresentation = Convert.ToString(@byte, 2).PadLeft(8, '0');
                _stringBuilder.Append(stringRepresentation).Append(' ');
            }

            _output.WriteLine("Character \"" + character + "\" in bytes:");
            _output.WriteLine(_stringBuilder.ToString());
        }

        public static TheoryData<string> AsciiInputCharacters
        {
            get
            {
                var theoryData = new TheoryData<string>();

                for (char character = default; character < 128; ++character)
                {
                    theoryData.Add(new string(character, 1));
                }

                return theoryData;
            }
        }
    }
}