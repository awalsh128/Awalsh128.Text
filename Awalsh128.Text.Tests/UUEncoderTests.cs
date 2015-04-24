namespace Awalsh128.Text.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using MbUnit.Framework;

    [TestFixture]
    [ApartmentState(System.Threading.ApartmentState.MTA)]
    public class UUEncoderTests
    {
        internal IEnumerable<object[]> EncodedDecodedTextPairs
        {
            get
            {
                yield return new object[] { "`", "" };
                yield return new object[] { "!9@``", "f" };
                yield return new object[] { "\"9F\\`", "fo" };
                yield return new object[] { "#9F]O", "foo" };
                yield return new object[] { "$9F]O(```", "foo " };
                yield return new object[] { "%9F]O(&(`", "foo b" };
                yield return new object[] { "&9F]O(&)A", "foo ba" };
                yield return new object[] { "'9F]O(&)A<@``", "foo bar" };
                yield return new object[] { "(9F]O(&)A<B``", "foo bar " };
                yield return new object[] { ")9F]O(&)A<B!B", "foo bar b" };
                yield return new object[] { "*9F]O(&)A<B!B80``", "foo bar ba" };
                yield return new object[] { "+9F]O(&)A<B!B87H`", "foo bar baz" };
                yield return new object[] { ",9F]O(&)A<B!B87H@", "foo bar baz " };
            }
        }

        [FixtureSetUp]
        public void TestFixtureSetUp()
        { }

        [FixtureTearDown]
        public void TestFixtureTearDown()
        { }

        [SetUp]
        public void SetUp()
        { }

        [TearDown]
        public void TearDown()
        { }

        [Test]
        [Factory("EncodedDecodedTextPairs")]
        public void DecodeLine(string encodedLineText, string expectedDecodedLineText)
        {
            using (var encodedStream = new MemoryStream(Encoding.ASCII.GetBytes(encodedLineText)))
            {
                byte[] actualDecodedLine = UUEncoder.DecodeLine(encodedStream);
                string acutalDecodedLineText = Encoding.ASCII.GetString(actualDecodedLine);
                Assert.AreElementsEqual(expectedDecodedLineText, acutalDecodedLineText);
            }
        }

        [Test]
        [Factory("EncodedDecodedTextPairs")]
        public void EncodeLine(string expectedEncodedLineText, string decodedLineText)
        {
            var decodedBuffer = Encoding.ASCII.GetBytes(decodedLineText);
            byte[] actualEncodedLine = UUEncoder.EncodeLine(decodedBuffer);
            string acutalEncodedLineText = Encoding.ASCII.GetString(actualEncodedLine);
            Assert.AreElementsEqual(expectedEncodedLineText, acutalEncodedLineText);
        }
    }
}