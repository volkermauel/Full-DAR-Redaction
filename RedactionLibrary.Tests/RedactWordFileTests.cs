using RedactionLibrary;
using System;

namespace RedactionLibrary.Tests
{
    public class RedactWordFileTests
    {
        [Fact]
        public void JoinWithoutQuality_AllNull_ReturnsNull()
        {
            var redactor = new RedactWordFile();
            var result = redactor.JoinWithoutQuality(null, null, null, null, null, null);
            Assert.Null(result);
        }

        [Fact]
        public void JoinWithoutQuality_SingleArray_ReturnsInput()
        {
            var redactor = new RedactWordFile();
            byte[] data = new byte[] {1,2,3};
            var result = redactor.JoinWithoutQuality(data, null, null, null, null, null);
            Assert.Same(data, result);
        }

        [Fact]
        public void Redact_NullInput_ReturnsNull()
        {
            var redactor = new RedactWordFile();
            var result = redactor.Redact(null!);
            Assert.Null(result);
        }
    }
}
