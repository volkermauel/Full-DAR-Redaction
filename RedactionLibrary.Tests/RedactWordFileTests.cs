using RedactionLibrary;
using System;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;

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

        private static byte[] CreateDocumentWithTable(string caption, int rows)
        {
            MemoryStream ms = new();
            using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
            {
                var main = doc.AddMainDocumentPart();
                main.Document = new Document(new Body());
                var table = new Table();
                var props = new TableProperties();
                props.TableCaption = new TableCaption { Val = caption };
                table.AppendChild(props);
                for (int i = 0; i < rows; i++)
                {
                    var row = new TableRow(new TableCell(new Paragraph(new Run(new Text($"row{i}")))));
                    table.Append(row);
                }
                main.Document.Body.Append(table);
                doc.PackageProperties.Creator = "creator";
                doc.PackageProperties.LastModifiedBy = "editor";
                main.Document.Save();
            }
            return ms.ToArray();
        }

        private static byte[] CreateSimpleDocument(string text)
        {
            MemoryStream ms = new();
            using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
            {
                var main = doc.AddMainDocumentPart();
                main.Document = new Document(new Body(new Paragraph(new Run(new Text(text)))));
                main.Document.Save();
            }
            return ms.ToArray();
        }

        [Fact]
        public void Redact_RemovesWorkspaceTableFirstRow()
        {
            byte[] docBytes = CreateDocumentWithTable("Workspace", 2);
            var redactor = new RedactWordFile();
            byte[] result = redactor.Redact(docBytes)!;

            using var mem = new MemoryStream(result);
            using var doc = WordprocessingDocument.Open(mem, false);
            var props = doc.MainDocumentPart.Document.Descendants<TableProperties>().First();
            Assert.Equal("Redacted", props.TableCaption.Val);
            Assert.Equal(1, doc.MainDocumentPart.Document.Descendants<TableRow>().Count());
            Assert.Null(doc.PackageProperties.Creator);
            Assert.Null(doc.PackageProperties.LastModifiedBy);
        }

        [Fact]
        public void Redact_LeavesOtherTablesUntouched()
        {
            byte[] docBytes = CreateDocumentWithTable("Other", 1);
            var redactor = new RedactWordFile();
            byte[] result = redactor.Redact(docBytes)!;

            using var mem = new MemoryStream(result);
            using var doc = WordprocessingDocument.Open(mem, false);
            var props = doc.MainDocumentPart.Document.Descendants<TableProperties>().First();
            Assert.Equal("Other", props.TableCaption.Val);
            Assert.Equal(1, doc.MainDocumentPart.Document.Descendants<TableRow>().Count());
            Assert.Null(doc.PackageProperties.Creator);
            Assert.Null(doc.PackageProperties.LastModifiedBy);
        }

        [Fact]
        public void JoinWithoutQuality_MultipleArrays_JoinsAll()
        {
            var redactor = new RedactWordFile();
            byte[] a1 = CreateSimpleDocument("doc1");
            byte[] a2 = CreateSimpleDocument("doc2");
            byte[] a3 = CreateSimpleDocument("doc3");

            byte[] result = redactor.JoinWithoutQuality(a1, a2, a3, null, null, null)!;

            Assert.True(result.Length > a1.Length);
            using var mem = new MemoryStream(result);
            using var doc = WordprocessingDocument.Open(mem, false);
            Assert.Equal(2, doc.MainDocumentPart.Document.Descendants<AltChunk>().Count());
        }

        [Fact]
        public void JoinWithoutQuality_AllArrays_JoinsSixFiles()
        {
            var redactor = new RedactWordFile();
            byte[] a1 = CreateSimpleDocument("one");
            byte[] a2 = CreateSimpleDocument("two");
            byte[] a3 = CreateSimpleDocument("three");
            byte[] a4 = CreateSimpleDocument("four");
            byte[] a5 = CreateSimpleDocument("five");
            byte[] a6 = CreateSimpleDocument("six");

            byte[] result = redactor.JoinWithoutQuality(a1, a2, a3, a4, a5, a6)!;

            using var mem = new MemoryStream(result);
            using var doc = WordprocessingDocument.Open(mem, false);
            Assert.Equal(5, doc.MainDocumentPart.Document.Descendants<AltChunk>().Count());
        }

        [Fact]
        public void JoinTwoFiles_AltChunkContainsSecondDoc()
        {
            var redactor = new RedactWordFile();
            byte[] a1 = CreateSimpleDocument("first");
            byte[] a2 = CreateSimpleDocument("second");

            byte[] result = redactor.JoinTwoFiles(a1, a2);

            using var mem = new MemoryStream(result);
            using var doc = WordprocessingDocument.Open(mem, false);
            var alt = doc.MainDocumentPart.Document.Descendants<AltChunk>().Single();
            var altPart = (AlternativeFormatImportPart)doc.MainDocumentPart.GetPartById(alt.Id!);
            using var altDoc = WordprocessingDocument.Open(altPart.GetStream(), false);
            string text = altDoc.MainDocumentPart.Document.InnerText;
            Assert.Contains("second", text);
        }
    }
}
